using System.Security.Cryptography;
using System.Text;
using Ci2dt2.Api.Infraestructura;
using Ci2dt2.Application.Cronogramas;
using Ci2dt2.Application.Notificaciones;
using Ci2dt2.Application.Seguridad;
using Ci2dt2.Application.Tablero;

namespace Ci2dt2.Api.Endpoints;

public sealed record ConteoNoLeidas(int NoLeidas);

public sealed record ResultadoJob(ResultadoAlertas Alertas, int CorreosEnviados);

/// <summary>HU-22 (bandeja), HU-18 (alertas y job protegido), HU-24 (tablero) y consentimiento de datos.</summary>
public sealed class NotificacionesEndpoints : IEndpointModule
{
    public const string EncabezadoApiKey = "X-Api-Key";

    public static void Map(IEndpointRouteBuilder app)
    {
        // ----- HU-22 -----
        var notificaciones = app.MapGroup("/api/v1/notificaciones").WithTags("Notificaciones")
            .RequireAuthorization(Politicas.VerNotificaciones)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        notificaciones.MapGet("/mias", async (bool? soloNoLeidas, int? pagina, NotificacionesService servicio, HttpContext http, CancellationToken ct) =>
                Results.Ok(await servicio.MiasAsync(http.User.UsuarioAutenticado().Id, soloNoLeidas ?? false, pagina ?? 1, ct)))
            .WithSummary("Mis notificaciones (paginadas) y el número de no leídas")
            .Produces<BandejaNotificaciones>();

        notificaciones.MapGet("/no-leidas", async (NotificacionesService servicio, HttpContext http, CancellationToken ct) =>
                Results.Ok(new ConteoNoLeidas(await servicio.ContarNoLeidasAsync(http.User.UsuarioAutenticado().Id, ct))))
            .WithSummary("Contador de notificaciones no leídas (campana)")
            .Produces<ConteoNoLeidas>();

        notificaciones.MapPost("/{id:int}/leer", async (int id, NotificacionesService servicio, HttpContext http, CancellationToken ct) =>
            {
                await servicio.LeerAsync(id, http.User.UsuarioAutenticado().Id, ct);
                return Results.NoContent();
            })
            .WithSummary("Marca una notificación como leída")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorApi>(StatusCodes.Status404NotFound);

        notificaciones.MapPost("/leer-todas", async (NotificacionesService servicio, HttpContext http, CancellationToken ct) =>
            {
                await servicio.LeerTodasAsync(http.User.UsuarioAutenticado().Id, ct);
                return Results.NoContent();
            })
            .WithSummary("Marca todas mis notificaciones como leídas")
            .Produces(StatusCodes.Status204NoContent);

        // ----- HU-18 -----
        app.MapGet("/api/v1/alertas/pendientes", async (AlertasService alertas, HttpContext http, CancellationToken ct) =>
                Results.Ok(await alertas.MisAlertasAsync(http.User.UsuarioAutenticado().Id, ct)))
            .WithTags("Alertas")
            .WithSummary("Mis alertas: hitos próximos (30 días) y vencidos de mis proyectos")
            .RequireAuthorization(Politicas.VerProyectos)
            .Produces<IReadOnlyList<AlertaHito>>();

        // El programador externo (cron) dispara aquí las alertas diarias y el despacho de correos.
        app.MapPost("/api/v1/jobs/alertas", async (HttpContext http, AlertasService alertas, DespachadorCorreos despachador,
                IConfiguration configuracion, CancellationToken ct) =>
            {
                var clave = configuracion["Trabajos:ApiKey"];
                if (string.IsNullOrWhiteSpace(clave) || !ClaveValida(http.Request.Headers[EncabezadoApiKey].ToString(), clave))
                    return Results.Json(new ErrorApi("Clave de API inválida."), statusCode: StatusCodes.Status401Unauthorized);
                var resultado = await alertas.EjecutarAsync(alertas.HoyColombia(), ct);
                var enviados = await despachador.ProcesarPendientesAsync(ct);
                return Results.Ok(new ResultadoJob(resultado, enviados));
            })
            .AllowAnonymous()
            .WithTags("Alertas")
            .WithSummary("Ejecuta el motor de alertas de hitos y el despacho de correos (protegido con X-Api-Key)")
            .Produces<ResultadoJob>()
            .Produces<ErrorApi>(StatusCodes.Status401Unauthorized);

        // ----- HU-24 -----
        var dashboard = app.MapGroup("/api/v1/dashboard").WithTags("Tablero")
            .RequireAuthorization(Politicas.VerTableroCentro)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        dashboard.MapGet("/centro", async (int? anio, TableroQueries tablero, CancellationToken ct) =>
                Results.Ok(await tablero.CentroAsync(anio, ct)))
            .WithSummary("Tablero del Director: tarjetas, proyectos por estado y programa, hitos y avances pendientes")
            .Produces<TableroCentro>();

        dashboard.MapGet("/hitos-proximos", async (TableroQueries tablero, CancellationToken ct) =>
            {
                var t = await tablero.CentroAsync(null, ct);
                return Results.Ok(new { proximos = t.HitosProximos, vencidos = t.HitosVencidos });
            })
            .WithSummary("Hitos próximos (30 días) y vencidos del centro");

        // ----- Política de tratamiento de datos (P17) -----
        var consentimientos = app.MapGroup("/api/v1/consentimientos").WithTags("Protección de datos");

        consentimientos.MapGet("/estado", async (ConsentimientosService servicio, HttpContext http, CancellationToken ct) =>
                Results.Ok(await servicio.EstadoAsync(http.User.UsuarioAutenticado().Id, ct)))
            .WithSummary("Política de datos vigente y si el usuario ya la aceptó")
            .Produces<EstadoConsentimiento>();

        consentimientos.MapPost("/", async (ConsentimientosService servicio, HttpContext http, CancellationToken ct) =>
            {
                await servicio.AceptarAsync(http.User.UsuarioAutenticado().Id, ct);
                return Results.NoContent();
            })
            .WithSummary("Registra la aceptación de la política de tratamiento de datos vigente")
            .Produces(StatusCodes.Status204NoContent);
    }

    private static bool ClaveValida(string recibida, string esperada) =>
        CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(recibida), Encoding.UTF8.GetBytes(esperada));
}
