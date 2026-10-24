using System.Security.Claims;
using Ci2dt2.Application.Common;
using Ci2dt2.Application.Seguridad;
using Ci2dt2.Infrastructure.Asistente;
using Ci2dt2.Infrastructure.Seguridad;
using Microsoft.AspNetCore.Diagnostics;

namespace Ci2dt2.Api.Infraestructura;

/// <summary>Traduce las excepciones de la aplicación al formato estándar de error.</summary>
public sealed class ManejadorExcepciones(ILogger<ManejadorExcepciones> logger) : IExceptionHandler
{
    public static (int Estado, string Mensaje) Clasificar(Exception exception) => exception switch
    {
        ReglaNegocioException e => (StatusCodes.Status422UnprocessableEntity, e.Message),
        NoEncontradoException e => (StatusCodes.Status404NotFound, e.Message),
        AccesoDenegadoException e => (StatusCodes.Status403Forbidden, e.Message),
        AsistenteNoConfiguradoException or AsistenteProveedorException =>
            (StatusCodes.Status503ServiceUnavailable, exception.Message),
        BadHttpRequestException => (StatusCodes.Status400BadRequest, "La solicitud no tiene un formato válido."),
        _ => (StatusCodes.Status500InternalServerError, "Ocurrió un error inesperado. Intente nuevamente."),
    };

    public async ValueTask<bool> TryHandleAsync(HttpContext http, Exception exception, CancellationToken ct)
    {
        var (estado, mensaje) = Clasificar(exception);

        if (estado == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Error no controlado en {Ruta}", http.Request.Path);

        http.Response.StatusCode = estado;
        await http.Response.WriteAsJsonAsync(new ErrorApi(mensaje), ct);
        return true;
    }
}

/// <summary>
/// Registro automático de auditoría (HU-28). Intercepta POST, PUT, PATCH y DELETE y los eventos
/// de autenticación, además de los accesos denegados (403) de usuarios autenticados. Registra los
/// valores anteriores y nuevos que la petición guardó, sin contraseñas, hashes ni tokens. La
/// escritura es asíncrona (cola en memoria y worker en segundo plano).
/// Se ubica antes de UseAuthorization para capturar también las denegaciones por política.
/// </summary>
public sealed class AuditoriaMiddleware(RequestDelegate next)
{
    private static readonly string[] Mutaciones = ["POST", "PUT", "PATCH", "DELETE"];

    // El inicio de sesión lo audita AuthService con el correo y el resultado (HU-28 CA-3).
    private const string RutaLogin = "/api/v1/auth/login";
    private const string RutaConsultaAsistente = "/api/v1/asistente/consultas";

    public async Task InvokeAsync(HttpContext http, Auditoria auditoria, ICambiosPeticion cambios)
    {
        try
        {
            await next(http);
        }
        catch (Exception ex)
        {
            // El manejador global responde después; aquí se registra el estado que tendrá la respuesta.
            Registrar(http, auditoria, cambios, ManejadorExcepciones.Clasificar(ex).Estado);
            throw;
        }

        Registrar(http, auditoria, cambios, http.Response.StatusCode);
    }

    private static void Registrar(HttpContext http, Auditoria auditoria, ICambiosPeticion cambios, int estado)
    {
        var ruta = http.Request.Path;
        if (!ruta.StartsWithSegments("/api") || ruta.StartsWithSegments(RutaLogin))
            return;

        // Esta petición usa POST para no exponer la consulta en la URL, pero no modifica datos.
        var esMutacion = Mutaciones.Contains(http.Request.Method) &&
            !ruta.StartsWithSegments(RutaConsultaAsistente);
        var esDenegado = estado == StatusCodes.Status403Forbidden && http.User.Identity?.IsAuthenticated == true;
        if (!esMutacion && !esDenegado)
            return;

        var capturados = cambios.Obtener();
        var recurso = ruta.Value!.Split('/', StringSplitOptions.RemoveEmptyEntries).ElementAtOrDefault(2) ?? "Desconocido";
        var recursoId = http.Request.RouteValues["id"]?.ToString()
            ?? capturados.FirstOrDefault(c => c.Estado == "Added")?.Id;

        var anteriores = capturados.Where(c => c.Anteriores.Count > 0)
            .Select(c => new { entidad = c.Entidad, id = c.Id, valores = c.Anteriores }).ToList();
        var nuevos = capturados.Where(c => c.Nuevos.Count > 0)
            .Select(c => new { entidad = c.Entidad, id = c.Id, valores = c.Nuevos }).ToList();

        auditoria.Registrar(recurso, http.Request.Method, recursoId, estado,
            anteriores.Count > 0 ? anteriores : null,
            nuevos.Count > 0 ? nuevos : null);
    }
}

public sealed class ContextoPeticion(IHttpContextAccessor accesor) : IContextoPeticion
{
    public int? UsuarioId => accesor.HttpContext?.User.ObtenerId();
    public string? Ip => accesor.HttpContext?.Connection.RemoteIpAddress?.ToString();
    public string? Ruta => accesor.HttpContext is { } http ? $"{http.Request.Method} {http.Request.Path}" : null;
}

public static class ClaimsPrincipalExtensions
{
    public static int? ObtenerId(this ClaimsPrincipal usuario) =>
        int.TryParse(usuario.FindFirstValue(ClaimsCi2dt2.Id), out var id) ? id : null;

    public static UsuarioAutenticado UsuarioAutenticado(this ClaimsPrincipal usuario) =>
        new(usuario.ObtenerId() ?? throw new InvalidOperationException("Usuario no autenticado."),
            usuario.FindAll(ClaimsCi2dt2.Rol).Select(c => c.Value).ToArray());
}
