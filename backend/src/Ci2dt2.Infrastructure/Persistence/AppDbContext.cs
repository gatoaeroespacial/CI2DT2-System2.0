using Ci2dt2.Application.Common;
using Ci2dt2.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ci2dt2.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IApplicationDbContext
{
    // M01 Seguridad
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<UsuarioRol> UsuarioRoles => Set<UsuarioRol>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // M02 Organización
    public DbSet<Programa> Programas => Set<Programa>();
    public DbSet<ObjetivoEspecifico> ObjetivosEspecificos => Set<ObjetivoEspecifico>();
    public DbSet<ProgramaIntegrante> ProgramaIntegrantes => Set<ProgramaIntegrante>();

    // M03 Proyectos
    public DbSet<Proyecto> Proyectos => Set<Proyecto>();
    public DbSet<EstadoProyecto> EstadosProyecto => Set<EstadoProyecto>();
    public DbSet<TipoProyecto> TiposProyecto => Set<TipoProyecto>();
    public DbSet<TipoConvocatoria> TiposConvocatoria => Set<TipoConvocatoria>();
    public DbSet<ProyectoIntegrante> ProyectoIntegrantes => Set<ProyectoIntegrante>();
    public DbSet<ProyectoRevision> ProyectoRevisiones => Set<ProyectoRevision>();

    // M04 Convocatorias y Financiamiento
    public DbSet<Convocatoria> Convocatorias => Set<Convocatoria>();
    public DbSet<PropuestaConvocatoria> PropuestasConvocatoria => Set<PropuestaConvocatoria>();
    public DbSet<FuenteFinanciamiento> FuentesFinanciamiento => Set<FuenteFinanciamiento>();

    // M05 Cronogramas y Seguimiento
    public DbSet<Cronograma> Cronogramas => Set<Cronograma>();
    public DbSet<TipoHito> TiposHito => Set<TipoHito>();
    public DbSet<Hito> Hitos => Set<Hito>();
    public DbSet<Avance> Avances => Set<Avance>();
    public DbSet<AvanceAdjunto> AvancesAdjuntos => Set<AvanceAdjunto>();
    public DbSet<AvanceValidacion> AvanceValidaciones => Set<AvanceValidacion>();

    // M06 Documentos
    public DbSet<TipoDocumento> TiposDocumento => Set<TipoDocumento>();
    public DbSet<Documento> Documentos => Set<Documento>();

    // M07 Notificaciones
    public DbSet<Notificacion> Notificaciones => Set<Notificacion>();

    // M11 Entorno Empresarial
    public DbSet<TipoAliado> TiposAliado => Set<TipoAliado>();
    public DbSet<Aliado> Aliados => Set<Aliado>();
    public DbSet<ProgramaAliado> ProgramaAliados => Set<ProgramaAliado>();

    // M14 Datos personales
    public DbSet<PoliticaTratamientoDatos> PoliticasTratamientoDatos => Set<PoliticaTratamientoDatos>();
    public DbSet<ConsentimientoDatos> ConsentimientosDatos => Set<ConsentimientoDatos>();

    // M15 Auditoría
    public DbSet<LogActividad> LogActividades => Set<LogActividad>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // El ER v3 define las cadenas como varchar.
        configurationBuilder.Properties<string>().AreUnicode(false);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Sin borrados en cascada: los registros se desactivan o se cierran con fechas.
        foreach (var fk in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            fk.DeleteBehavior = DeleteBehavior.Restrict;
    }
}
