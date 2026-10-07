using Ci2dt2.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ci2dt2.Application.Common;

/// <summary>Acceso a datos para comandos (EF Core). Las consultas complejas usan Dapper.</summary>
public interface IApplicationDbContext
{
    DbSet<Rol> Roles { get; }
    DbSet<Usuario> Usuarios { get; }
    DbSet<UsuarioRol> UsuarioRoles { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Programa> Programas { get; }
    DbSet<ProgramaIntegrante> ProgramaIntegrantes { get; }
    DbSet<Proyecto> Proyectos { get; }
    DbSet<ProyectoIntegrante> ProyectoIntegrantes { get; }
    DbSet<Aliado> Aliados { get; }
    DbSet<LogActividad> LogActividades { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
