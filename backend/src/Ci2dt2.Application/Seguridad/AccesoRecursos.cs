using Ci2dt2.Application.Common;
using Ci2dt2.Domain;
using Microsoft.EntityFrameworkCore;

namespace Ci2dt2.Application.Seguridad;

/// <summary>Usuario autenticado de la petición (id y roles del token).</summary>
public sealed record UsuarioAutenticado(int Id, IReadOnlyCollection<string> Roles)
{
    public bool Tiene(string rol) => Roles.Contains(rol);
}

/// <summary>
/// Autorización por recurso (HU-05). Las restricciones de alcance del Anexo A se validan
/// siempre contra la base de datos y los permisos de varios roles se combinan por unión.
/// </summary>
public sealed class AccesoRecursos(IApplicationDbContext db, TimeProvider reloj)
{
    private DateOnly Hoy => DateOnly.FromDateTime(reloj.GetUtcNow().UtcDateTime);

    /// <summary>Coordinador con vinculación vigente (FechaFin nula o futura) al programa.</summary>
    public Task<bool> EsCoordinadorVigenteAsync(int usuarioId, int programaId, CancellationToken ct = default)
    {
        var hoy = Hoy;
        return db.ProgramaIntegrantes.AnyAsync(pi =>
            pi.UsuarioId == usuarioId &&
            pi.ProgramaId == programaId &&
            pi.RolEnPrograma == RolesPrograma.Coordinador &&
            pi.FechaInicio <= hoy &&
            (pi.FechaFin == null || pi.FechaFin > hoy), ct);
    }

    /// <summary>Integrante vigente del proyecto (ProyectoIntegrantes).</summary>
    public Task<bool> EsIntegranteProyectoAsync(int usuarioId, int proyectoId, CancellationToken ct = default)
    {
        var hoy = Hoy;
        return db.ProyectoIntegrantes.AnyAsync(pi =>
            pi.UsuarioId == usuarioId &&
            pi.ProyectoId == proyectoId &&
            (pi.FechaFin == null || pi.FechaFin > hoy), ct);
    }

    public Task<bool> EsInvestigadorPrincipalAsync(int usuarioId, int proyectoId, CancellationToken ct = default) =>
        db.Proyectos.AnyAsync(p => p.Id == proyectoId && p.InvestigadorPrincipalId == usuarioId, ct);

    /// <summary>
    /// Celdas "Su programa" del Anexo A: Administrador y Director sobre todos los programas,
    /// Coordinador solo sobre los programas donde es Coordinador vigente.
    /// </summary>
    public async Task<bool> PuedeGestionarProgramaAsync(UsuarioAutenticado usuario, int programaId, CancellationToken ct = default)
    {
        if (usuario.Tiene(Roles.Administrador) || usuario.Tiene(Roles.Director))
            return true;
        return usuario.Tiene(Roles.Coordinador) && await EsCoordinadorVigenteAsync(usuario.Id, programaId, ct);
    }

    /// <summary>
    /// "Ver proyectos": Administrador y Director todos; Coordinador los de su programa;
    /// Investigador solo aquellos donde es integrante. Externo no accede.
    /// </summary>
    public async Task<bool> PuedeVerProyectoAsync(UsuarioAutenticado usuario, int proyectoId, CancellationToken ct = default)
    {
        if (usuario.Tiene(Roles.Administrador) || usuario.Tiene(Roles.Director))
            return true;

        if (usuario.Tiene(Roles.Coordinador) && await EsCoordinadorDelProyectoAsync(usuario.Id, proyectoId, ct))
            return true;

        return usuario.Tiene(Roles.Investigador) && await EsIntegranteProyectoAsync(usuario.Id, proyectoId, ct);
    }

    /// <summary>
    /// Operaciones de escritura sobre un proyecto con alcance "Su programa" para el Coordinador
    /// y "Si es IP" para el Investigador (editar propuestas, gestionar hitos).
    /// </summary>
    public async Task<bool> PuedeModificarProyectoComoIpAsync(UsuarioAutenticado usuario, int proyectoId, CancellationToken ct = default)
    {
        if (usuario.Tiene(Roles.Administrador) || usuario.Tiene(Roles.Director))
            return true;

        if (usuario.Tiene(Roles.Coordinador) && await EsCoordinadorDelProyectoAsync(usuario.Id, proyectoId, ct))
            return true;

        return usuario.Tiene(Roles.Investigador) && await EsInvestigadorPrincipalAsync(usuario.Id, proyectoId, ct);
    }

    private async Task<bool> EsCoordinadorDelProyectoAsync(int usuarioId, int proyectoId, CancellationToken ct)
    {
        var programaId = await db.Proyectos.Where(p => p.Id == proyectoId)
            .Select(p => (int?)p.ProgramaId).FirstOrDefaultAsync(ct);
        return programaId is not null && await EsCoordinadorVigenteAsync(usuarioId, programaId.Value, ct);
    }
}
