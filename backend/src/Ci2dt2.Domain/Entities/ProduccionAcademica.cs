namespace Ci2dt2.Domain.Entities;

// M12 Producción académica y propiedad intelectual
public class TipoPublicacion : EntidadAuditable
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
}

public class Publicacion : EntidadAuditable
{
    public int Id { get; set; }
    public int? ProgramaId { get; set; }
    public int? ProyectoId { get; set; }
    public int TipoPublicacionId { get; set; }
    public string Titulo { get; set; } = "";
    public string? Revista { get; set; }
    public int? Anio { get; set; }
    public string? Doi { get; set; }
    public bool EsIndexada { get; set; }

    public Programa? Programa { get; set; }
    public Proyecto? Proyecto { get; set; }
    public TipoPublicacion? TipoPublicacion { get; set; }
}

public class PublicacionAutor : EntidadAuditable
{
    public int PublicacionId { get; set; }
    public int UsuarioId { get; set; }
    public int? Orden { get; set; }

    public Publicacion? Publicacion { get; set; }
    public Usuario? Usuario { get; set; }
}

public class PropiedadIntelectual : EntidadAuditable
{
    public int Id { get; set; }
    public int? ProyectoId { get; set; }
    public string Tipo { get; set; } = "";
    public string Titulo { get; set; } = "";
    public string? EntidadTramite { get; set; }
    public string Estado { get; set; } = "";
    public string? NumeroRadicado { get; set; }
    public DateOnly? FechaRadicacion { get; set; }
    public string? NumeroRegistro { get; set; }
    public string? NumeroResolucion { get; set; }
    public DateOnly? FechaConcesion { get; set; }
    public decimal? ParticipacionUcaldasPct { get; set; }

    public Proyecto? Proyecto { get; set; }
}

public class PropiedadIntelectualAutor : EntidadAuditable
{
    public int PropiedadIntelectualId { get; set; }
    public int UsuarioId { get; set; }
    public string? RolAutor { get; set; }
    public decimal? ParticipacionPct { get; set; }

    public PropiedadIntelectual? PropiedadIntelectual { get; set; }
    public Usuario? Usuario { get; set; }
}

public class TrabajoGrado : EntidadAuditable
{
    public int Id { get; set; }
    public int? ProgramaId { get; set; }
    public int? ProyectoId { get; set; }
    public int EstudianteId { get; set; }
    public int? DirectorId { get; set; }
    public int? CodirectorId { get; set; }
    public int? PropuestaDocId { get; set; }
    public string Tipo { get; set; } = "";
    public string Titulo { get; set; } = "";
    public string Estado { get; set; } = "";
    public string? ConceptoFinal { get; set; }

    public Programa? Programa { get; set; }
    public Proyecto? Proyecto { get; set; }
    public Usuario? Estudiante { get; set; }
    public Usuario? Director { get; set; }
    public Usuario? Codirector { get; set; }
    public Documento? PropuestaDoc { get; set; }
}

public class TrabajoGradoJurado : EntidadAuditable
{
    public int Id { get; set; }
    public int TrabajoGradoId { get; set; }
    public int UsuarioId { get; set; }
    public string? RolJurado { get; set; }
    public string? Concepto { get; set; }

    public TrabajoGrado? TrabajoGrado { get; set; }
    public Usuario? Usuario { get; set; }
}

public class Sustentacion : EntidadAuditable
{
    public int Id { get; set; }
    public int TrabajoGradoId { get; set; }
    public int? ActaDocumentoId { get; set; }
    public DateTime? FechaHora { get; set; }
    public string? Modalidad { get; set; }
    public string? Lugar { get; set; }
    public string? Concepto { get; set; }

    public TrabajoGrado? TrabajoGrado { get; set; }
    public Documento? ActaDocumento { get; set; }
}
