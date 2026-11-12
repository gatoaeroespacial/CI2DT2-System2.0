using Ci2dt2.Api.Infraestructura;
using Ci2dt2.Application.Avances;
using Ci2dt2.Application.Common;
using Ci2dt2.Application.Convocatorias;
using Ci2dt2.Application.Cronogramas;
using Ci2dt2.Application.Seguridad;
using Microsoft.AspNetCore.Mvc;

namespace Ci2dt2.Api.Endpoints;

/// <summary>HU-15 (convocatorias), HU-17 (hitos) y HU-19/HU-20 (avances y su validación).</summary>
public sealed class SeguimientoEndpoints : IEndpointModule
{
    public static void Map(IEndpointRouteBuilder app)
    {
        // ----- HU-15 -----
        var convocatorias = app.MapGroup("/api/v1/convocatorias").WithTags("Convocatorias")
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        // Los investigadores las consultan para vincular sus propuestas.
        convocatorias.MapGet("/", async (int? tipoConvocatoriaId, int? entidadConvocanteId, string? estado, string? busqueda, int? pagina,
                ConvocatoriasService servicio, CancellationToken ct) =>
                Results.Ok(await servicio.ListarAsync(new FiltroConvocatorias(tipoConvocatoriaId, entidadConvocanteId, estado, busqueda, pagina ?? 1), ct)))
            .WithSummary("Convocatorias con estado calculado por fechas (Próxima, Abierta, Cerrada) y filtros")
            .RequireAuthorization(Politicas.VerProyectos)
            .Produces<Pagina<ConvocatoriaItem>>();

        convocatorias.MapGet("/{id:int}", async (int id, ConvocatoriasService servicio, CancellationToken ct) =>
                await servicio.ObtenerAsync(id, ct) is { } c ? Results.Ok(c) : Results.NotFound(new ErrorApi("La convocatoria no existe.")))
            .WithSummary("Detalle de la convocatoria")
            .RequireAuthorization(Politicas.VerProyectos)
            .Produces<ConvocatoriaItem>()
            .Produces<ErrorApi>(StatusCodes.Status404NotFound);

        convocatorias.MapPost("/", async (ConvocatoriaRequest request, ConvocatoriasService servicio, CancellationToken ct) =>
            {
                var id = await servicio.CrearAsync(request, ct);
                return Results.Created($"/api/v1/convocatorias/{id}", new { id });
            })
            .Validar<ConvocatoriaRequest>()
            .WithSummary("Registra una convocatoria")
            .RequireAuthorization(Politicas.GestionarAliadosConvocatorias)
            .Produces(StatusCodes.Status201Created)
            .Produces<ErrorApi>(StatusCodes.Status422UnprocessableEntity);

        convocatorias.MapPut("/{id:int}", async (int id, ConvocatoriaRequest request, ConvocatoriasService servicio, CancellationToken ct) =>
            {
                await servicio.ActualizarAsync(id, request, ct);
                return Results.NoContent();
            })
            .Validar<ConvocatoriaRequest>()
            .WithSummary("Actualiza una convocatoria")
            .RequireAuthorization(Politicas.GestionarAliadosConvocatorias)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorApi>(StatusCodes.Status422UnprocessableEntity);

        convocatorias.MapDelete("/{id:int}", async (int id, ConvocatoriasService servicio, CancellationToken ct) =>
            {
                await servicio.EliminarAsync(id, ct);
                return Results.NoContent();
            })
            .WithSummary("Elimina una convocatoria sin postulaciones (borrado lógico)")
            .RequireAuthorization(Politicas.GestionarAliadosConvocatorias)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorApi>(StatusCodes.Status422UnprocessableEntity);

        // ----- HU-17 -----
        var hitos = app.MapGroup("/api/v1/hitos").WithTags("Hitos")
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        hitos.MapPut("/{id:int}", async (int id, HitoRequest request, HitosService servicio, HttpContext http, CancellationToken ct) =>
                Results.Ok(await servicio.ActualizarAsync(id, request, http.User.UsuarioAutenticado(), ct)))
            .Validar<HitoRequest>()
            .WithSummary("Actualiza o reprograma un hito (cambiar la fecha exige justificación)")
            .RequireAuthorization(Politicas.GestionarHitos)
            .Produces<HitoGuardado>()
            .Produces<ErrorApi>(StatusCodes.Status422UnprocessableEntity);

        hitos.MapPost("/{id:int}/iniciar", async (int id, HitosService servicio, HttpContext http, CancellationToken ct) =>
            {
                await servicio.IniciarAsync(id, http.User.UsuarioAutenticado(), ct);
                return Results.NoContent();
            })
            .WithSummary("El responsable marca el hito \"En curso\"")
            .RequireAuthorization(Politicas.VerProyectos)
            .Produces(StatusCodes.Status204NoContent);

        hitos.MapPost("/{id:int}/completar", async (int id, CompletarHitoRequest request, HitosService servicio, HttpContext http, CancellationToken ct) =>
            {
                await servicio.CompletarAsync(id, request, http.User.UsuarioAutenticado(), ct);
                return Results.NoContent();
            })
            .WithSummary("Completa el hito con la fecha del día y, opcionalmente, un avance como evidencia")
            .RequireAuthorization(Politicas.VerProyectos)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorApi>(StatusCodes.Status422UnprocessableEntity);

        // ----- HU-19 y HU-20 -----
        var avances = app.MapGroup("/api/v1/avances").WithTags("Avances")
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        avances.MapPut("/{id:int}", async (int id, AvanceRequest request, AvancesService servicio, HttpContext http, CancellationToken ct) =>
            {
                await servicio.ActualizarAsync(id, request, http.User.UsuarioAutenticado(), ct);
                return Results.NoContent();
            })
            .Validar<AvanceRequest>()
            .WithSummary("El autor corrige el avance (vuelve a \"Pendiente\")")
            .RequireAuthorization(Politicas.RegistrarAvances)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorApi>(StatusCodes.Status422UnprocessableEntity);

        avances.MapPost("/{id:int}/adjuntos", async (int id, [FromForm] IFormFile? archivo, AvancesService servicio, HttpContext http, CancellationToken ct) =>
            {
                if (archivo is null)
                    return Results.BadRequest(new ErrorApi("Debe adjuntar el archivo."));
                await using var contenido = archivo.OpenReadStream();
                var adjuntoId = await servicio.AdjuntarAsync(id, archivo.FileName, archivo.Length, contenido, http.User.UsuarioAutenticado(), ct);
                return Results.Created($"/api/v1/avances/adjuntos/{adjuntoId}", new { id = adjuntoId });
            })
            .DisableAntiforgery() // API con token Bearer: no usa cookies de sesión para autenticar.
            .WithSummary("Adjunta una evidencia al avance (hasta 5, máx. 20 MB: PDF, DOCX, XLSX, CSV, JPG o PNG)")
            .RequireAuthorization(Politicas.RegistrarAvances)
            .Produces(StatusCodes.Status201Created)
            .Produces<ErrorApi>(StatusCodes.Status422UnprocessableEntity);

        avances.MapGet("/adjuntos/{id:int}/descarga", async (int id, AvancesService servicio, HttpContext http, CancellationToken ct) =>
                Results.Ok(await servicio.GenerarDescargaAdjuntoAsync(id, http.User.UsuarioAutenticado(), ct)))
            .WithSummary("URL firmada (5 minutos) para descargar un adjunto")
            .RequireAuthorization(Politicas.VerProyectos)
            .Produces<EnlaceDescargaAdjunto>();

        avances.MapGet("/adjuntos/archivo", async (string token, AvancesService servicio, CancellationToken ct) =>
            {
                var archivo = await servicio.AbrirAdjuntoAsync(token, ct);
                return Results.File(archivo.Contenido, archivo.TipoContenido, archivo.Nombre);
            })
            .AllowAnonymous()
            .WithSummary("Descarga el adjunto con el token de la URL firmada")
            .Produces(StatusCodes.Status200OK);

        avances.MapGet("/pendientes", async (int? proyectoId, string? periodo, AvancesService servicio, HttpContext http, CancellationToken ct) =>
                Results.Ok(await servicio.PendientesAsync(http.User.UsuarioAutenticado(), proyectoId, periodo, ct)))
            .WithSummary("Bandeja \"Avances por validar\" con contador, filtrable por proyecto y periodo (AAAA-MM)")
            .RequireAuthorization(Politicas.ValidarAvances)
            .Produces<BandejaAvances>();

        avances.MapPost("/{id:int}/validaciones", async (int id, ValidacionRequest request, AvancesService servicio, HttpContext http, CancellationToken ct) =>
            {
                await servicio.ValidarAsync(id, request, http.User.UsuarioAutenticado(), ct);
                return Results.NoContent();
            })
            .Validar<ValidacionRequest>()
            .WithSummary("Valida o rechaza (con observaciones) un avance")
            .RequireAuthorization(Politicas.ValidarAvances)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorApi>(StatusCodes.Status422UnprocessableEntity);
    }
}
