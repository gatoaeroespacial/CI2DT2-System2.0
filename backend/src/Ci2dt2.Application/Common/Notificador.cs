using Ci2dt2.Domain;
using Ci2dt2.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ci2dt2.Application.Common;

/// <summary>
/// Registra notificaciones en la tabla Notificaciones. Se guardan en el mismo SaveChanges que
/// la operación que las origina; el envío por correo y la bandeja llegan con HU-22/HU-23.
/// </summary>
public sealed class Notificador(IApplicationDbContext db, TimeProvider reloj)
{
    public void Notificar(int usuarioId, string tipo, string titulo, string mensaje, string? referenciaTipo = null, int? referenciaId = null) =>
        db.Notificaciones.Add(new Notificacion
        {
            UsuarioId = usuarioId,
            Tipo = tipo,
            Titulo = titulo.Length > 200 ? titulo[..200] : titulo,
            Mensaje = mensaje,
            ReferenciaTipo = referenciaTipo,
            ReferenciaId = referenciaId,
        });

    /// <summary>Coordinador con vinculación vigente al programa, si existe.</summary>
    public Task<int?> CoordinadorVigenteAsync(int programaId, CancellationToken ct)
    {
        var hoy = DateOnly.FromDateTime(reloj.GetUtcNow().UtcDateTime);
        return db.ProgramaIntegrantes
            .Where(pi => pi.ProgramaId == programaId && pi.Activo && pi.RolEnPrograma == RolesPrograma.Coordinador
                && (pi.FechaInicio == null || pi.FechaInicio <= hoy) && (pi.FechaFin == null || pi.FechaFin > hoy))
            .Select(pi => (int?)pi.UsuarioId)
            .FirstOrDefaultAsync(ct);
    }
}
