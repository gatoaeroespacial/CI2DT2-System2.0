namespace Ci2dt2.Domain.Entities;

// M03 Proyectos de investigación
public class EstadoProyecto : EntidadAuditable
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public int? Orden { get; set; }
    public bool EsFinal { get; set; }
}

public class TipoProyecto : EntidadAuditable
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public bool RequiereHitosFijos { get; set; }
}

public class TipoConvocatoria : EntidadAuditable
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
}

public class Proyecto : EntidadAuditable
{
    public int Id { get; set; }
    public string? Codigo { get; set; }
    public string Titulo { get; set; } = "";
    public string? Resumen { get; set; }
    public int ProgramaId { get; set; }
    public int TipoProyectoId { get; set; }
    public int EstadoId { get; set; }
    public int InvestigadorPrincipalId { get; set; }
    public DateOnly? FechaInicio { get; set; }
    public DateOnly? FechaFin { get; set; }

    /// <summary>Entre 0 y 100 (CHECK).</summary>
    public decimal? PorcentajeAvance { get; set; }
    public string? Moneda { get; set; }
    public decimal? PresupuestoTotal { get; set; }
    public decimal? TasaCambio { get; set; }
    public decimal? PresupuestoTotalCop { get; set; }

    public Programa? Programa { get; set; }
    public TipoProyecto? TipoProyecto { get; set; }
    public EstadoProyecto? Estado { get; set; }
    public Usuario? InvestigadorPrincipal { get; set; }
}

public class ProyectoIntegrante : EntidadAuditable
{
    public int Id { get; set; }
    public int ProyectoId { get; set; }
    public int UsuarioId { get; set; }

    /// <summary>IP | Co-investigador | Auxiliar de campo | Estudiante</summary>
    public string RolEnProyecto { get; set; } = "";
    public DateOnly? FechaInicio { get; set; }
    public DateOnly? FechaFin { get; set; }

    public Proyecto? Proyecto { get; set; }
    public Usuario? Usuario { get; set; }
}

public class ProyectoAntecedente : EntidadAuditable
{
    public int Id { get; set; }
    public int ProyectoId { get; set; }
    public int? ConvocatoriaId { get; set; }
    public string NombreConvocatoria { get; set; } = "";
    public int? Anio { get; set; }
    public string? Resultado { get; set; }
    public decimal? Puntaje { get; set; }
    public string? EstadoBanco { get; set; }
    public string? Observaciones { get; set; }

    public Proyecto? Proyecto { get; set; }
    public Convocatoria? Convocatoria { get; set; }
}

public class ProyectoRevision : EntidadAuditable
{
    public int Id { get; set; }
    public int ProyectoId { get; set; }
    public int RevisorId { get; set; }
    public string Etapa { get; set; } = "";
    public string Decision { get; set; } = "";
    public int? EstadoAnteriorId { get; set; }
    public int EstadoNuevoId { get; set; }
    public string? Observaciones { get; set; }
    public DateTime Fecha { get; set; }

    public Proyecto? Proyecto { get; set; }
    public Usuario? Revisor { get; set; }
    public EstadoProyecto? EstadoAnterior { get; set; }
    public EstadoProyecto? EstadoNuevo { get; set; }
}

public class ProyectoGrupo : EntidadAuditable
{
    public int ProyectoId { get; set; }
    public int GrupoId { get; set; }

    public Proyecto? Proyecto { get; set; }
    public GrupoInvestigacion? Grupo { get; set; }
}

public class ProyectoAliado : EntidadAuditable
{
    public int Id { get; set; }
    public int ProyectoId { get; set; }
    public int AliadoId { get; set; }
    public string? RolAliado { get; set; }
    public int? CartaCompromisoDocId { get; set; }

    public Proyecto? Proyecto { get; set; }
    public Aliado? Aliado { get; set; }
    public Documento? CartaCompromisoDoc { get; set; }
}

public class ProyectoProducto : EntidadAuditable
{
    public int Id { get; set; }
    public int ProyectoId { get; set; }
    public string TipoProducto { get; set; } = "";
    public string? Descripcion { get; set; }
    public string Estado { get; set; } = "";
    public DateOnly? FechaCompromiso { get; set; }
    public int? PublicacionId { get; set; }
    public int? PropiedadIntelectualId { get; set; }
    public int? DocumentoId { get; set; }

    public Proyecto? Proyecto { get; set; }
    public Publicacion? Publicacion { get; set; }
    public PropiedadIntelectual? PropiedadIntelectual { get; set; }
    public Documento? Documento { get; set; }
}

public class ProyectoCierre : EntidadAuditable
{
    public int Id { get; set; }

    /// <summary>Un cierre por proyecto (UNIQUE).</summary>
    public int ProyectoId { get; set; }
    public int? InformeTecnicoDocId { get; set; }
    public int? InformeFinancieroDocId { get; set; }
    public DateOnly? FechaRadicacion { get; set; }
    public string? ConceptoComite { get; set; }
    public int? ActaComiteDocId { get; set; }
    public int? PazYSalvoDocId { get; set; }
    public DateOnly? FechaPazYSalvo { get; set; }

    public Proyecto? Proyecto { get; set; }
    public Documento? InformeTecnicoDoc { get; set; }
    public Documento? InformeFinancieroDoc { get; set; }
    public Documento? ActaComiteDoc { get; set; }
    public Documento? PazYSalvoDoc { get; set; }
}
