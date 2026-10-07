using System.Security.Claims;
using Ci2dt2.Application.Common;
using Ci2dt2.Application.Seguridad;
using Ci2dt2.Infrastructure.Seguridad;
using Microsoft.AspNetCore.Diagnostics;

namespace Ci2dt2.Api.Infraestructura;

/// <summary>Traduce las excepciones de la aplicación al formato estándar de error.</summary>
public sealed class ManejadorExcepciones(ILogger<ManejadorExcepciones> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext http, Exception exception, CancellationToken ct)
    {
        var (estado, mensaje) = exception switch
        {
            ReglaNegocioException e => (StatusCodes.Status422UnprocessableEntity, e.Message),
            NoEncontradoException e => (StatusCodes.Status404NotFound, e.Message),
            BadHttpRequestException => (StatusCodes.Status400BadRequest, "La solicitud no tiene un formato válido."),
            _ => (StatusCodes.Status500InternalServerError, "Ocurrió un error inesperado. Intente nuevamente."),
        };

        if (estado == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Error no controlado en {Ruta}", http.Request.Path);

        http.Response.StatusCode = estado;
        await http.Response.WriteAsJsonAsync(new ErrorApi(mensaje), ct);
        return true;
    }
}

/// <summary>
/// Registra en LogActividades los accesos denegados (HTTP 403) a cualquier endpoint (HU-05 CA-1).
/// Se ubica antes de UseAuthorization para capturar también las denegaciones por política.
/// </summary>
public sealed class AuditoriaAccesoDenegadoMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext http, Auditoria auditoria, IApplicationDbContext db)
    {
        await next(http);

        // Solo usuarios autenticados: los anónimos reciben 401 y el login audita sus propios resultados.
        if (http.Response.StatusCode != StatusCodes.Status403Forbidden || http.User.Identity?.IsAuthenticated != true)
            return;

        var recurso = http.Request.Path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries).ElementAtOrDefault(2) ?? "Desconocido";
        auditoria.Registrar(recurso, http.Request.Method, http.Request.RouteValues["id"]?.ToString(), StatusCodes.Status403Forbidden);
        await db.SaveChangesAsync(http.RequestAborted);
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
