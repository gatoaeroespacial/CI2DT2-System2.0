using Ci2dt2.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ci2dt2.Application.Common;

/// <summary>Acceso a datos para comandos (EF Core). Las consultas usan Dapper.</summary>
public interface IApplicationDbContext
{
    IQueryable<TEntity> Query<TEntity>() where TEntity : class;

    DbSet<Rol> Roles { get; }
    DbSet<Usuario> Usuarios { get; }
    DbSet<UsuarioRol> UsuarioRoles { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Programa> Programas { get; }
    DbSet<ObjetivoEspecifico> ObjetivosEspecificos { get; }
    DbSet<ProgramaIntegrante> ProgramaIntegrantes { get; }
    DbSet<Proyecto> Proyectos { get; }
    DbSet<EstadoProyecto> EstadosProyecto { get; }
    DbSet<TipoProyecto> TiposProyecto { get; }
    DbSet<ProyectoIntegrante> ProyectoIntegrantes { get; }
    DbSet<FuenteFinanciamiento> FuentesFinanciamiento { get; }
    DbSet<PropuestaConvocatoria> PropuestasConvocatoria { get; }
    DbSet<Convocatoria> Convocatorias { get; }
    DbSet<Avance> Avances { get; }
    DbSet<TipoDocumento> TiposDocumento { get; }
    DbSet<Documento> Documentos { get; }
    DbSet<Notificacion> Notificaciones { get; }
    DbSet<TipoAliado> TiposAliado { get; }
    DbSet<Aliado> Aliados { get; }
    DbSet<ProgramaAliado> ProgramaAliados { get; }
    DbSet<LogActividad> LogActividades { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
