using Ci2dt2.Domain.Entities;

namespace Ci2dt2.Infrastructure.Persistence;

/// <summary>
/// Catálogos iniciales según el diccionario de datos (modelo v3). Los Id son fijos para que
/// las migraciones sean reproducibles.
/// </summary>
internal static class DatosSemilla
{
    private static readonly DateTime Fecha = new(2026, 10, 7, 0, 0, 0, DateTimeKind.Utc);

    private static T S<T>(T entidad) where T : EntidadAuditable
    {
        entidad.CreadoEn = Fecha;
        return entidad;
    }

    public static readonly Rol[] Roles =
    [
        S(new Rol { Id = 1, Nombre = Domain.Roles.Administrador, Descripcion = "Configura el sistema y gestiona usuarios y roles" }),
        S(new Rol { Id = 2, Nombre = Domain.Roles.Director, Descripcion = "Director del Centro" }),
        S(new Rol { Id = 3, Nombre = Domain.Roles.Coordinador, Descripcion = "Coordinador de programa" }),
        S(new Rol { Id = 4, Nombre = Domain.Roles.Investigador, Descripcion = "Investigador / Investigador principal" }),
        S(new Rol { Id = 5, Nombre = Domain.Roles.Externo, Descripcion = "Representante de un aliado" }),
        S(new Rol { Id = 6, Nombre = Domain.Roles.Estudiante, Descripcion = "Estudiante (educación continua, semilleros, trabajos de grado)" }),
        S(new Rol { Id = 7, Nombre = Domain.Roles.Docente, Descripcion = "Docente de educación continua" }),
        S(new Rol { Id = 8, Nombre = Domain.Roles.Logistica, Descripcion = "Responsable de logística de eventos" }),
        S(new Rol { Id = 9, Nombre = Domain.Roles.Secretaria, Descripcion = "Registro de visitas y apoyo administrativo" }),
    ];

    // Los 5 programas corresponden a las 5 áreas de investigación del centro.
    public static readonly Programa[] Programas =
    [
        S(new Programa { Id = 1, Nombre = "Biotecnología y Agroindustria" }),
        S(new Programa { Id = 2, Nombre = "Bioingeniería" }),
        S(new Programa { Id = 3, Nombre = "Inteligencia Artificial, Modelado y Simulación" }),
        S(new Programa { Id = 4, Nombre = "Sistemas Computacionales y Bioinformática" }),
        S(new Programa { Id = 5, Nombre = "Ciudades Inteligentes" }),
    ];

    public static readonly EstadoProyecto[] EstadosProyecto =
    [
        S(new EstadoProyecto { Id = 1, Nombre = "En elaboración", Orden = 1 }),
        S(new EstadoProyecto { Id = 2, Nombre = "En revisión", Orden = 2 }),
        S(new EstadoProyecto { Id = 3, Nombre = "Devuelto", Orden = 3 }),
        S(new EstadoProyecto { Id = 4, Nombre = "Presentado", Orden = 4 }),
        S(new EstadoProyecto { Id = 5, Nombre = "Aprobado", Orden = 5 }),
        S(new EstadoProyecto { Id = 6, Nombre = "En ejecución", Orden = 6 }),
        S(new EstadoProyecto { Id = 7, Nombre = "Finalizado", Orden = 7, EsFinal = true }),
        S(new EstadoProyecto { Id = 8, Nombre = "No aprobado", Orden = 8, EsFinal = true }),
        S(new EstadoProyecto { Id = 9, Nombre = "Suspendido", Orden = 9 }),
    ];

    public static readonly TipoProyecto[] TiposProyecto =
    [
        S(new TipoProyecto { Id = 1, Nombre = "I+D+i" }),
        S(new TipoProyecto { Id = 2, Nombre = "Reto Empresarial", RequiereHitosFijos = true }),
        S(new TipoProyecto { Id = 3, Nombre = "Consultoría" }),
        S(new TipoProyecto { Id = 4, Nombre = "Tesis" }),
    ];

    public static readonly TipoConvocatoria[] TiposConvocatoria =
    [
        S(new TipoConvocatoria { Id = 1, Nombre = "Interna" }),
        S(new TipoConvocatoria { Id = 2, Nombre = "Nacional" }),
        S(new TipoConvocatoria { Id = 3, Nombre = "Internacional" }),
        S(new TipoConvocatoria { Id = 4, Nombre = "Minciencias FJC" }),
        S(new TipoConvocatoria { Id = 5, Nombre = "SGR" }),
        S(new TipoConvocatoria { Id = 6, Nombre = "Conjunta" }),
    ];

    public static readonly TipoHito[] TiposHito =
    [
        S(new TipoHito { Id = 1, Nombre = "Entregable" }),
        S(new TipoHito { Id = 2, Nombre = "Informe" }),
        S(new TipoHito { Id = 3, Nombre = "Reunión" }),
        S(new TipoHito { Id = 4, Nombre = "Pago" }),
        S(new TipoHito { Id = 5, Nombre = "Sprint" }),
        S(new TipoHito { Id = 6, Nombre = "MVP" }),
    ];

