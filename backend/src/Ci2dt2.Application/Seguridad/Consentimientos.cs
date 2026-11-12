using Ci2dt2.Application.Common;
using Ci2dt2.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ci2dt2.Application.Seguridad;

public sealed record EstadoConsentimiento(int? PoliticaId, string? Version, string? TextoUrl, bool Aceptado);

/// <summary>
/// P17 (parcial) y Ley 1581 de 2012: aceptación de la política de tratamiento de datos vigente.
/// Los consentimientos son inmutables (sin UPDATE ni DELETE).
/// </summary>
public sealed class ConsentimientosService(IApplicationDbContext db, IContextoPeticion contexto, TimeProvider reloj)
{
    public const string ContextoRegistro = "Registro";

    public async Task<EstadoConsentimiento> EstadoAsync(int usuarioId, CancellationToken ct = default)
    {
        var politica = await PoliticaVigenteAsync(ct);
        if (politica is null)
            return new EstadoConsentimiento(null, null, null, true);
        var aceptado = await db.ConsentimientosDatos.AnyAsync(c => c.UsuarioId == usuarioId && c.PoliticaId == politica.Id && c.Aceptado, ct);
        return new EstadoConsentimiento(politica.Id, politica.Version, politica.TextoUrl, aceptado);
    }

    public async Task AceptarAsync(int usuarioId, CancellationToken ct = default)
    {
        var politica = await PoliticaVigenteAsync(ct) ?? throw new ReglaNegocioException("No hay una política de tratamiento de datos vigente.");
        if (await db.ConsentimientosDatos.AnyAsync(c => c.UsuarioId == usuarioId && c.PoliticaId == politica.Id && c.Aceptado, ct))
            return;
        var usuario = await db.Usuarios.AsNoTracking().FirstAsync(u => u.Id == usuarioId, ct);
        var fecha = reloj.GetUtcNow().UtcDateTime;
        db.ConsentimientosDatos.Add(new ConsentimientoDatos
        {
            PoliticaId = politica.Id,
            UsuarioId = usuarioId,
            Correo = usuario.Email,
            Contexto = ContextoRegistro,
            Aceptado = true,
            FechaAceptacion = fecha,
            IpOrigen = contexto.Ip is { Length: > 45 } ip ? ip[..45] : contexto.Ip,
            HashEvidencia = Criptografia.Sha256Hex($"{politica.Id}|{usuarioId}|{usuario.Email}|{fecha:O}|{politica.HashTexto}"),
        });
        await db.SaveChangesAsync(ct);
    }

    private Task<PoliticaTratamientoDatos?> PoliticaVigenteAsync(CancellationToken ct)
    {
        var hoy = DateOnly.FromDateTime(reloj.GetUtcNow().UtcDateTime);
        return db.PoliticasTratamientoDatos
            .Where(p => p.Activo && p.VigenteDesde <= hoy && (p.VigenteHasta == null || p.VigenteHasta >= hoy))
            .OrderByDescending(p => p.VigenteDesde).FirstOrDefaultAsync(ct);
    }
}
