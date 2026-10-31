using System.Text.RegularExpressions;
using Ci2dt2.Application.Asistente;
using Ci2dt2.Application.Common;
using Ci2dt2.Application.Seguridad;
using Ci2dt2.Domain;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ci2dt2.Infrastructure.Asistente;

public sealed partial class ExportadorBorradorDocxService(
    IApplicationDbContext db,
    ILogger<ExportadorBorradorDocxService> logger) : IExportadorBorradorService
{
    public async Task<(byte[] Contenido, string NombreArchivo)> GenerarDocxAsync(
        int borradorId,
        UsuarioAutenticado usuario,
        CancellationToken ct = default)
    {
        var borrador = await db.BorradoresProyecto
            .Include(b => b.Usuario)
            .Include(b => b.Programa)
            .Include(b => b.Secciones.Where(s => s.Activo))
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == borradorId && b.Activo, ct);

        if (borrador is null)
            throw new KeyNotFoundException($"No se encontró el borrador #{borradorId}.");

        if (borrador.UsuarioId != usuario.Id &&
            !usuario.Roles.Contains(Roles.Administrador) &&
            !usuario.Roles.Contains(Roles.Director))
        {
            throw new UnauthorizedAccessException("No tiene permisos para exportar este borrador.");
        }

        using var memoryStream = new MemoryStream();
        using (var wordDoc = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document, true))
        {
            var mainPart = wordDoc.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = new Body();

            // 1. Encabezado Institucional
            body.AppendChild(CrearParrafo(
                "CENTRO DE INVESTIGACIÓN E INNOVACIÓN EN DESARROLLO Y TRANSFERENCIA TECNOLÓGICA (CI²DT²)",
                negrita: true,
                tamanoFuente: 20,
                colorHex: "047857",
                alineacion: JustificationValues.Center));

            body.AppendChild(CrearParrafo(
                "PROPUESTA DE PROYECTO DE INVESTIGACIÓN",
                negrita: true,
                tamanoFuente: 24,
                colorHex: "1E293B",
                alineacion: JustificationValues.Center));

            body.AppendChild(CrearSeparador());

            // 2. Título del Proyecto
            body.AppendChild(CrearParrafo(
                borrador.TituloTentativo,
                negrita: true,
                tamanoFuente: 32,
                colorHex: "0F172A",
                alineacion: JustificationValues.Center));

            body.AppendChild(CrearEspacio());

            // 3. Ficha Técnica / Metadatos en Tabla
            var tablaFicha = new Table();
            AplicarBordesTabla(tablaFicha);

            var nombreInvestigador = borrador.Usuario is not null
                ? $"{borrador.Usuario.Nombres} {borrador.Usuario.Apellidos}".Trim()
                : $"Investigador #{borrador.UsuarioId}";

            AgregarFilaTabla(tablaFicha, "Investigador Principal:", nombreInvestigador);
            AgregarFilaTabla(tablaFicha, "Línea de Investigación:", borrador.LineaInvestigacion ?? "CI²DT² General");
            if (borrador.Programa is not null)
                AgregarFilaTabla(tablaFicha, "Programa Asociado:", borrador.Programa.Nombre);
            if (!string.IsNullOrWhiteSpace(borrador.ConvocatoriaNombre))
                AgregarFilaTabla(tablaFicha, "Convocatoria:", borrador.ConvocatoriaNombre);
            AgregarFilaTabla(tablaFicha, "Fecha de Generación:", DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm 'UTC'"));
            AgregarFilaTabla(tablaFicha, "Estado de Formulación:", borrador.EstadoAvance);

            body.AppendChild(tablaFicha);
            body.AppendChild(CrearEspacio());
            body.AppendChild(CrearSeparador());

            // 4. Secciones del Proyecto
            var seccionesOrdenadas = borrador.Secciones.OrderBy(s => s.Orden).ToList();

            foreach (var seccion in seccionesOrdenadas)
            {
                // Título de la Sección
                body.AppendChild(CrearParrafo(
                    $"{seccion.Orden}. {seccion.TituloSeccion}",
                    negrita: true,
                    tamanoFuente: 24,
                    colorHex: "065F46",
                    espacioAntes: 300,
                    espacioDespues: 100));

                var estadoTexto = $"[Estado: {seccion.Estado}]";
                body.AppendChild(CrearParrafo(
                    estadoTexto,
                    cursiva: true,
                    tamanoFuente: 16,
                    colorHex: "64748B",
                    espacioDespues: 150));

                if (string.IsNullOrWhiteSpace(seccion.Contenido))
                {
                    body.AppendChild(CrearParrafo(
                        "(Apartado pendiente de redacción)",
                        cursiva: true,
                        tamanoFuente: 18,
                        colorHex: "94A3B8"));
                }
                else
                {
                    ProcesarTextoContenido(body, seccion.Contenido);
                }

                body.AppendChild(CrearEspacio());
            }

            mainPart.Document.AppendChild(body);
            mainPart.Document.Save();
        }

        var bytes = memoryStream.ToArray();
        var nombreSanitizado = SanitizarNombreArchivo(borrador.TituloTentativo);
        var nombreFinal = $"{nombreSanitizado}_Borrador.docx";

        logger.LogInformation("DOCX para borrador #{Id} generado exitosamente ({Bytes} bytes).", borradorId, bytes.Length);

        return (bytes, nombreFinal);
    }

    private static void ProcesarTextoContenido(Body body, string contenido)
    {
        var lineas = contenido.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);
        var enTabla = false;
        var lineasTabla = new List<string>();

        foreach (var linea in lineas)
        {
            var lineaTrim = linea.Trim();

            // Detección de tablas markdown (| col1 | col2 |)
            if (lineaTrim.StartsWith('|') && lineaTrim.EndsWith('|'))
            {
                enTabla = true;
                if (!lineaTrim.Contains("---")) // Ignorar línea divisoria markdown
                    lineasTabla.Add(lineaTrim);
                continue;
            }

            if (enTabla && lineasTabla.Count > 0)
            {
                GenerarTablaMarkdown(body, lineasTabla);
                lineasTabla.Clear();
                enTabla = false;
            }

            if (string.IsNullOrWhiteSpace(lineaTrim))
            {
                body.AppendChild(CrearEspacio());
                continue;
            }

            // Subtítulos markdown (###, ##, #)
            if (lineaTrim.StartsWith("### "))
            {
                body.AppendChild(CrearParrafo(
                    lineaTrim[4..],
                    negrita: true,
                    tamanoFuente: 20,
                    colorHex: "1E293B",
                    espacioAntes: 180,
                    espacioDespues: 60));
            }
            else if (lineaTrim.StartsWith("## "))
            {
                body.AppendChild(CrearParrafo(
                    lineaTrim[3..],
                    negrita: true,
                    tamanoFuente: 22,
                    colorHex: "0F172A",
                    espacioAntes: 220,
                    espacioDespues: 80));
            }
            else if (lineaTrim.StartsWith("- ") || lineaTrim.StartsWith("* "))
            {
                // Elemento de viñeta
                body.AppendChild(CrearParrafo(
                    $"•  {lineaTrim[2..]}",
                    tamanoFuente: 20,
                    sangriaIzquierda: 360));
            }
            else
            {
                // Párrafo de texto estándar
                body.AppendChild(CrearParrafo(
                    lineaTrim,
                    tamanoFuente: 20,
                    alineacion: JustificationValues.Both,
                    espacioDespues: 120));
            }
        }

        if (enTabla && lineasTabla.Count > 0)
        {
            GenerarTablaMarkdown(body, lineasTabla);
        }
    }

    private static void GenerarTablaMarkdown(Body body, List<string> lineas)
    {
        var tabla = new Table();
        AplicarBordesTabla(tabla);

        var esEncabezado = true;
        foreach (var linea in lineas)
        {
            var celdas = linea.Split('|', StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Trim())
                .ToList();

            var fila = new TableRow();
            foreach (var celda in celdas)
            {
                var tc = new TableCell();
                tc.AppendChild(CrearParrafo(
                    celda,
                    negrita: esEncabezado,
                    tamanoFuente: esEncabezado ? 18 : 16,
                    colorHex: esEncabezado ? "047857" : "1E293B"));
                fila.AppendChild(tc);
            }
            tabla.AppendChild(fila);
            esEncabezado = false;
        }

        body.AppendChild(tabla);
        body.AppendChild(CrearEspacio());
    }

    private static Paragraph CrearParrafo(
        string texto,
        bool negrita = false,
        bool cursiva = false,
        int tamanoFuente = 20,
        string colorHex = "000000",
        JustificationValues? alineacion = null,
        int espacioAntes = 0,
        int espacioDespues = 0,
        int sangriaIzquierda = 0)
    {
        var parrafo = new Paragraph();
        var pPr = new ParagraphProperties();

        if (alineacion.HasValue)
        {
            pPr.AppendChild(new Justification { Val = alineacion.Value });
        }

        if (espacioAntes > 0 || espacioDespues > 0)
        {
            pPr.AppendChild(new SpacingBetweenLines
            {
                Before = espacioAntes > 0 ? espacioAntes.ToString() : "0",
                After = espacioDespues > 0 ? espacioDespues.ToString() : "0",
                Line = "240",
                LineRule = LineSpacingRuleValues.Auto
            });
        }

        if (sangriaIzquierda > 0)
        {
            pPr.AppendChild(new Indentation { Left = sangriaIzquierda.ToString() });
        }

        parrafo.AppendChild(pPr);

        var run = new Run();
        var rPr = new RunProperties();
        if (negrita) rPr.AppendChild(new Bold());
        if (cursiva) rPr.AppendChild(new Italic());
        rPr.AppendChild(new FontSize { Val = tamanoFuente.ToString() });
        rPr.AppendChild(new Color { Val = colorHex });
        rPr.AppendChild(new RunFonts { Ascii = "Calibri", HighAnsi = "Calibri" });

        run.AppendChild(rPr);
        run.AppendChild(new Text(texto));
        parrafo.AppendChild(run);

        return parrafo;
    }

    private static Paragraph CrearEspacio() =>
        new(new ParagraphProperties(new SpacingBetweenLines { After = "120" }));

    private static Paragraph CrearSeparador()
    {
        var p = new Paragraph();
        var pPr = new ParagraphProperties(
            new ParagraphBorders(new BottomBorder
            {
                Val = BorderValues.Single,
                Size = 12,
                Space = 4,
                Color = "E2E8F0"
            }),
            new SpacingBetweenLines { After = "200" });
        p.AppendChild(pPr);
        return p;
    }

    private static void AplicarBordesTabla(Table tabla)
    {
        var tblPr = new TableProperties(
            new TableWidth { Type = TableWidthUnitValues.Pct, Width = "5000" },
            new TableBorders(
                new TopBorder { Val = BorderValues.Single, Size = 4, Color = "CBD5E1" },
                new BottomBorder { Val = BorderValues.Single, Size = 4, Color = "CBD5E1" },
                new LeftBorder { Val = BorderValues.None },
                new RightBorder { Val = BorderValues.None },
                new InsideHorizontalBorder { Val = BorderValues.Single, Size = 4, Color = "F1F5F9" },
                new InsideVerticalBorder { Val = BorderValues.None }
            )
        );
        tabla.AppendChild(tblPr);
    }

    private static void AgregarFilaTabla(Table tabla, string etiqueta, string valor)
    {
        var fila = new TableRow();

        var celdaEtiqueta = new TableCell();
        celdaEtiqueta.AppendChild(new TableCellProperties(new TableCellWidth { Type = TableWidthUnitValues.Pct, Width = "1500" }));
        celdaEtiqueta.AppendChild(CrearParrafo(etiqueta, negrita: true, tamanoFuente: 18, colorHex: "334155"));

        var celdaValor = new TableCell();
        celdaValor.AppendChild(new TableCellProperties(new TableCellWidth { Type = TableWidthUnitValues.Pct, Width = "3500" }));
        celdaValor.AppendChild(CrearParrafo(valor, tamanoFuente: 18, colorHex: "0F172A"));

        fila.AppendChild(celdaEtiqueta);
        fila.AppendChild(celdaValor);
        tabla.AppendChild(fila);
    }

    private static string SanitizarNombreArchivo(string nombre)
    {
        var invalido = RegexCaractereInvalidos().Replace(nombre, "_");
        if (invalido.Length > 50)
            invalido = invalido[..50];
        return string.IsNullOrWhiteSpace(invalido) ? "Proyecto" : invalido.Trim('_');
    }

    [GeneratedRegex(@"[^\w\-\.]+", RegexOptions.Compiled)]
    private static partial Regex RegexCaractereInvalidos();
}
