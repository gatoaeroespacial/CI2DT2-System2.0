using System.Net;
using Ci2dt2.Application.Common;
using Ci2dt2.Application.Seguridad;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ci2dt2.Application.Notificaciones;

public sealed class NotificacionItem
{
    public int Id { get; init; }
    public string Tipo { get; init; } = "";
    public string Titulo { get; init; } = "";
    public string Mensaje { get; init; } = "";
    public string? ReferenciaTipo { get; init; }
    public int? ReferenciaId { get; init; }
    public bool Leida { get; init; }
    public DateTime Fecha { get; init; }

    /// <summary>Ruta del recurso relacionado en el frontend (HU-22 CA-2).</summary>
    public string? Ruta { get; init; }
}

public sealed record BandejaNotificaciones(Pagina<NotificacionItem> Pagina, int NoLeidas);

/// <summary>URL pública del frontend, para los enlaces de los correos (HU-23 y HU-03).</summary>
public sealed class OpcionesFrontend
{
    public const string Seccion = "Frontend";

    public string Url { get; set; } = "http://localhost:3000";
}

/// <summary>Cola de correos directos que no son notificaciones de la bandeja (p. ej. restablecer contraseña).</summary>
public interface IColaCorreos
{
    void Encolar(string para, string asunto, string cuerpoHtml);
}

/// <summary>HU-22: centro de notificaciones. Cada usuario solo ve las suyas.</summary>
public sealed class NotificacionesService(IApplicationDbContext db, IDbConnectionFactory conexiones)
{
    public const int TamanoPagina = 20;

    public async Task<BandejaNotificaciones> MiasAsync(int usuarioId, bool soloNoLeidas, int pagina, CancellationToken ct = default)
    {
        pagina = Math.Max(1, pagina);
        const string sql = """
            SELECT COUNT(*) FROM Notificaciones WHERE UsuarioId = @usuarioId AND Activo = 1 AND (@soloNoLeidas = 0 OR Leida = 0);
            SELECT COUNT(*) FROM Notificaciones WHERE UsuarioId = @usuarioId AND Activo = 1 AND Leida = 0;
            SELECT n.Id, n.Tipo, n.Titulo, n.Mensaje, n.ReferenciaTipo, n.ReferenciaId, n.Leida, n.CreadoEn AS Fecha,
                   CASE n.ReferenciaTipo
                        WHEN N'Proyecto' THEN CONCAT(N'/proyectos/', n.ReferenciaId)
                        WHEN N'Programa' THEN CONCAT(N'/programas/', n.ReferenciaId)
                        WHEN N'Convocatoria' THEN CONCAT(N'/convocatorias/', n.ReferenciaId)
                        WHEN N'Hito' THEN CONCAT(N'/proyectos/', c.ProyectoId)
                   END AS Ruta
            FROM Notificaciones n
            LEFT JOIN Hitos h ON n.ReferenciaTipo = N'Hito' AND h.Id = n.ReferenciaId
            LEFT JOIN Cronogramas c ON c.Id = h.CronogramaId
            WHERE n.UsuarioId = @usuarioId AND n.Activo = 1 AND (@soloNoLeidas = 0 OR n.Leida = 0)
            ORDER BY n.CreadoEn DESC, n.Id DESC
            OFFSET @salto ROWS FETCH NEXT @tamano ROWS ONLY;
            """;
        using var conexion = conexiones.Crear();
        using var r = await conexion.QueryMultipleAsync(new CommandDefinition(sql,
            new { usuarioId, soloNoLeidas, salto = (pagina - 1) * TamanoPagina, tamano = TamanoPagina }, cancellationToken: ct));
        var total = await r.ReadSingleAsync<int>();
        var noLeidas = await r.ReadSingleAsync<int>();
        var items = (await r.ReadAsync<NotificacionItem>()).ToList();
        return new BandejaNotificaciones(new Pagina<NotificacionItem>(items, total, pagina, TamanoPagina), noLeidas);
    }

    public async Task<int> ContarNoLeidasAsync(int usuarioId, CancellationToken ct = default) =>
        await db.Notificaciones.CountAsync(n => n.UsuarioId == usuarioId && n.Activo && !n.Leida, ct);

    public async Task LeerAsync(int id, int usuarioId, CancellationToken ct = default)
    {
        var notificacion = await db.Notificaciones.FirstOrDefaultAsync(n => n.Id == id && n.UsuarioId == usuarioId && n.Activo, ct)
            ?? throw new NoEncontradoException("La notificación no existe.");
        notificacion.Leida = true;
        await db.SaveChangesAsync(ct);
    }

    public async Task LeerTodasAsync(int usuarioId, CancellationToken ct = default)
    {
        var pendientes = await db.Notificaciones.Where(n => n.UsuarioId == usuarioId && n.Activo && !n.Leida).ToListAsync(ct);
        foreach (var n in pendientes)
            n.Leida = true;
        await db.SaveChangesAsync(ct);
    }
}

