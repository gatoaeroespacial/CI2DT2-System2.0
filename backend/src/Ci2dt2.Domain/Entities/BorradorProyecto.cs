namespace Ci2dt2.Domain.Entities;

/// <summary>
/// Sesión y estado consolidado de un proyecto en formulación asistida por IA.
/// </summary>
public class BorradorProyecto : EntidadAuditable
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string TituloTentativo { get; set; } = "Proyecto sin título";
    public int? ProgramaId { get; set; }
    public string? LineaInvestigacion { get; set; }
    public string? ConvocatoriaNombre { get; set; }

    /// <summary>Resumen estructurado en JSON de la convocatoria: rubros, fechas, topes, criterios.</summary>
    public string? ConvocatoriaMetadataJson { get; set; }

    /// <summary>Documento TDR cargado en el repositorio (opcional).</summary>
    public int? DocumentoTdrId { get; set; }

    /// <summary>Ideacion | EnRedaccion | Revision | Completado</summary>
    public string EstadoAvance { get; set; } = "Ideacion";

    public Usuario? Usuario { get; set; }
    public Programa? Programa { get; set; }
    public Documento? DocumentoTdr { get; set; }
    public List<BorradorProyectoSeccion> Secciones { get; set; } = [];
    public List<BorradorProyectoMensaje> Mensajes { get; set; } = [];
}

/// <summary>
/// Apartado o sección del proyecto en el Draft Canvas (Justificación, Objetivos, Metodología, etc.).
/// </summary>
public class BorradorProyectoSeccion : EntidadAuditable
{
    public int Id { get; set; }
    public int BorradorProyectoId { get; set; }

    /// <summary>titulo | resumen | justificacion | estado_arte | objetivos | metodologia | cronograma | presupuesto | impactos</summary>
    public string CodigoSeccion { get; set; } = "";
    public string TituloSeccion { get; set; } = "";
    public string Contenido { get; set; } = "";
    public int Orden { get; set; }

    /// <summary>Vacio | Generado | Modificado | Aprobado</summary>
    public string Estado { get; set; } = "Vacio";

    /// <summary>Retroalimentación o recomendaciones del modelo sobre esta sección.</summary>
    public string? ObservacionesIA { get; set; }

    public BorradorProyecto? BorradorProyecto { get; set; }
}

/// <summary>
/// Mensaje en el hilo de interacción del asistente para este borrador.
/// </summary>
public class BorradorProyectoMensaje
{
    public long Id { get; set; }
    public int BorradorProyectoId { get; set; }

    /// <summary>usuario | asistente | sistema</summary>
    public string Rol { get; set; } = "";
    public string Texto { get; set; } = "";

    /// <summary>chat | analisis_tdr | propuesta_ideas | propuesta_seccion | auditoria_brechas</summary>
    public string TipoMensaje { get; set; } = "chat";

    /// <summary>Código de sección cuando el mensaje propone o impacta una sección específica.</summary>
    public string? SeccionDestino { get; set; }

    /// <summary>Datos estructurados adicionales (ej. opciones de ideas sugeridas en JSON).</summary>
    public string? MetadatosJson { get; set; }

    public DateTime FechaUtc { get; set; } = DateTime.UtcNow;

    public BorradorProyecto? BorradorProyecto { get; set; }
}

