using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Ci2dt2.Application.Asistente;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Ci2dt2.Infrastructure.Asistente;

public sealed class GeminiAgenteFormulacionService(
    HttpClient http,
    IConfiguration configuracion,
    ILogger<GeminiAgenteFormulacionService> logger) : IAgenteFormulacionService
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private string ObtenerApiKey()
    {
        var apiKey = configuracion["Gemini:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new AsistenteNoConfiguradoException();
        return apiKey;
    }

    private string ObtenerModelo() => configuracion["Gemini:Modelo"] ?? "gemini-3.5-flash";

    public async Task<AnalisisTdrResultado> AnalizarTdrEIdearAsync(
        AnalisisTdrRequest request,
        CancellationToken ct = default)
    {
        var apiKey = ObtenerApiKey();
        var modelo = ObtenerModelo();

        var instrucciones = """
            Eres un Evaluador Senior y Asesor Principal de Formulación de Proyectos de Investigación Científica y Desarrollo Tecnológico (estándar Minciencias / CI²DT²).
            Tu labor es doble:
            1. Analizar rigurosamente los Términos de Referencia (TDR) de una convocatoria académica y extraer un resumen estructurado.
            2. Proponer exactamente 3 ideas de proyectos de investigación innovadoras, de alto impacto y factibles, que articulen de forma armónica la convocatoria con la Línea de Investigación activa del investigador.

            Debes responder OBLIGATORIAMENTE en formato JSON con la siguiente estructura exacta:
            {
              "resumen": {
                "entidad": "Nombre de la entidad convocante",
                "objetivoConvocatoria": "Objetivo principal de la convocatoria",
                "montoMaximo": "Tope financiero o 'No especificado'",
                "rubrosFinanciables": ["rubro 1", "rubro 2"],
                "requisitosElegibilidad": ["requisito 1", "requisito 2"],
                "criteriosEvaluacion": ["criterio 1", "criterio 2"],
                "entregablesEsperados": ["entregable 1", "entregable 2"],
                "fechaCierre": "Fecha de cierre o 'Consultar cronograma oficial'"
              },
              "ideasPropuestas": [
                {
                  "id": "idea-1",
                  "titulo": "Título formal y académico del proyecto",
                  "descripcionBreve": "Resumen conciso del enfoque y alcance (2-3 oraciones)",
                  "justificacionAlineacion": "Cómo atiende puntualmente los requisitos del TDR y potencia la línea de investigación del usuario",
                  "impactoEsperado": "Impacto científico, tecnológico o social esperado",
                  "palabrasClave": ["palabra1", "palabra2", "palabra3"]
                }
              ]
            }
            """;

        var payloadUsuario = $"""
            INFORMACIÓN DEL INVESTIGADOR:
            - Nombre: {request.NombreInvestigador ?? "Investigador Principal"}
            - Programa Académico: {request.NombrePrograma ?? "CI²DT²"}
            - Línea de Investigación Activa: {request.LineaInvestigacionUsuario}

            DOCUMENTO DE TÉRMINOS DE REFERENCIA (TDR) DE LA CONVOCATORIA:
            {(string.IsNullOrWhiteSpace(request.TextoTdr) ? "No se adjuntó texto de TDR. Genera propuestas estratégicas generales alineadas a la línea." : request.TextoTdr)}
            """;

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"https://generativelanguage.googleapis.com/v1beta/models/{Uri.EscapeDataString(modelo)}:generateContent");
        httpRequest.Headers.Add("x-goog-api-key", apiKey);

        httpRequest.Content = JsonContent.Create(new
        {
            systemInstruction = new { parts = new[] { new { text = instrucciones } } },
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[] { new { text = payloadUsuario } }
                }
            },
            generationConfig = new
            {
                temperature = 0.4,
                responseMimeType = "application/json"
            }
        });

        var response = await http.SendAsync(httpRequest, ct);
        if (!response.IsSuccessStatusCode)
        {
            var detalle = await response.Content.ReadAsStringAsync(ct);
            logger.LogError("Gemini error {Code} al analizar TDR: {Detalle}", response.StatusCode, detalle);
            throw new AsistenteProveedorException($"Gemini devolvió error {(int)response.StatusCode} al analizar la convocatoria.");
        }

        var jsonDoc = await response.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken: ct)
            ?? throw new AsistenteProveedorException("Respuesta nula recibida de Gemini.");

        var textoJson = ExtraerTextoRespuesta(jsonDoc);
        try
        {
            var resultado = JsonSerializer.Deserialize<AnalisisTdrResultado>(textoJson, JsonOpts);
            if (resultado?.Resumen is null || resultado.IdeasPropuestas is null || resultado.IdeasPropuestas.Count == 0)
                throw new JsonException("Estructura JSON incompleta.");

            return resultado;
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Error al deserializar JSON de Gemini: {Raw}", textoJson);
            throw new AsistenteProveedorException("No se pudo interpretar el análisis estructurado de la convocatoria.");
        }
    }

    public async IAsyncEnumerable<string> RedactarSeccionStreamAsync(
        RedactarSeccionRequest request,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var apiKey = ObtenerApiKey();
        var modelo = ObtenerModelo();

        var seccionesPrevias = new StringBuilder();
        if (request.SeccionesExistentes is { Count: > 0 })
        {
            foreach (var (clave, contenido) in request.SeccionesExistentes)
            {
                if (!string.IsNullOrWhiteSpace(contenido) && clave != request.CodigoSeccion)
                {
                    seccionesPrevias.AppendLine($"### [Sección Previa: {clave.ToUpperInvariant()}]");
                    seccionesPrevias.AppendLine(contenido);
                    seccionesPrevias.AppendLine();
                }
            }
        }

        var instrucciones = """
            Eres un Redactor Científico Senior y Experto en Formulación de Proyectos de I+D+i.
            Tu misión es redactar la sección solicitada con el más alto rigor metodológico, lenguaje académico formal, coherencia interna y cumplimiento de las directrices del TDR de la convocatoria.
            
            REGLAS DE REDACCIÓN:
            - Usa formato Markdown estructurado (títulos nivel 3 `###`, viñetas, tablas comparativas si aplica).
            - Sé exhaustivo, fundamentado y cuantitativo cuando corresponda.
            - Si estás redactando 'Objetivos': formula un Objetivo General medible y objetivos específicos secuenciales (diagnóstico, desarrollo, validación).
            - Si estás redactando 'Metodología': define fases claras, actividades, técnicas o instrumentos y criterios de validación.
            - Asegura coherencia absoluta con las secciones previas ya redactadas.
            """;

        var promptUsuario = $"""
            PROYECTO: {request.TituloProyecto}
            IDEA / ENFOQUE SELECCIONADO: {request.IdeaSeleccionada ?? "Formulación estratégica"}
            SECCIÓN A REDACTAR: {request.CodigoSeccion.ToUpperInvariant()}
            
            METADATOS DE CONVOCATORIA (TDR):
            {request.ConvocatoriaMetadataJson ?? "Estándar institucional Minciencias"}

            SECCIONES YA CONSOLIDADAS EN EL BORRADOR (CONTEXTO DE COHERENCIA):
            {(seccionesPrevias.Length > 0 ? seccionesPrevias.ToString() : "Esta es una de las primeras secciones a redactar.")}

            INSTRUCCIONES ESPECÍFICAS DEL INVESTIGADOR:
            {(string.IsNullOrWhiteSpace(request.InstruccionesUsuario) ? "Redacta una propuesta completa y rigurosa." : request.InstruccionesUsuario)}
            """;

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"https://generativelanguage.googleapis.com/v1beta/models/{Uri.EscapeDataString(modelo)}:streamGenerateContent?alt=sse");
        httpRequest.Headers.Add("x-goog-api-key", apiKey);

        httpRequest.Content = JsonContent.Create(new
        {
            systemInstruction = new { parts = new[] { new { text = instrucciones } } },
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[] { new { text = promptUsuario } }
                }
            },
            generationConfig = new
            {
                temperature = 0.5,
                maxOutputTokens = 3500
            }
        });

        using var response = await http.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, ct);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(ct);
            logger.LogError("Gemini stream error {Code}: {Err}", response.StatusCode, err);
            yield return $"Error al conectar con el asistente de redacción (HTTP {(int)response.StatusCode}).";
            yield break;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream, Encoding.UTF8);

        while (await reader.ReadLineAsync(ct) is { } linea)
        {
            if (ct.IsCancellationRequested)
                break;

            if (string.IsNullOrWhiteSpace(linea) || !linea.StartsWith("data: "))
                continue;

            var jsonChunk = linea["data: ".Length..].Trim();
            if (jsonChunk == "[DONE]")
                break;

            string? textoGenerado = null;
            try
            {
                using var chunkDoc = JsonDocument.Parse(jsonChunk);
                textoGenerado = ExtraerTextoRespuesta(chunkDoc);
            }
            catch (JsonException)
            {
                // Línea SSE incompleta o latido
            }

            if (!string.IsNullOrEmpty(textoGenerado))
                yield return textoGenerado;
        }
    }

    public async Task<AuditoriaBorradorResultado> AuditarBorradorAsync(
        AuditoriaBorradorRequest request,
        CancellationToken ct = default)
    {
        var apiKey = ObtenerApiKey();
        var modelo = ObtenerModelo();

        var instrucciones = """
            Eres un Auditor Académico de Convocatorias de Investigación (Minciencias / CI²DT²).
            Tu tarea es analizar un borrador de proyecto cargado por un investigador, comparar su contenido con la estructura estándar requerida y detectar qué apartados están completos, cuáles incompletos y cuáles faltan totalmente.

            Apartados estándar a evaluar:
            1. 'titulo' (Título del Proyecto)
            2. 'resumen' (Resumen Ejecutivo)
            3. 'justificacion' (Planteamiento del Problema y Justificación)
            4. 'estado_arte' (Marco Teórico y Estado del Arte)
            5. 'objetivo_general' (Objetivo General)
            6. 'objetivos_especificos' (Objetivos Específicos)
            7. 'metodologia' (Metodología y Diseño Experimental)
            8. 'cronograma' (Plan de Trabajo y Cronograma)
            9. 'presupuesto' (Presupuesto y Rubros Solicitados)
            10. 'impactos' (Resultados Esperados e Impactos)

            Debes responder OBLIGATORIAMENTE en JSON con la siguiente estructura:
            {
              "diagnosticoGeneral": "Diagnóstico general del estado del borrador y ruta recomendada para completarlo",
              "secciones": [
                {
                  "codigoSeccion": "codigo",
                  "tituloSeccion": "Título de la sección",
                  "estado": "Completa | Incompleta | Faltante",
                  "contenidoDetectado": "Texto exacto o sintetizado extraído del borrador que corresponde a este apartado (o cadena vacía si no existe)",
                  "recomendaciones": "Observaciones para mejorar o completar la sección"
                }
              ]
            }
            """;

        var payloadUsuario = $"""
            TDR DE LA CONVOCATORIA (SI APLICA):
            {request.ConvocatoriaMetadataJson ?? "Convocatoria estándar de investigación"}

            CONTENIDO DEL BORRADOR PREVIO CARGADO:
            {request.TextoBorradorCompleto}
            """;

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"https://generativelanguage.googleapis.com/v1beta/models/{Uri.EscapeDataString(modelo)}:generateContent");
        httpRequest.Headers.Add("x-goog-api-key", apiKey);

        httpRequest.Content = JsonContent.Create(new
        {
            systemInstruction = new { parts = new[] { new { text = instrucciones } } },
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[] { new { text = payloadUsuario } }
                }
            },
            generationConfig = new
            {
                temperature = 0.2,
                responseMimeType = "application/json"
            }
        });

        var response = await http.SendAsync(httpRequest, ct);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(ct);
            logger.LogError("Gemini error {Code} en auditoría de borrador: {Err}", response.StatusCode, err);
            throw new AsistenteProveedorException("Error al auditar el borrador con Gemini.");
        }

        var jsonDoc = await response.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken: ct)
            ?? throw new AsistenteProveedorException("Respuesta vacía de auditoría.");

        var textoJson = ExtraerTextoRespuesta(jsonDoc);
        try
        {
            var auditoria = JsonSerializer.Deserialize<AuditoriaBorradorResultado>(textoJson, JsonOpts);
            if (auditoria?.Secciones is null)
                throw new JsonException("Estructura de secciones no encontrada.");
            return auditoria;
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Error al deserializar auditoría: {Raw}", textoJson);
            throw new AsistenteProveedorException("No se pudo interpretar el diagnóstico del borrador.");
        }
    }

    private static string ExtraerTextoRespuesta(JsonDocument doc)
    {
        var root = doc.RootElement;
        if (!root.TryGetProperty("candidates", out var candidates) ||
            candidates.ValueKind != JsonValueKind.Array ||
            candidates.GetArrayLength() == 0)
            return string.Empty;

        var primerCand = candidates[0];
        if (!primerCand.TryGetProperty("content", out var content) ||
            !content.TryGetProperty("parts", out var parts) ||
            parts.ValueKind != JsonValueKind.Array)
            return string.Empty;

        var sb = new StringBuilder();
        foreach (var parte in parts.EnumerateArray())
        {
            if (parte.TryGetProperty("text", out var textElem) && textElem.ValueKind == JsonValueKind.String)
            {
                sb.Append(textElem.GetString());
            }
        }

        return sb.ToString().Trim();
    }
}
