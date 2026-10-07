namespace Ci2dt2.Domain.Entities;

// M06 Documentos
public class TipoDocumento
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
}

public class Documento
{
    public int Id { get; set; }
    public int TipoDocumentoId { get; set; }
    public string EntidadTipo { get; set; } = "";
    public int EntidadId { get; set; }
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
