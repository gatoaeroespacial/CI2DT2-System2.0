namespace Ci2dt2.Application.Asistente;

public sealed record DocumentoPagina(int NumeroPagina, string Texto);

public sealed record DocumentoExtraido(
    string NombreArchivo,
    string Extension,
    string TextoCompleto,
    int TotalPaginas,
    int TotalPalabras,
    IReadOnlyList<DocumentoPagina> Paginas,
    long TamanoBytes);

/// <summary>
/// Extrae texto estructurado de documentos de convocatorias (TDR) y borradores previos (PDF y DOCX).
/// </summary>
public interface IDocumentParserService
{
    bool SoportaArchivo(string nombreArchivo);
    Task<DocumentoExtraido> ExtraerTextoAsync(Stream stream, string nombreArchivo, CancellationToken ct = default);
}

