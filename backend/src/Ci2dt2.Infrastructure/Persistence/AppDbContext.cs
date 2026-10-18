using Ci2dt2.Application.Common;
using Ci2dt2.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ci2dt2.Infrastructure.Persistence;

/// <summary>Modelo v3 del diccionario de datos: 67 tablas en 15 módulos.</summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IApplicationDbContext
{
    // M01 Seguridad y acceso
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<UsuarioRol> UsuarioRoles => Set<UsuarioRol>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // M02 Organización del Centro
    public DbSet<Programa> Programas => Set<Programa>();
    public DbSet<ObjetivoEspecifico> ObjetivosEspecificos => Set<ObjetivoEspecifico>();
    public DbSet<ProgramaIntegrante> ProgramaIntegrantes => Set<ProgramaIntegrante>();
    public DbSet<GrupoInvestigacion> GruposInvestigacion => Set<GrupoInvestigacion>();

    // M03 Proyectos de investigación
    public DbSet<Proyecto> Proyectos => Set<Proyecto>();
    public DbSet<EstadoProyecto> EstadosProyecto => Set<EstadoProyecto>();
    public DbSet<TipoProyecto> TiposProyecto => Set<TipoProyecto>();
    public DbSet<TipoConvocatoria> TiposConvocatoria => Set<TipoConvocatoria>();
    public DbSet<ProyectoIntegrante> ProyectoIntegrantes => Set<ProyectoIntegrante>();
    public DbSet<ProyectoAntecedente> ProyectoAntecedentes => Set<ProyectoAntecedente>();
    public DbSet<ProyectoRevision> ProyectoRevisiones => Set<ProyectoRevision>();
    public DbSet<ProyectoGrupo> ProyectoGrupos => Set<ProyectoGrupo>();
    public DbSet<ProyectoAliado> ProyectoAliados => Set<ProyectoAliado>();
    public DbSet<ProyectoProducto> ProyectoProductos => Set<ProyectoProducto>();
    public DbSet<ProyectoCierre> ProyectoCierres => Set<ProyectoCierre>();

    // M04 Convocatorias y financiamiento
    public DbSet<Convocatoria> Convocatorias => Set<Convocatoria>();
    public DbSet<PropuestaConvocatoria> PropuestasConvocatoria => Set<PropuestaConvocatoria>();
    public DbSet<FuenteFinanciamiento> FuentesFinanciamiento => Set<FuenteFinanciamiento>();

    // M05 Cronogramas y seguimiento
    public DbSet<Cronograma> Cronogramas => Set<Cronograma>();
    public DbSet<TipoHito> TiposHito => Set<TipoHito>();
    public DbSet<Hito> Hitos => Set<Hito>();
    public DbSet<Avance> Avances => Set<Avance>();
    public DbSet<AvanceAdjunto> AvancesAdjuntos => Set<AvanceAdjunto>();
    public DbSet<AvanceValidacion> AvanceValidaciones => Set<AvanceValidacion>();

    // M06 Gestión documental
    public DbSet<TipoDocumento> TiposDocumento => Set<TipoDocumento>();
    public DbSet<Documento> Documentos => Set<Documento>();
    public DbSet<DocumentoFirma> DocumentoFirmas => Set<DocumentoFirma>();

    // M07 Notificaciones
    public DbSet<Notificacion> Notificaciones => Set<Notificacion>();
    public DbSet<PreferenciaNotificacion> PreferenciasNotificacion => Set<PreferenciaNotificacion>();

    // M08 Educación continua
    public DbSet<TipoFormacion> TiposFormacion => Set<TipoFormacion>();
    public DbSet<Modalidad> Modalidades => Set<Modalidad>();
    public DbSet<Formacion> Formaciones => Set<Formacion>();
    public DbSet<FormacionDocente> FormacionDocentes => Set<FormacionDocente>();
    public DbSet<Inscripcion> Inscripciones => Set<Inscripcion>();
    public DbSet<FormacionSesion> FormacionSesiones => Set<FormacionSesion>();
    public DbSet<Asistencia> Asistencias => Set<Asistencia>();
    public DbSet<Calificacion> Calificaciones => Set<Calificacion>();
    public DbSet<Certificado> Certificados => Set<Certificado>();

    // M09 Eventos
    public DbSet<TipoEvento> TiposEvento => Set<TipoEvento>();
    public DbSet<Evento> Eventos => Set<Evento>();
    public DbSet<EventoAsistente> EventoAsistentes => Set<EventoAsistente>();

    // M10 Semilleros
    public DbSet<Semillero> Semilleros => Set<Semillero>();
    public DbSet<TipoIntegranteSemillero> TiposIntegranteSemillero => Set<TipoIntegranteSemillero>();
    public DbSet<SemilleroIntegrante> SemilleroIntegrantes => Set<SemilleroIntegrante>();

    // M11 Entorno empresarial
    public DbSet<TipoAliado> TiposAliado => Set<TipoAliado>();
    public DbSet<Aliado> Aliados => Set<Aliado>();
    public DbSet<ProgramaAliado> ProgramaAliados => Set<ProgramaAliado>();

    // M12 Producción académica y propiedad intelectual
    public DbSet<TipoPublicacion> TiposPublicacion => Set<TipoPublicacion>();
    public DbSet<Publicacion> Publicaciones => Set<Publicacion>();
    public DbSet<PublicacionAutor> PublicacionAutores => Set<PublicacionAutor>();
    public DbSet<PropiedadIntelectual> PropiedadIntelectual => Set<PropiedadIntelectual>();
    public DbSet<PropiedadIntelectualAutor> PropiedadIntelectualAutores => Set<PropiedadIntelectualAutor>();
    public DbSet<TrabajoGrado> TrabajosGrado => Set<TrabajoGrado>();
    public DbSet<TrabajoGradoJurado> TrabajoGradoJurados => Set<TrabajoGradoJurado>();
    public DbSet<Sustentacion> Sustentaciones => Set<Sustentacion>();

    // M13 Indicadores y reportes
    public DbSet<Indicador> Indicadores => Set<Indicador>();
    public DbSet<IndicadorMedicion> IndicadorMediciones => Set<IndicadorMedicion>();
    public DbSet<ReporteGenerado> ReportesGenerados => Set<ReporteGenerado>();

    // M14 Visitas y datos personales
    public DbSet<PoliticaTratamientoDatos> PoliticasTratamientoDatos => Set<PoliticaTratamientoDatos>();
    public DbSet<ConsentimientoDatos> ConsentimientosDatos => Set<ConsentimientoDatos>();
    public DbSet<Visita> Visitas => Set<Visita>();

    // M15 Auditoría e IA
    public DbSet<LogActividad> LogActividades => Set<LogActividad>();
    public DbSet<FuenteConocimiento> FuentesConocimiento => Set<FuenteConocimiento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Las cadenas son nvarchar por defecto (tildes y ñ); las columnas char(n) se marcan en cada configuración.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        foreach (var tipo in modelBuilder.Model.GetEntityTypes().Select(t => t.ClrType).ToList())
        {
            var entidad = modelBuilder.Entity(tipo);

            // Columnas de auditoría (regla 4): todas las tablas salvo las inmutables.
            if (typeof(EntidadAuditable).IsAssignableFrom(tipo))
            {
                entidad.Property(nameof(EntidadAuditable.CreadoEn)).HasDefaultValueSql("SYSUTCDATETIME()");
                entidad.HasOne(typeof(Usuario)).WithMany().HasForeignKey(nameof(EntidadAuditable.CreadoPorId));
            }

            // Columnas bit: NOT NULL DEFAULT 0, salvo Activo que es DEFAULT 1 (regla 6).
            foreach (var bit in tipo.GetProperties().Where(p => p.PropertyType == typeof(bool)))
            {
                if (bit.Name == nameof(EntidadAuditable.Activo))
                    entidad.Property(bit.Name).HasDefaultValue(true).HasSentinel(true);
                else
                    entidad.Property(bit.Name).HasDefaultValue(false);
            }
        }

        // Borrado lógico: todas las llaves foráneas usan ON DELETE NO ACTION (regla 5).
        foreach (var fk in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            fk.DeleteBehavior = DeleteBehavior.NoAction;
    }
}
