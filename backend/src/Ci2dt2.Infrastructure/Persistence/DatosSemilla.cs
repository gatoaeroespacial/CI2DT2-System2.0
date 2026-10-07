using Ci2dt2.Domain.Entities;

namespace Ci2dt2.Infrastructure.Persistence;

/// <summary>Catálogos iniciales del MVP (HT-02). Los Id son fijos para que las migraciones sean reproducibles.</summary>
internal static class DatosSemilla
{
    public static readonly Rol[] Roles =
    [
        new() { Id = 1, Nombre = Domain.Roles.Administrador, Descripcion = "Configura el sistema y gestiona usuarios y roles" },
        new() { Id = 2, Nombre = Domain.Roles.Director, Descripcion = "Director del Centro" },
        new() { Id = 3, Nombre = Domain.Roles.Coordinador, Descripcion = "Coordinador de programa" },
        new() { Id = 4, Nombre = Domain.Roles.Investigador, Descripcion = "Investigador / Investigador principal" },
        new() { Id = 5, Nombre = Domain.Roles.Externo, Descripcion = "Representante de un aliado" },
    ];

    // D-2: los 5 programas corresponden a las 5 áreas de investigación del centro.
    public static readonly Programa[] Programas =
    [
        new() { Id = 1, Nombre = "Biotecnología y Agroindustria" },
        new() { Id = 2, Nombre = "Bioingeniería" },
        new() { Id = 3, Nombre = "Inteligencia Artificial, Modelado y Simulación" },
        new() { Id = 4, Nombre = "Sistemas Computacionales y Bioinformática" },
        new() { Id = 5, Nombre = "Ciudades Inteligentes" },
    ];

    // D-3: se agrega "No aprobado".
    public static readonly EstadoProyecto[] EstadosProyecto =
    [
        new() { Id = 1, Nombre = "En elaboración", Orden = 1, EsFinal = false },
        new() { Id = 2, Nombre = "Presentado", Orden = 2, EsFinal = false },
        new() { Id = 3, Nombre = "Aprobado", Orden = 3, EsFinal = false },
        new() { Id = 4, Nombre = "En ejecución", Orden = 4, EsFinal = false },
        new() { Id = 5, Nombre = "Finalizado", Orden = 5, EsFinal = true },
        new() { Id = 6, Nombre = "No aprobado", Orden = 6, EsFinal = true },
    ];

    public static readonly TipoProyecto[] TiposProyecto =
    [
        new() { Id = 1, Nombre = "Investigación básica" },
        new() { Id = 2, Nombre = "Investigación aplicada" },
        new() { Id = 3, Nombre = "Desarrollo tecnológico" },
        new() { Id = 4, Nombre = "Innovación" },
        new() { Id = 5, Nombre = "Reto empresarial", RequiereHitosFijos = true },
        new() { Id = 6, Nombre = "Consultoría / servicio" },
    ];

    public static readonly TipoConvocatoria[] TiposConvocatoria =
    [
        new() { Id = 1, Nombre = "Interna (VIP UCaldas)" },
        new() { Id = 2, Nombre = "Externa nacional" },
        new() { Id = 3, Nombre = "Externa internacional" },
        new() { Id = 4, Nombre = "Minciencias – Fondo FJC" },
        new() { Id = 5, Nombre = "Minciencias – SGR (Fondo CTeI)" },
        new() { Id = 6, Nombre = "Conjunta" },
    ];

    public static readonly TipoHito[] TiposHito =
    [
        new() { Id = 1, Nombre = "Entregable" },
        new() { Id = 2, Nombre = "Informe de avance" },
        new() { Id = 3, Nombre = "Informe final" },
        new() { Id = 4, Nombre = "Reunión técnica" },
        new() { Id = 5, Nombre = "Desembolso / pago" },
        new() { Id = 6, Nombre = "Sprint" },
        new() { Id = 7, Nombre = "Entrega de MVP" },
    ];

    public static readonly TipoDocumento[] TiposDocumento =
    [
        new() { Id = 1, Nombre = "Propuesta técnica" },
        new() { Id = 2, Nombre = "Presupuesto" },
        new() { Id = 3, Nombre = "Acto administrativo de aprobación" },
        new() { Id = 4, Nombre = "Carta de compromiso" },
        new() { Id = 5, Nombre = "Plan de trabajo" },
        new() { Id = 6, Nombre = "Términos de referencia" },
        new() { Id = 7, Nombre = "Comprobante de radicación" },
        new() { Id = 8, Nombre = "Documento conceptual MGA" },
        new() { Id = 9, Nombre = "Informe técnico" },
        new() { Id = 10, Nombre = "Informe financiero" },
        new() { Id = 11, Nombre = "Acta de comité" },
        new() { Id = 12, Nombre = "Paz y salvo" },
        new() { Id = 13, Nombre = "Otro" },
    ];

    public static readonly TipoAliado[] TiposAliado =
    [
        new() { Id = 1, Nombre = "Universidad" },
        new() { Id = 2, Nombre = "Empresa" },
        new() { Id = 3, Nombre = "Entidad pública" },
        new() { Id = 4, Nombre = "ONG / Fundación" },
        new() { Id = 5, Nombre = "Organismo internacional" },
        new() { Id = 6, Nombre = "Gremio / Clúster" },
    ];

    // Primera versión de la política de datos. El texto oficial (y su HashTexto) se
    // publica con HU-29; mientras tanto la versión queda registrada como vigente.
    public static readonly PoliticaTratamientoDatos[] PoliticasTratamientoDatos =
    [
        new() { Id = 1, Version = "1.0", TextoUrl = "/politica-datos", VigenteDesde = new DateOnly(2026, 10, 7) },
    ];
}
