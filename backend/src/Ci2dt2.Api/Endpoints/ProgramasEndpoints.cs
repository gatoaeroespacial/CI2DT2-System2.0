using Ci2dt2.Api.Infraestructura;
using Ci2dt2.Application.Programas;
using Ci2dt2.Application.Seguridad;
using Ci2dt2.Domain;

namespace Ci2dt2.Api.Endpoints;

/// <summary>HU-06: programas, objetivos específicos e integrantes; HU-07: aliados del programa.</summary>
public sealed class ProgramasEndpoints : IEndpointModule
{
    public static void Map(IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/v1/programas")
            .WithTags("Programas")
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        var lectura = new[] { Roles.Administrador, Roles.Director, Roles.Coordinador, Roles.Investigador };

        grupo.MapGet("/", async (ProgramasQueries consultas, CancellationToken ct) => Results.Ok(await consultas.ListarAsync(ct)))
            .WithSummary("Programas del centro con su Coordinador vigente")
            .RequireAuthorization(p => p.RequireRole(lectura))
            .Produces<IReadOnlyList<ProgramaResumen>>();

        grupo.MapGet("/{id:int}", async (int id, ProgramasQueries consultas, CancellationToken ct) =>
                await consultas.ObtenerAsync(id, ct) is { } programa
                    ? Results.Ok(programa)
                    : Results.NotFound(new ErrorApi("El programa no existe.")))
            .WithSummary("Detalle del programa con objetivos (en orden) y aliados")
            .RequireAuthorization(p => p.RequireRole(lectura))
            .Produces<ProgramaDetalle>()
            .Produces<ErrorApi>(StatusCodes.Status404NotFound);

        grupo.MapPut("/{id:int}", async (int id, ActualizarProgramaRequest request, ProgramasService servicio, CancellationToken ct) =>
            {
                await servicio.ActualizarAsync(id, request, ct);
                return Results.NoContent();
            })
            .Validar<ActualizarProgramaRequest>()
            .WithSummary("Edita el nombre y la descripción del programa")
            .RequireAuthorization(Politicas.EditarProgramas)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorApi>(StatusCodes.Status404NotFound);

        // ----- Objetivos específicos (CRUD + orden) -----
        var objetivos = grupo.MapGroup("/{id:int}/objetivos");

        objetivos.MapGet("/", async (int id, ProgramasQueries consultas, CancellationToken ct) =>
                await consultas.ObtenerAsync(id, ct) is { } programa
                    ? Results.Ok(programa.Objetivos)
                    : Results.NotFound(new ErrorApi("El programa no existe.")))
            .WithSummary("Objetivos específicos en su orden")
            .RequireAuthorization(p => p.RequireRole(lectura))
            .Produces<IReadOnlyList<ObjetivoItem>>();

        objetivos.MapPost("/", async (int id, ObjetivoRequest request, ProgramasService servicio, CancellationToken ct) =>
            {
                var objetivoId = await servicio.CrearObjetivoAsync(id, request, ct);
                return Results.Created($"/api/v1/programas/{id}/objetivos/{objetivoId}", new { id = objetivoId });
            })
            .Validar<ObjetivoRequest>()
            .WithSummary("Agrega un objetivo al final de la lista")
            .RequireAuthorization(Politicas.EditarProgramas)
            .Produces(StatusCodes.Status201Created);

        objetivos.MapPut("/{objetivoId:int}", async (int id, int objetivoId, ObjetivoRequest request, ProgramasService servicio, CancellationToken ct) =>
            {
                await servicio.ActualizarObjetivoAsync(id, objetivoId, request, ct);
                return Results.NoContent();
            })
            .Validar<ObjetivoRequest>()
            .WithSummary("Edita un objetivo")
            .RequireAuthorization(Politicas.EditarProgramas)
            .Produces(StatusCodes.Status204NoContent);

        objetivos.MapDelete("/{objetivoId:int}", async (int id, int objetivoId, ProgramasService servicio, CancellationToken ct) =>
            {
                await servicio.EliminarObjetivoAsync(id, objetivoId, ct);
                return Results.NoContent();
            })
            .WithSummary("Elimina un objetivo (borrado lógico)")
            .RequireAuthorization(Politicas.EditarProgramas)
            .Produces(StatusCodes.Status204NoContent);

        objetivos.MapPut("/orden", async (int id, ReordenarObjetivosRequest request, ProgramasService servicio, CancellationToken ct) =>
            {
                await servicio.ReordenarObjetivosAsync(id, request.Ids, ct);
                return Results.NoContent();
            })
            .Validar<ReordenarObjetivosRequest>()
            .WithSummary("Guarda el nuevo orden de los objetivos (arrastrar y soltar)")
            .RequireAuthorization(Politicas.EditarProgramas)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorApi>(StatusCodes.Status422UnprocessableEntity);

        // ----- Integrantes -----
        var integrantes = grupo.MapGroup("/{id:int}/integrantes");

        integrantes.MapGet("/", async (int id, ProgramasQueries consultas, AccesoRecursos acceso, HttpContext http, CancellationToken ct) =>
            {
                if (!await acceso.PuedeGestionarProgramaAsync(http.User.UsuarioAutenticado(), id, ct))
                    return Results.Forbid();
                return Results.Ok(await consultas.IntegrantesAsync(id, ct));
            })
            .WithSummary("Integrantes vigentes e históricos del programa")
            .RequireAuthorization(Politicas.VincularInvestigadores)
            .Produces<IReadOnlyList<IntegranteProgramaItem>>();

        integrantes.MapPost("/", async (int id, VincularIntegranteRequest request, ProgramasService servicio, HttpContext http, CancellationToken ct) =>
            {
                var integranteId = await servicio.VincularIntegranteAsync(id, request, http.User.UsuarioAutenticado(), ct);
                return Results.Created($"/api/v1/programas/{id}/integrantes/{integranteId}", new { id = integranteId });
            })
            .Validar<VincularIntegranteRequest>()
            .WithSummary("Vincula un Coordinador (Admin/Director) o un Investigador (también el Coordinador del programa)")
            .RequireAuthorization(Politicas.VincularInvestigadores)
            .Produces(StatusCodes.Status201Created)
            .Produces<ErrorApi>(StatusCodes.Status422UnprocessableEntity);

        integrantes.MapPut("/{integranteId:int}", async (int id, int integranteId, ActualizarVinculacionRequest request, ProgramasService servicio, HttpContext http, CancellationToken ct) =>
            {
                await servicio.ActualizarVinculacionAsync(id, integranteId, request, http.User.UsuarioAutenticado(), ct);
                return Results.NoContent();
            })
            .WithSummary("Actualiza las fechas de la vinculación; el retiro se registra con la fecha de fin")
            .RequireAuthorization(Politicas.VincularInvestigadores)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorApi>(StatusCodes.Status422UnprocessableEntity);

        // ----- Aliados del programa (HU-07) -----
        grupo.MapPut("/{id:int}/aliados", async (int id, AsignarAliadosProgramaRequest request, ProgramasService servicio, HttpContext http, CancellationToken ct) =>
            {
                await servicio.AsignarAliadosAsync(id, request.AliadoIds, http.User.UsuarioAutenticado(), ct);
                return Results.NoContent();
            })
            .Validar<AsignarAliadosProgramaRequest>()
            .WithTags("Aliados")
            .WithSummary("Reemplaza los aliados asociados al programa")
            .RequireAuthorization(Politicas.GestionarAliadosConvocatorias)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorApi>(StatusCodes.Status422UnprocessableEntity);
    }
}
