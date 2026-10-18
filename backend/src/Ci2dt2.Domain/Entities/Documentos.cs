namespace Ci2dt2.Domain.Entities;

// M06 Gestión documental
public class TipoDocumento : EntidadAuditable
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
}

public class Documento : EntidadAuditable
{
    public int Id { get; set; }
    public int TipoDocumentoId { get; set; }

    // Referencia polimórfica sin llave foránea (la valida la aplicación).
    public string? EntidadTipo { get; set; }
    public int? EntidadId { get; set; }
    public string NombreArchivo { get; set; } = "";
    public string UrlArchivo { get; set; } = "";
    public string HashSha256 { get; set; } = "";
    public int Version { get; set; } = 1;
    public bool EsPublico { get; set; }
    public int SubidoPorId { get; set; }
    public DateTime FechaSubida { get; set; }

    public TipoDocumento? TipoDocumento { get; set; }
    public Usuario? SubidoPor { get; set; }
}

/// <summary>Inmutable: sin columnas de auditoría ni UPDATE/DELETE.</summary>
public class DocumentoFirma
{
    public long Id { get; set; }
    public int DocumentoId { get; set; }
    public int FirmanteId { get; set; }
    public string RolFirmante { get; set; } = "";
    public string HashDocumento { get; set; } = "";
    public string? IpFirma { get; set; }
    public DateTime FechaFirma { get; set; }
    public string? HashEncadenado { get; set; }

    public Documento? Documento { get; set; }
    public Usuario? Firmante { get; set; }
}
