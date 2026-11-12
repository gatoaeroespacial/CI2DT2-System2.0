using Ci2dt2.Application.Common;
using Ci2dt2.Application.Proyectos;
using Ci2dt2.Application.Seguridad;
using Ci2dt2.Domain;
using Ci2dt2.Domain.Entities;
using Dapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Ci2dt2.Application.Cronogramas;

public static class EstadosHito
{
    public const string Pendiente = "Pendiente";
    public const string EnCurso = "En curso";
    public const string Completado = "Completado";
    public const string Vencido = "Vencido";
}

public sealed record HitoRequest(int TipoHitoId, string Titulo, DateOnly? FechaLimite, int? ResponsableId, int? AlertarDias, string? Justificacion);

public sealed record CompletarHitoRequest(int? AvanceId);

public sealed record HitoGuardado(int Id, string? Advertencia);

public sealed class HitoDetalle
{
    public int Id { get; init; }
    public int TipoHitoId { get; init; }
    public string TipoHito { get; init; } = "";
    public string Titulo { get; init; } = "";
    public DateTime FechaLimite { get; init; }
    public int? AlertarDias { get; init; }
    public int? ResponsableId { get; init; }
    public string? Responsable { get; init; }
    public string Estado { get; init; } = "";
    public DateTime? FechaCompletado { get; init; }
    public int AvancesVinculados { get; init; }
}

public sealed class HitoRequestValidator : AbstractValidator<HitoRequest>
{
    public HitoRequestValidator()
    {
        RuleFor(x => x.TipoHitoId).GreaterThan(0).WithMessage("Debe seleccionar el tipo de hito.");
        RuleFor(x => x.Titulo).NotEmpty().WithMessage("El título es obligatorio.").MaximumLength(200);
        RuleFor(x => x.FechaLimite).NotNull().WithMessage("La fecha límite es obligatoria.");
        RuleFor(x => x.ResponsableId).NotNull().GreaterThan(0).WithMessage("Debe seleccionar el responsable.");
        RuleFor(x => x.AlertarDias).InclusiveBetween(1, 60).WithMessage("Los días de anticipación de la alerta deben estar entre 1 y 60.");
        RuleFor(x => x.Justificacion).MaximumLength(500);
    }
}

