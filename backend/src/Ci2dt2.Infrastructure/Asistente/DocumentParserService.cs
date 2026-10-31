using System.Text;
using System.Text.RegularExpressions;
using Ci2dt2.Application.Asistente;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Logging;
using UglyToad.PdfPig;

namespace Ci2dt2.Infrastructure.Asistente;

public sealed partial class DocumentParserService(ILogger<DocumentParserService> logger) : IDocumentParserService
{
    private static readonly HashSet<string> ExtensionesSoportadas = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf",
        ".docx"
    };

    public bool SoportaArchivo(string nombreArchivo)
    {
        var ext = Path.GetExtension(nombreArchivo);
        return !string.IsNullOrWhiteSpace(ext) && ExtensionesSoportadas.Contains(ext);
    }

    public async Task<DocumentoExtraido> ExtraerTextoAsync(
        Stream stream,
        string nombreArchivo,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (string.IsNullOrWhiteSpace(nombreArchivo))
            throw new ArgumentException("El nombre del archivo no puede estar vacío.", nameof(nombreArchivo));

        var extension = Path.GetExtension(nombreArchivo).ToLowerInvariant();
        if (!ExtensionesSoportadas.Contains(extension))
            throw new NotSupportedException($"El formato '{extension}' no es compatible. Suba un archivo PDF o DOCX.");

        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream, ct);
        memoryStream.Position = 0;
        var tamanoBytes = memoryStream.Length;

        return extension switch
        {
            ".pdf" => ExtraerPdf(memoryStream, nombreArchivo, tamanoBytes),
            ".docx" => ExtraerDocx(memoryStream, nombreArchivo, tamanoBytes),
            _ => throw new NotSupportedException($"Formato no soportado: {extension}")
        };
    }

    private DocumentoExtraido ExtraerPdf(MemoryStream stream, string nombreArchivo, long tamanoBytes)
    {
        try
        {
            using var document = PdfDocument.Open(stream);
            var paginas = new List<DocumentoPagina>();
            var textoBuilder = new StringBuilder();

            foreach (var pagina in document.GetPages())
            {
                var textoPagina = LimpiarTexto(pagina.Text);
                paginas.Add(new DocumentoPagina(pagina.Number, textoPagina));
                if (!string.IsNullOrWhiteSpace(textoPagina))
                {
                    textoBuilder.AppendLine($"--- PÁGINA {pagina.Number} ---");
                    textoBuilder.AppendLine(textoPagina);
                    textoBuilder.AppendLine();
                }
            }

            var textoCompleto = textoBuilder.ToString().Trim();
            var totalPalabras = ContarPalabras(textoCompleto);

            logger.LogInformation("PDF '{Nombre}' procesado exitosamente: {Paginas} páginas, {Palabras} palabras.",
                nombreArchivo, paginas.Count, totalPalabras);

            return new DocumentoExtraido(
                NombreArchivo: nombreArchivo,
                Extension: ".pdf",
                TextoCompleto: textoCompleto,
                TotalPaginas: paginas.Count,
                TotalPalabras: totalPalabras,
                Paginas: paginas,
                TamanoBytes: tamanoBytes);
        }
        catch (Exception ex) when (ex is not NotSupportedException)
        {
            logger.LogError(ex, "Error al procesar el archivo PDF '{Nombre}'.", nombreArchivo);
            throw new InvalidOperationException($"No se pudo leer el contenido del PDF '{nombreArchivo}'. Verifique que no tenga contraseña o esté dañado.", ex);
        }
    }

    private DocumentoExtraido ExtraerDocx(MemoryStream stream, string nombreArchivo, long tamanoBytes)
    {
        try
        {
            using var wordDoc = WordprocessingDocument.Open(stream, false);
            var body = wordDoc.MainDocumentPart?.Document?.Body;
            if (body is null)
            {
                return new DocumentoExtraido(nombreArchivo, ".docx", string.Empty, 1, 0, [], tamanoBytes);
            }

            var textoBuilder = new StringBuilder();
            var elementos = body.Elements();

            foreach (var elemento in elementos)
            {
                if (elemento is Paragraph p)
                {
                    var textoParrafo = LimpiarTexto(p.InnerText);
                    if (!string.IsNullOrWhiteSpace(textoParrafo))
                        textoBuilder.AppendLine(textoParrafo);
                }
                else if (elemento is Table tabla)
                {
                    textoBuilder.AppendLine();
                    foreach (var fila in tabla.Elements<TableRow>())
                    {
                        var celdas = fila.Elements<TableCell>()
                            .Select(c => LimpiarTexto(c.InnerText).Replace("\r", " ").Replace("\n", " "))
                            .ToList();
                        if (celdas.Count > 0)
                            textoBuilder.AppendLine("| " + string.Join(" | ", celdas) + " |");
                    }
                    textoBuilder.AppendLine();
                }
            }

            var textoCompleto = textoBuilder.ToString().Trim();
            var totalPalabras = ContarPalabras(textoCompleto);
            var paginasEstimadas = Math.Max(1, (int)Math.Ceiling(totalPalabras / 450.0));

            logger.LogInformation("DOCX '{Nombre}' procesado exitosamente: ~{Paginas} páginas estimadas, {Palabras} palabras.",
                nombreArchivo, paginasEstimadas, totalPalabras);

            return new DocumentoExtraido(
                NombreArchivo: nombreArchivo,
                Extension: ".docx",
                TextoCompleto: textoCompleto,
                TotalPaginas: paginasEstimadas,
                TotalPalabras: totalPalabras,
                Paginas: [new DocumentoPagina(1, textoCompleto)],
                TamanoBytes: tamanoBytes);
        }
        catch (Exception ex) when (ex is not NotSupportedException)
        {
            logger.LogError(ex, "Error al procesar el archivo Word DOCX '{Nombre}'.", nombreArchivo);
            throw new InvalidOperationException($"No se pudo leer el archivo Word '{nombreArchivo}'. Verifique que no esté protegido o dañado.", ex);
        }
    }

    private static string LimpiarTexto(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return string.Empty;

        // Normaliza espacios múltiples y saltos redundantes
        var normalizado = EspaciosMultiplesRegex().Replace(texto, " ");
        return SaltosMultiplesRegex().Replace(normalizado, "\n\n").Trim();
    }

    private static int ContarPalabras(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return 0;

        return PalabrasRegex().Matches(texto).Count;
    }

    [GeneratedRegex(@"[^\S\r\n]+", RegexOptions.Compiled)]
    private static partial Regex EspaciosMultiplesRegex();

    [GeneratedRegex(@"(\r?\n){3,}", RegexOptions.Compiled)]
    private static partial Regex SaltosMultiplesRegex();

    [GeneratedRegex(@"\b[\w\-áéíóúÁÉÍÓÚñÑ]+\b", RegexOptions.Compiled)]
    private static partial Regex PalabrasRegex();
}

