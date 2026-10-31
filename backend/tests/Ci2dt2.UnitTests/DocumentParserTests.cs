using System.Text;
using Ci2dt2.Infrastructure.Asistente;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ci2dt2.UnitTests;

public class DocumentParserTests
{
    private readonly DocumentParserService _parser = new(NullLogger<DocumentParserService>.Instance);

    [Theory]
    [InlineData("terminos_referencia.pdf", true)]
    [InlineData("convocatoria_2026.docx", true)]
    [InlineData("ARCHIVO.PDF", true)]
    [InlineData("DOCUMENTO.DOCX", true)]
    [InlineData("programa.xlsx", false)]
    [InlineData("imagen.png", false)]
    [InlineData("script.sh", false)]
    public void SoportaArchivo_ValidaFormatosPermitidos(string nombre, bool esperado)
    {
        var resultado = _parser.SoportaArchivo(nombre);
        Assert.Equal(esperado, resultado);
    }

    [Fact]
    public async Task ExtraerTextoAsync_LanzaExcepcionParaFormatoNoSoportado()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("contenido de prueba"));
        await Assert.ThrowsAsync<NotSupportedException>(() =>
            _parser.ExtraerTextoAsync(stream, "archivo.txt"));
    }

    [Fact]
    public async Task ExtraerTextoAsync_ExtraeTextoDeDocxEnMemoria()
    {
        // Generar un DOCX válido en memoria
        using var memoryStream = new MemoryStream();
        using (var doc = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document, true))
        {
            var mainPart = doc.AddMainDocumentPart();
            mainPart.Document = new Document(
                new Body(
                    new Paragraph(new Run(new Text("Convocatoria Nacional de Investigación 2026"))),
                    new Paragraph(new Run(new Text("Objetivo: Financiar proyectos en Inteligencia Artificial."))),
                    new Table(
                        new TableRow(
                            new TableCell(new Paragraph(new Run(new Text("Rubro")))),
                            new TableCell(new Paragraph(new Run(new Text("Tope"))))
                        ),
                        new TableRow(
                            new TableCell(new Paragraph(new Run(new Text("Personal")))),
                            new TableCell(new Paragraph(new Run(new Text("$50.000.000"))))
                        )
                    )
                )
            );
            mainPart.Document.Save();
        }

        memoryStream.Position = 0;
        var resultado = await _parser.ExtraerTextoAsync(memoryStream, "TDR_Prueba.docx");

        Assert.Equal(".docx", resultado.Extension);
        Assert.Contains("Convocatoria Nacional de Investigación 2026", resultado.TextoCompleto);
        Assert.Contains("Objetivo: Financiar proyectos en Inteligencia Artificial", resultado.TextoCompleto);
        Assert.Contains("| Personal | $50.000.000 |", resultado.TextoCompleto);
        Assert.True(resultado.TotalPalabras > 0);
    }
}