/// <summary>HU-17: definir, completar y reprogramar los hitos del proyecto.</summary>
public sealed class HitosService(
    IApplicationDbContext db,
    IDbConnectionFactory conexiones,
    AccesoRecursos acceso,
    Notificador notificador,
    Auditoria auditoria,
    TimeProvider reloj)
{
    public const int AlertarDiasPorDefecto = 7;
    private DateOnly Hoy => DateOnly.FromDateTime(reloj.GetUtcNow().UtcDateTime);

    public async Task<IReadOnlyList<HitoDetalle>> ListarAsync(int proyectoId, CancellationToken ct = default)
    {
        using var conexion = conexiones.Crear();
        return (await conexion.QueryAsync<HitoDetalle>(new CommandDefinition("""
            SELECT h.Id, h.TipoHitoId, th.Nombre AS TipoHito, h.Titulo, CAST(h.FechaLimite AS datetime2) AS FechaLimite,
                   h.AlertarDias, h.ResponsableId, u.Nombres + ' ' + u.Apellidos AS Responsable, h.Estado,
                   CAST(h.FechaCompletado AS datetime2) AS FechaCompletado,
                   (SELECT COUNT(*) FROM Avances a WHERE a.HitoId = h.Id AND a.Activo = 1) AS AvancesVinculados
            FROM Hitos h
            JOIN Cronogramas c ON c.Id = h.CronogramaId
            JOIN TiposHito th ON th.Id = h.TipoHitoId
            LEFT JOIN Usuarios u ON u.Id = h.ResponsableId
            WHERE c.ProyectoId = @proyectoId AND c.Activo = 1 AND h.Activo = 1
            ORDER BY h.FechaLimite, h.Orden, h.Id
            """, new { proyectoId }, cancellationToken: ct))).ToList();
    }

    public async Task<HitoGuardado> CrearAsync(int proyectoId, HitoRequest r, UsuarioAutenticado actor, CancellationToken ct = default)
    {
        var proyecto = await ValidarGestionAsync(proyectoId, actor, ct);
        var cronograma = await db.Cronogramas.FirstOrDefaultAsync(c => c.ProyectoId == proyectoId && c.Activo, ct)
            ?? throw new ReglaNegocioException("El proyecto aún no tiene cronograma: se crea al registrar su aprobación.");
        await ValidarAsync(proyectoId, r, ct);

        var orden = await db.Hitos.Where(h => h.CronogramaId == cronograma.Id && h.Activo).MaxAsync(h => (int?)h.Orden, ct) ?? 0;
        var hito = new Hito
        {
            CronogramaId = cronograma.Id,
            TipoHitoId = r.TipoHitoId,
            Titulo = r.Titulo.Trim(),
            FechaLimite = r.FechaLimite!.Value,
            ResponsableId = r.ResponsableId,
            AlertarDias = r.AlertarDias ?? AlertarDiasPorDefecto,
            Estado = EstadosHito.Pendiente,
            Orden = orden + 1,
        };
        db.Hitos.Add(hito);
        await db.SaveChangesAsync(ct);
        return new HitoGuardado(hito.Id, AdvertenciaVigencia(proyecto, hito.FechaLimite));
    }

    public async Task<HitoGuardado> ActualizarAsync(int hitoId, HitoRequest r, UsuarioAutenticado actor, CancellationToken ct = default)
    {
        var hito = await CargarAsync(hitoId, ct);
        var proyecto = await ValidarGestionAsync(hito.Cronograma!.ProyectoId!.Value, actor, ct);
        if (hito.Estado == EstadosHito.Completado)
            throw new ReglaNegocioException("El hito ya está completado y no admite cambios.");
        await ValidarAsync(proyecto.Id, r, ct);

        var fechaAnterior = hito.FechaLimite;
        var nuevaFecha = r.FechaLimite!.Value;
        if (nuevaFecha != fechaAnterior)
        {
            // CA-5: reprogramar exige justificación; las fechas quedan en el log y se notifica al Coordinador.
            if (string.IsNullOrWhiteSpace(r.Justificacion))
                throw new ReglaNegocioException("Para cambiar la fecha límite escriba la justificación de la reprogramación.");
            auditoria.Registrar("hitos", "REPROGRAMA", hito.Id.ToString(), 200,
                new { fechaLimite = fechaAnterior }, new { fechaLimite = nuevaFecha, justificacion = r.Justificacion.Trim() });
            var coordinador = await notificador.CoordinadorVigenteAsync(proyecto.ProgramaId, ct);
            if (coordinador is { } c)
                notificador.Notificar(c, "HitoReprogramado", $"Hito reprogramado en {proyecto.Codigo}",
                    $"El hito \"{hito.Titulo}\" pasó del {fechaAnterior:dd/MM/yyyy} al {nuevaFecha:dd/MM/yyyy}. Justificación: {r.Justificacion.Trim()}",
                    "Hito", hito.Id);
            if (hito.Estado == EstadosHito.Vencido && nuevaFecha >= Hoy)
                hito.Estado = EstadosHito.Pendiente;
        }

        hito.TipoHitoId = r.TipoHitoId;
        hito.Titulo = r.Titulo.Trim();
        hito.FechaLimite = nuevaFecha;
        hito.ResponsableId = r.ResponsableId;
        hito.AlertarDias = r.AlertarDias ?? hito.AlertarDias ?? AlertarDiasPorDefecto;
        await db.SaveChangesAsync(ct);
        return new HitoGuardado(hito.Id, AdvertenciaVigencia(proyecto, nuevaFecha));
    }

    /// <summary>"En curso" lo marca el responsable al empezar a trabajar el hito.</summary>
    public async Task IniciarAsync(int hitoId, UsuarioAutenticado actor, CancellationToken ct = default)
    {
        var hito = await CargarAsync(hitoId, ct);
        await ValidarResponsableOGestorAsync(hito, actor, ct);
        if (hito.Estado != EstadosHito.Pendiente)
            throw new ReglaNegocioException("Solo un hito pendiente puede marcarse como \"En curso\".");
        hito.Estado = EstadosHito.EnCurso;
        await db.SaveChangesAsync(ct);
    }

    /// <summary>CA-4: completar registra la fecha del día y permite vincular un avance como evidencia.</summary>
    public async Task CompletarAsync(int hitoId, CompletarHitoRequest r, UsuarioAutenticado actor, CancellationToken ct = default)
    {
        var hito = await CargarAsync(hitoId, ct);
        await ValidarResponsableOGestorAsync(hito, actor, ct);
        if (hito.Estado == EstadosHito.Completado)
            throw new ReglaNegocioException("El hito ya está completado.");

        if (r.AvanceId is { } avanceId)
        {
            var avance = await db.Avances.FirstOrDefaultAsync(a => a.Id == avanceId && a.ProyectoId == hito.Cronograma!.ProyectoId && a.Activo, ct)
                ?? throw new ReglaNegocioException("El avance de evidencia no pertenece al proyecto.");
            avance.HitoId = hito.Id;
        }

        hito.Estado = EstadosHito.Completado;
        hito.FechaCompletado = Hoy;
        await db.SaveChangesAsync(ct);
    }

    // ----- Reglas -----

    /// <summary>Anexo A, "Gestionar hitos": Admin y Director; Coordinador su programa; Investigador si es IP.</summary>
    private async Task<Proyecto> ValidarGestionAsync(int proyectoId, UsuarioAutenticado actor, CancellationToken ct)
    {
        var proyecto = await db.Proyectos.Include(p => p.Estado).FirstOrDefaultAsync(p => p.Id == proyectoId && p.Activo, ct)
            ?? throw new NoEncontradoException("El proyecto no existe.");
        if (!actor.Roles.Any(r => Politicas.Matriz[Politicas.GestionarHitos].Contains(r))
            || !await acceso.PuedeModificarProyectoComoIpAsync(actor, proyectoId, ct))
            throw new AccesoDenegadoException();
        if (proyecto.Estado!.EsFinal)
            throw new ReglaNegocioException($"El proyecto está en estado \"{proyecto.Estado.Nombre}\" y no admite cambios.");
        return proyecto;
    }

    private async Task ValidarResponsableOGestorAsync(Hito hito, UsuarioAutenticado actor, CancellationToken ct)
    {
        if (hito.ResponsableId == actor.Id && await acceso.EsIntegranteProyectoAsync(actor.Id, hito.Cronograma!.ProyectoId!.Value, ct))
            return;
        await ValidarGestionAsync(hito.Cronograma!.ProyectoId!.Value, actor, ct);
    }

    private async Task ValidarAsync(int proyectoId, HitoRequest r, CancellationToken ct)
    {
        if (!await db.TiposHito.AnyAsync(t => t.Id == r.TipoHitoId && t.Activo, ct))
            throw new ReglaNegocioException("El tipo de hito no existe.");
        // CA-2: el responsable debe ser un integrante vigente del proyecto.
        if (!await acceso.EsIntegranteProyectoAsync(r.ResponsableId!.Value, proyectoId, ct))
            throw new ReglaNegocioException("El responsable debe ser un integrante vigente del proyecto.");
    }

    private static string? AdvertenciaVigencia(Proyecto p, DateOnly fecha) =>
        (p.FechaInicio is { } inicio && fecha < inicio) || (p.FechaFin is { } fin && fecha > fin)
            ? "La fecha límite queda fuera de la vigencia del proyecto."
            : null;

    private async Task<Hito> CargarAsync(int hitoId, CancellationToken ct) =>
        await db.Hitos.Include(h => h.Cronograma).FirstOrDefaultAsync(h => h.Id == hitoId && h.Activo && h.Cronograma!.ProyectoId != null, ct)
        ?? throw new NoEncontradoException("El hito no existe.");
}