/// <summary>
/// HU-23: despacho asíncrono de las notificaciones por correo. Primero se registra la notificación
/// y luego este despachador la envía; hasta 3 reintentos con espera creciente (1, 2 y 4 minutos).
/// </summary>
public sealed class DespachadorCorreos(
    IApplicationDbContext db,
    IEmailSender correo,
    IOptions<OpcionesFrontend> frontend,
    TimeProvider reloj,
    ILogger<DespachadorCorreos> logger)
{
    public const int MaxIntentos = 3;
    private const int Lote = 25;

    public async Task<int> ProcesarPendientesAsync(CancellationToken ct = default)
    {
        var ahora = reloj.GetUtcNow().UtcDateTime;
        var candidatas = await db.Notificaciones
            .Include(n => n.Usuario)
            .Where(n => n.Activo && !n.EnviadaEmail && (n.Intentos ?? 0) < MaxIntentos)
            .OrderBy(n => n.Id)
            .Take(Lote * 2)
            .ToListAsync(ct);

        var lote = candidatas.Where(n => ListaParaIntento(n.Intentos ?? 0, n.ActualizadoEn, ahora)).Take(Lote).ToList();
        // Los hitos enlazan a la ficha de su proyecto.
        var idsHitos = lote.Where(n => n.ReferenciaTipo == "Hito" && n.ReferenciaId is not null).Select(n => n.ReferenciaId!.Value).ToList();
        var proyectoDeHito = idsHitos.Count == 0 ? [] : await db.Hitos
            .Where(h => idsHitos.Contains(h.Id)).Select(h => new { h.Id, h.Cronograma!.ProyectoId })
            .ToDictionaryAsync(h => h.Id, h => h.ProyectoId, ct);

        var enviadas = 0;
        foreach (var n in lote)
        {
            if (n.Usuario is not { TieneAcceso: true } usuario)
            {
                // Usuario inactivo: no se reintenta.
                n.Intentos = MaxIntentos;
                n.UltimoError = "El usuario está inactivo.";
                continue;
            }

            try
            {
                await correo.EnviarAsync(usuario.Email, $"CI²DT² · {n.Titulo}",
                    PlantillaCorreo.Html(n.Titulo, n.Mensaje, n.ReferenciaTipo == "Hito"
                        ? EnlaceRecurso("Proyecto", n.ReferenciaId is { } h ? proyectoDeHito.GetValueOrDefault(h) : null)
                        : EnlaceRecurso(n.ReferenciaTipo, n.ReferenciaId)), ct);
                n.EnviadaEmail = true;
                n.FechaEnvioEmail = reloj.GetUtcNow().UtcDateTime;
                n.UltimoError = null;
                enviadas++;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // CA-2: el fallo del SMTP no afecta la operación del usuario; queda en UltimoError.
                n.Intentos = (n.Intentos ?? 0) + 1;
                n.UltimoError = ex.Message.Length > 300 ? ex.Message[..300] : ex.Message;
                logger.LogWarning(ex, "Falló el envío de la notificación {Id} (intento {Intento})", n.Id, n.Intentos);
            }
        }

        await db.SaveChangesAsync(ct);
        return enviadas;
    }

    /// <summary>Espera creciente entre reintentos: 1, 2 y 4 minutos después del último fallo.</summary>
    public static bool ListaParaIntento(int intentos, DateTime? ultimoCambio, DateTime ahora) =>
        intentos == 0 || ultimoCambio is null || ahora >= ultimoCambio.Value.AddMinutes(Math.Pow(2, intentos - 1));

    private string? EnlaceRecurso(string? tipo, int? id)
    {
        var ruta = RutaRecurso(tipo, id);
        return ruta is null ? null : frontend.Value.Url.TrimEnd('/') + ruta;
    }

    /// <summary>Ruta del recurso en el frontend (también la usa la bandeja, HU-22 CA-2).</summary>
    public static string? RutaRecurso(string? tipo, int? id) => (tipo, id) switch
    {
        ("Proyecto", { } i) => $"/proyectos/{i}",
        ("Programa", { } i) => $"/programas/{i}",
        ("Convocatoria", { } i) => $"/convocatorias/{i}",
        _ => null,
    };
}

/// <summary>Plantilla HTML de los correos con la identidad del CI²DT² y un enlace directo al recurso.</summary>
public static class PlantillaCorreo
{
    public static string Html(string titulo, string mensaje, string? enlace, string textoEnlace = "Ver en la plataforma")
    {
        var boton = enlace is null
            ? ""
            : $"""<p style="margin:24px 0"><a href="{WebUtility.HtmlEncode(enlace)}" style="background:#065f46;color:#ffffff;padding:10px 18px;border-radius:6px;text-decoration:none;font-weight:600">{WebUtility.HtmlEncode(textoEnlace)}</a></p>""";
        return $"""
            <!doctype html>
            <html lang="es"><body style="margin:0;background:#f1f5f9;font-family:Arial,Helvetica,sans-serif;color:#0f172a">
            <table role="presentation" width="100%" cellpadding="0" cellspacing="0"><tr><td align="center" style="padding:24px">
            <table role="presentation" width="600" cellpadding="0" cellspacing="0" style="max-width:600px;background:#ffffff;border-radius:8px;overflow:hidden">
            <tr><td style="background:#065f46;color:#ffffff;padding:16px 24px;font-size:18px;font-weight:700">CI²DT² · Universidad de Caldas</td></tr>
            <tr><td style="padding:24px">
            <h1 style="font-size:18px;margin:0 0 12px">{WebUtility.HtmlEncode(titulo)}</h1>
            <p style="font-size:14px;line-height:1.5;margin:0;white-space:pre-line">{WebUtility.HtmlEncode(mensaje)}</p>
            {boton}
            <p style="font-size:12px;color:#64748b;margin:24px 0 0">Este es un mensaje automático del sistema del Centro de Investigación, Innovación, Desarrollo y Transferencia de Tecnología. No responda a este correo.</p>
            </td></tr></table></td></tr></table></body></html>
            """;
    }
}
