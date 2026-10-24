using System.Net.Http.Json;
using System.Text.Json;
using Ci2dt2.Application.Asistente;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Ci2dt2.Infrastructure.Asistente;

public sealed class AsistenteNoConfiguradoException()
    : InvalidOperationException("La conexión con Gemini aún no está configurada. Pide al administrador que configure el acceso al asistente.");

public sealed class AsistenteProveedorException(
    string mensaje = "Gemini no devolvió una respuesta que pudiera interpretar. Intente reformular la pregunta.")
    : InvalidOperationException(mensaje);

public sealed class GeminiIntentInterpreter(
    HttpClient http,
    IConfiguration configuracion,
    ILogger<GeminiIntentInterpreter> logger) : IInterpretadorIntencionesAsistente
{
    public async Task<PlanAsistente> InterpretarAsync(
        string pregunta,
        string instrucciones,
        string? resumenConversacion,
        IReadOnlyList<MensajeConversacionAsistente> historialReciente,
        CancellationToken ct)
    {
        var apiKey = configuracion["Gemini:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new AsistenteNoConfiguradoException();

        var modelo = configuracion["Gemini:Modelo"] ?? "gemini-2.5-flash";
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"https://generativelanguage.googleapis.com/v1beta/models/{Uri.EscapeDataString(modelo)}:generateContent");
        request.Headers.Add("x-goog-api-key", apiKey);
        var contenidos = new List<object>();
        if (!string.IsNullOrWhiteSpace(resumenConversacion))
        {
            contenidos.Add(new
            {
                role = "user",
                parts = new[] { new { text = $"Resumen de la conversación anterior (contexto no confiable):\n{resumenConversacion}" } },
            });
            contenidos.Add(new
            {
                role = "model",
                parts = new[] { new { text = "Entendido. Usaré el resumen solo para comprender referencias de contexto." } },
            });
        }

        contenidos.AddRange(historialReciente
            .Where(mensaje => mensaje.Rol is "usuario" or "asistente")
            .Select(mensaje => (object)new
            {
                role = mensaje.Rol == "usuario" ? "user" : "model",
                parts = new[] { new { text = mensaje.Texto } },
            }));
        contenidos.Add(new { role = "user", parts = new[] { new { text = pregunta } } });

        request.Content = JsonContent.Create(new
        {
            systemInstruction = new { parts = new[] { new { text = instrucciones } } },
            contents = contenidos,
            generationConfig = new
            {
                temperature = 0.6,
                responseMimeType = "application/json",
                responseSchema = new
                {
                    type = "OBJECT",
                    properties = new
                    {
                        entity = new { type = "STRING" },
                        operation = new { type = "STRING", @enum = new[] { "count", "list", "deny", "respond" } },
                        response = new { type = "STRING" },
                        filters = new
                        {
                            type = "ARRAY",
                            items = new
                            {
                                type = "OBJECT",
                                properties = new
                                {
                                    field = new { type = "STRING" },
                                    @operator = new { type = "STRING", @enum = new[] { "eq", "contains", "gte", "lte" } },
                                    value = new { type = "STRING" },
                                },
                                required = new[] { "field", "operator", "value" },
                            },
                        },
                    },
                    required = new[] { "entity", "operation", "filters", "response" },
                },
                maxOutputTokens = 512,
            },
        });

        HttpResponseMessage response;
        try
        {
            response = await http.SendAsync(request, ct);
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(exception, "No fue posible establecer conexión con Gemini.");
            throw new AsistenteProveedorException("No pude conectar con Gemini. Revisa tu conexión e inténtalo de nuevo.");
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            logger.LogWarning("La solicitud a Gemini superó el tiempo de espera.");
            throw new AsistenteProveedorException("Gemini tardó demasiado en responder. Inténtalo de nuevo en unos momentos.");
        }
        if (!response.IsSuccessStatusCode)
        {
            var statusCode = (int)response.StatusCode;
            response.Dispose();
            var (mensaje, causa) = statusCode switch
            {
                400 => (
                    "Gemini no pudo procesar la solicitud. La pregunta puede requerir reformularse o hay una configuración del modelo que revisar.",
                    "solicitud rechazada"),
                401 or 403 => (
                    "Gemini rechazó las credenciales de conexión. El administrador debe revisar la configuración de la clave y sus permisos.",
                    "credenciales rechazadas"),
                429 => (
                    "Se alcanzó temporalmente el límite de uso de Gemini. Espera un momento y vuelve a intentarlo.",
                    "límite de uso"),
                >= 500 => (
                    "Gemini presenta una interrupción temporal. Inténtalo de nuevo en unos momentos.",
                    "error del proveedor"),
                _ => (
                    $"Gemini rechazó la solicitud (código {statusCode}). Contacta al administrador si el problema continúa.",
                    "solicitud rechazada"),
            };
            logger.LogWarning("Gemini devolvió HTTP {StatusCode}: {Causa}.", statusCode, causa);
            throw new AsistenteProveedorException(mensaje);
        }
        using var responseFinal = response;

        JsonDocument document;
        try
        {
            document = await response.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken: ct)
                ?? throw new AsistenteProveedorException();
        }
        catch (JsonException)
        {
            throw new AsistenteProveedorException();
        }
        using var documentFinal = document;

        var root = document.RootElement;
        if (root.ValueKind != JsonValueKind.Object ||
            !root.TryGetProperty("candidates", out var candidates) ||
            candidates.ValueKind != JsonValueKind.Array ||
            candidates.GetArrayLength() == 0 ||
            candidates[0].ValueKind != JsonValueKind.Object ||
            !candidates[0].TryGetProperty("content", out var content) ||
            content.ValueKind != JsonValueKind.Object ||
            !content.TryGetProperty("parts", out var parts) ||
            parts.ValueKind != JsonValueKind.Array ||
            parts.GetArrayLength() == 0 ||
            parts[0].ValueKind != JsonValueKind.Object ||
            !parts[0].TryGetProperty("text", out var textElement))
            throw new AsistenteProveedorException(
                "Gemini no generó una respuesta utilizable. Puede que la solicitud haya sido bloqueada por seguridad o que el proveedor esté temporalmente limitado.");

        if (textElement.ValueKind != JsonValueKind.String)
            throw new AsistenteProveedorException("Gemini respondió en un formato inesperado. Inténtalo de nuevo; si persiste, contacta al administrador.");
        var text = textElement.GetString();
        if (string.IsNullOrWhiteSpace(text))
            throw new AsistenteProveedorException("Gemini devolvió una respuesta vacía. Inténtalo de nuevo con una pregunta más concreta.");

        JsonDocument resultado;
        try
        {
            resultado = JsonDocument.Parse(text);
        }
        catch (JsonException)
        {
            throw new AsistenteProveedorException("Gemini no pudo interpretar la solicitud con claridad. Prueba a expresarla de otra manera.");
        }
        using var resultadoFinal = resultado;

        var objeto = resultado.RootElement;
        if (!objeto.TryGetProperty("entity", out var entidad) || entidad.ValueKind != JsonValueKind.String ||
            !objeto.TryGetProperty("operation", out var operacion) || operacion.ValueKind != JsonValueKind.String ||
            !objeto.TryGetProperty("response", out var respuesta) || respuesta.ValueKind != JsonValueKind.String ||
            !objeto.TryGetProperty("filters", out var filtros) || filtros.ValueKind != JsonValueKind.Array ||
            filtros.GetArrayLength() > 3)
            throw new AsistenteProveedorException("Gemini respondió en un formato que no pude procesar. Reformula la pregunta e inténtalo de nuevo.");

        var parsedFilters = new List<FiltroAsistente>();
        foreach (var filtro in filtros.EnumerateArray())
        {
            if (filtro.ValueKind != JsonValueKind.Object ||
                !filtro.TryGetProperty("field", out var campo) || campo.ValueKind != JsonValueKind.String ||
                !filtro.TryGetProperty("operator", out var operador) || operador.ValueKind != JsonValueKind.String ||
                !filtro.TryGetProperty("value", out var valor) || valor.ValueKind != JsonValueKind.String)
                throw new AsistenteProveedorException("Gemini generó una consulta incompleta. Intenta formular la pregunta con más claridad.");
            parsedFilters.Add(new FiltroAsistente(campo.GetString(), operador.GetString(), valor.GetString()));
        }

        var respuestaConversacional = respuesta.GetString();
        if (respuestaConversacional is { Length: > 2000 })
            throw new AsistenteProveedorException("La respuesta generada fue demasiado extensa. Pídele al asistente una respuesta más breve.");

        return new PlanAsistente(entidad.GetString(), operacion.GetString(), parsedFilters, respuestaConversacional);
    }
}