public sealed record ResultadoAlertas(int Proximos, int VencenHoy, int Vencidos, int Notificaciones);

public sealed class AlertaHito
{
    public int HitoId { get; init; }
    public string Titulo { get; init; } = "";
    public DateTime FechaLimite { get; init; }
    public string Estado { get; init; } = "";
    public int ProyectoId { get; init; }
    public string? Codigo { get; init; }
    public string Proyecto { get; init; } = "";
    public string? Programa { get; init; }
    public string? Responsable { get; init; }
}

/// <summary>
/// HU-18: motor de alertas de hitos. Idempotente: si se ejecuta varias veces el mismo día no
/// duplica notificaciones (cada alerta se registra una sola vez por hito y destinatario).
/// </summary>
public sealed class AlertasService(IApplicationDbContext db, IDbConnectionFactory conexiones, Notificador notificador, TimeProvider reloj)
{
    public const string TipoProximo = "HitoProximo";
    public const string TipoVenceHoy = "HitoVenceHoy";
    public const string TipoVencido = "HitoVencido";

    /// <summary>Fecha de hoy en Colombia (UTC−5, sin horario de verano).</summary>
    public DateOnly HoyColombia() => DateOnly.FromDateTime(reloj.GetUtcNow().UtcDateTime.AddHours(-5));