    // Tipos del diccionario más los que exige el repositorio documental del MVP (HU-21).
    public static readonly TipoDocumento[] TiposDocumento =
    [
        S(new TipoDocumento { Id = 1, Nombre = "Acta" }),
        S(new TipoDocumento { Id = 2, Nombre = "Plan de trabajo" }),
        S(new TipoDocumento { Id = 3, Nombre = "Doc. Conceptual MGA" }),
        S(new TipoDocumento { Id = 4, Nombre = "Informe final" }),
        S(new TipoDocumento { Id = 5, Nombre = "Hoja de vida" }),
        S(new TipoDocumento { Id = 6, Nombre = "Propuesta técnica" }),
        S(new TipoDocumento { Id = 7, Nombre = "Presupuesto" }),
        S(new TipoDocumento { Id = 8, Nombre = "Acto administrativo de aprobación" }),
        S(new TipoDocumento { Id = 9, Nombre = "Carta de compromiso" }),
        S(new TipoDocumento { Id = 10, Nombre = "Términos de referencia" }),
        S(new TipoDocumento { Id = 11, Nombre = "Comprobante de radicación" }),
        S(new TipoDocumento { Id = 12, Nombre = "Informe técnico" }),
        S(new TipoDocumento { Id = 13, Nombre = "Informe financiero" }),
        S(new TipoDocumento { Id = 14, Nombre = "Paz y salvo" }),
        S(new TipoDocumento { Id = 15, Nombre = "Otro" }),
    ];

    public static readonly TipoFormacion[] TiposFormacion =
    [
        S(new TipoFormacion { Id = 1, Nombre = "Diplomado" }),
        S(new TipoFormacion { Id = 2, Nombre = "Curso" }),
        S(new TipoFormacion { Id = 3, Nombre = "Microcertificación" }),
        S(new TipoFormacion { Id = 4, Nombre = "Taller" }),
    ];

    public static readonly Modalidad[] Modalidades =
    [
        S(new Modalidad { Id = 1, Nombre = "Presencial" }),
        S(new Modalidad { Id = 2, Nombre = "Virtual" }),
        S(new Modalidad { Id = 3, Nombre = "Híbrido" }),
    ];

    public static readonly TipoEvento[] TiposEvento =
    [
        S(new TipoEvento { Id = 1, Nombre = "Congreso" }),
        S(new TipoEvento { Id = 2, Nombre = "Seminario" }),
        S(new TipoEvento { Id = 3, Nombre = "Taller" }),
        S(new TipoEvento { Id = 4, Nombre = "Innovación en Órbita" }),
    ];

    public static readonly TipoIntegranteSemillero[] TiposIntegranteSemillero =
    [
        S(new TipoIntegranteSemillero { Id = 1, Nombre = "Estudiante" }),
        S(new TipoIntegranteSemillero { Id = 2, Nombre = "Docente" }),
        S(new TipoIntegranteSemillero { Id = 3, Nombre = "Egresado" }),
        S(new TipoIntegranteSemillero { Id = 4, Nombre = "Joven investigador" }),
    ];

    public static readonly TipoAliado[] TiposAliado =
    [
        S(new TipoAliado { Id = 1, Nombre = "Universidad" }),
        S(new TipoAliado { Id = 2, Nombre = "Empresa" }),
        S(new TipoAliado { Id = 3, Nombre = "Entidad pública" }),
        S(new TipoAliado { Id = 4, Nombre = "Cámara de Comercio" }),
        S(new TipoAliado { Id = 5, Nombre = "ONG" }),
    ];

    public static readonly TipoPublicacion[] TiposPublicacion =
    [
        S(new TipoPublicacion { Id = 1, Nombre = "Q1" }),
        S(new TipoPublicacion { Id = 2, Nombre = "Q2" }),
        S(new TipoPublicacion { Id = 3, Nombre = "Q3" }),
        S(new TipoPublicacion { Id = 4, Nombre = "Q4" }),
        S(new TipoPublicacion { Id = 5, Nombre = "Libro" }),
        S(new TipoPublicacion { Id = 6, Nombre = "Capítulo" }),
        S(new TipoPublicacion { Id = 7, Nombre = "Ponencia" }),
        S(new TipoPublicacion { Id = 8, Nombre = "Tesis" }),
    ];

    // Primera versión de la política de datos. El texto oficial y su HashTexto se publican con HU-29.
    public static readonly PoliticaTratamientoDatos[] PoliticasTratamientoDatos =
    [
        S(new PoliticaTratamientoDatos { Id = 1, Version = "1.0", TextoUrl = "/politica-datos", VigenteDesde = new DateOnly(2026, 10, 7) }),
    ];
}
