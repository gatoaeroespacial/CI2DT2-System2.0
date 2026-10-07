using Ci2dt2.Api.Infraestructura;
using Ci2dt2.Application.Seguridad;

namespace Ci2dt2.Api.Endpoints;

public sealed record RespuestaSesion(string AccessToken, DateTime ExpiraEn, UsuarioSesion Usuario);

/// <summary>HU-01 y HU-02: inicio de sesión, renovación, cierre de sesión y definición de contraseña.</summary>
public sealed class AuthEndpoints : IEndpointModule
{
    /// <summary>Cookie HttpOnly con el token de refresco; solo viaja a las rutas de autenticación.</summary>
    public const string CookieRefresco = "ci2dt2_rt";
    private const string RutaCookie = "/api/v1/auth";

    public static void Map(IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/v1/auth").WithTags("Autenticación").AllowAnonymous();

        grupo.MapPost("/login", Login)
            .Validar<LoginRequest>()
            .WithSummary("Inicia sesión con correo y contraseña")
            .Produces<RespuestaSesion>()
            .Produces<ErrorApi>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorApi>(StatusCodes.Status403Forbidden)
            .Produces<ErrorApi>(StatusCodes.Status429TooManyRequests);

        grupo.MapPost("/refresh", Refrescar)
            .WithSummary("Renueva la sesión con el token de refresco (cookie) y lo rota")
            .Produces<RespuestaSesion>()
            .Produces<ErrorApi>(StatusCodes.Status401Unauthorized);

        grupo.MapPost("/logout", CerrarSesion)
            .WithSummary("Cierra la sesión y revoca el token de refresco actual")
            .Produces(StatusCodes.Status204NoContent);

        grupo.MapPost("/reset-password", DefinirContrasena)
            .Validar<DefinirContrasenaRequest>()
            .WithSummary("Define la contraseña con el enlace enviado por correo")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorApi>(StatusCodes.Status422UnprocessableEntity);
    }

    private static async Task<IResult> Login(LoginRequest request, AuthService auth, HttpContext http, CancellationToken ct)
    {
        var resultado = await auth.LoginAsync(request, Ip(http), ct);
        switch (resultado.Resultado)
        {
            case ResultadoLogin.Exitoso:
                return Sesion(http, resultado.Sesion!);

            case ResultadoLogin.CuentaInactiva:
                return Results.Json(new ErrorApi("Su cuenta está inactiva. Contacte al administrador del sistema"),
                    statusCode: StatusCodes.Status403Forbidden);

            case ResultadoLogin.CuentaBloqueada:
                var segundos = (int)Math.Ceiling(resultado.TiempoBloqueo!.Value.TotalSeconds);
                var minutos = (int)Math.Ceiling(segundos / 60.0);
                http.Response.Headers.RetryAfter = segundos.ToString();
                return Results.Json(
                    new ErrorApi($"Su cuenta está bloqueada temporalmente por intentos fallidos. Intente nuevamente en {minutos} minuto(s).",
                        SegundosRestantes: segundos),
                    statusCode: StatusCodes.Status429TooManyRequests);

            default:
                return Results.Json(new ErrorApi("Correo o contraseña incorrectos"), statusCode: StatusCodes.Status401Unauthorized);
        }
    }

    private static async Task<IResult> Refrescar(AuthService auth, HttpContext http, CancellationToken ct)
    {
        var sesion = await auth.RefrescarAsync(http.Request.Cookies[CookieRefresco], Ip(http), ct);
        if (sesion is null)
        {
            BorrarCookie(http);
            return Results.Json(new ErrorApi("Su sesión expiró. Inicie sesión nuevamente"), statusCode: StatusCodes.Status401Unauthorized);
        }
        return Sesion(http, sesion);
    }

    private static async Task<IResult> CerrarSesion(AuthService auth, HttpContext http, CancellationToken ct)
    {
        await auth.CerrarSesionAsync(http.Request.Cookies[CookieRefresco], ct);
        BorrarCookie(http);
        return Results.NoContent();
    }

    private static async Task<IResult> DefinirContrasena(DefinirContrasenaRequest request, AuthService auth, CancellationToken ct)
    {
        await auth.DefinirContrasenaAsync(request, ct);
        return Results.NoContent();
    }

    private static IResult Sesion(HttpContext http, SesionEmitida sesion)
    {
        http.Response.Cookies.Append(CookieRefresco, sesion.RefreshToken, OpcionesCookie(sesion.RefreshTokenExpiraEn));
        return Results.Ok(new RespuestaSesion(sesion.AccessToken, sesion.AccessTokenExpiraEn, sesion.Usuario));
    }

    private static void BorrarCookie(HttpContext http) =>
        http.Response.Cookies.Delete(CookieRefresco, OpcionesCookie(null));

    private static CookieOptions OpcionesCookie(DateTime? expira) => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Path = RutaCookie,
        Expires = expira,
        IsEssential = true,
    };

    private static string? Ip(HttpContext http) => http.Connection.RemoteIpAddress?.ToString();
}