    public async Task<ResultadoAlertas> EjecutarAsync(DateOnly hoy, CancellationToken ct = default)
    {
        var hitos = await db.Hitos
            .Include(h => h.Cronograma!).ThenInclude(c => c.Proyecto)
            .Where(h => h.Activo && (h.Estado == EstadosHito.Pendiente || h.Estado == EstadosHito.EnCurso)
                && h.Cronograma!.Activo && h.Cronograma.Proyecto != null && h.Cronograma.Proyecto.Activo)
            .ToListAsync(ct);

        var ids = hitos.Select(h => h.Id).ToList();
        var yaEnviadas = (await db.Notificaciones
                .Where(n => n.ReferenciaTipo == "Hito" && n.ReferenciaId != null && ids.Contains(n.ReferenciaId.Value)
                    && (n.Tipo == TipoProximo || n.Tipo == TipoVenceHoy || n.Tipo == TipoVencido))
                .Select(n => new { n.Tipo, Hito = n.ReferenciaId!.Value, n.UsuarioId }).ToListAsync(ct))
            .Select(n => (n.Tipo, n.Hito, n.UsuarioId)).ToHashSet();

        int proximos = 0, hoyVencen = 0, vencidos = 0, total = 0;
        foreach (var hito in hitos)
        {
            var proyecto = hito.Cronograma!.Proyecto!;
            var destinatarios = new List<int> { proyecto.InvestigadorPrincipalId };
            if (hito.ResponsableId is { } resp)
                destinatarios.Add(resp);

            string tipo;
            string titulo;
            string mensaje;
            if (hito.FechaLimite < hoy)
            {
                // Vencido: cambia de estado y se avisa también al Coordinador.
                hito.Estado = EstadosHito.Vencido;
                if (await notificador.CoordinadorVigenteAsync(proyecto.ProgramaId, ct) is { } coord)
                    destinatarios.Add(coord);
                (tipo, titulo, mensaje) = (TipoVencido, $"Hito vencido en {proyecto.Codigo}",
                    $"El hito \"{hito.Titulo}\" del proyecto \"{proyecto.Titulo}\" venció el {hito.FechaLimite:dd/MM/yyyy}.");
                vencidos++;
            }
            else if (hito.FechaLimite == hoy)
            {
                (tipo, titulo, mensaje) = (TipoVenceHoy, $"Hito vence hoy en {proyecto.Codigo}",
                    $"El hito \"{hito.Titulo}\" del proyecto \"{proyecto.Titulo}\" vence hoy.");
                hoyVencen++;
            }
            else if (hoy >= hito.FechaLimite.AddDays(-(hito.AlertarDias ?? HitosService.AlertarDiasPorDefecto)))
            {
                (tipo, titulo, mensaje) = (TipoProximo, $"Hito próximo a vencer en {proyecto.Codigo}",
                    $"El hito \"{hito.Titulo}\" del proyecto \"{proyecto.Titulo}\" vence el {hito.FechaLimite:dd/MM/yyyy}.");
                proximos++;
            }
            else
            {
                continue;
            }

            foreach (var usuario in destinatarios.Distinct())
            {
                if (!yaEnviadas.Add((tipo, hito.Id, usuario)))
                    continue;
                notificador.Notificar(usuario, tipo, titulo, mensaje, "Hito", hito.Id);
                total++;
            }
        }

        await db.SaveChangesAsync(ct);
        return new ResultadoAlertas(proximos, hoyVencen, vencidos, total);
    }

    /// <summary>CA-4 "Mis alertas": hitos próximos (30 días) y vencidos de los proyectos del usuario.</summary>
    public async Task<IReadOnlyList<AlertaHito>> MisAlertasAsync(int usuarioId, CancellationToken ct = default)
    {
        var hoy = HoyColombia().ToDateTime(TimeOnly.MinValue);
        using var conexion = conexiones.Crear();
        return (await conexion.QueryAsync<AlertaHito>(new CommandDefinition("""
            SELECT h.Id AS HitoId, h.Titulo, CAST(h.FechaLimite AS datetime2) AS FechaLimite, h.Estado,
                   p.Id AS ProyectoId, p.Codigo, p.Titulo AS Proyecto, pr.Nombre AS Programa,
                   u.Nombres + ' ' + u.Apellidos AS Responsable
            FROM Hitos h
            JOIN Cronogramas c ON c.Id = h.CronogramaId AND c.Activo = 1
            JOIN Proyectos p ON p.Id = c.ProyectoId AND p.Activo = 1
            JOIN Programas pr ON pr.Id = p.ProgramaId
            LEFT JOIN Usuarios u ON u.Id = h.ResponsableId
            WHERE h.Activo = 1
              AND (h.Estado = N'Vencido' OR (h.Estado IN (N'Pendiente', N'En curso') AND h.FechaLimite <= DATEADD(DAY, 30, @hoy)))
              AND (h.ResponsableId = @usuarioId OR p.InvestigadorPrincipalId = @usuarioId
                   OR EXISTS (SELECT 1 FROM ProyectoIntegrantes pi WHERE pi.ProyectoId = p.Id AND pi.UsuarioId = @usuarioId
                              AND pi.Activo = 1 AND (pi.FechaFin IS NULL OR pi.FechaFin > @hoy))
                   OR EXISTS (SELECT 1 FROM ProgramaIntegrantes g WHERE g.ProgramaId = p.ProgramaId AND g.UsuarioId = @usuarioId
                              AND g.Activo = 1 AND g.RolEnPrograma = N'Coordinador'
                              AND (g.FechaInicio IS NULL OR g.FechaInicio <= @hoy) AND (g.FechaFin IS NULL OR g.FechaFin > @hoy)))
            ORDER BY h.FechaLimite
            """, new { usuarioId, hoy }, cancellationToken: ct))).ToList();
    }
}
