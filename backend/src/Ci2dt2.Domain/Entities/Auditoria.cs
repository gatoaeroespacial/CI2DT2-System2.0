namespace Ci2dt2.Domain.Entities;

// M15 Auditoría e IA

/// <summary>Inmutable: sin columnas de auditoría ni UPDATE/DELETE.</summary>
public class LogActividad
{
    public long Id { get; set; }
    public int? UsuarioId { get; set; }
    public string? IpOrigen { get; set; }

    // Recurso/RecursoId es una referencia sin llave foránea.
    public string Recurso { get; set; } = "";
    public string? RecursoId { get; set; }
    public string Accion { get; set; } = "";
    public string? Ruta { get; set; }
    public int? EstadoHttp { get; set; }
    public string? ValoresAnteriores { get; set; }
    public string? ValoresNuevos { get; set; }
    public DateTime FechaUtc { get; set; }

    // Nulo en el MVP; la cadena de integridad se activa en Release 2.
    public string? HashEncadenado { get; set; }

    public Usuario? Usuario { get; set; }
}

public class FuenteConocimiento : EntidadAuditable
{
    public int Id { get; set; }
    public int? DocumentoId { get; set; }
    public string Titulo { get; set; } = "";
    public string? TipoFuente { get; set; }

    // Referencia polimórfica sin llave foránea.
    public string? EntidadTipo { get; set; }
    public int? EntidadId { get; set; }

    /// <summary>El chatbot público solo usa EsPublica = 1.</summary>
    public bool EsPublica { get; set; }
    public string? HashContenido { get; set; }
    public string? EstadoIndexacion { get; set; }
    public string? IdVectorStore { get; set; }
    public DateTime? FechaIndexacion { get; set; }

    public Documento? Documento { get; set; }
}
