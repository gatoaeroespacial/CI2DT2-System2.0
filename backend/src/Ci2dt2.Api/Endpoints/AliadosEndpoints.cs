using Ci2dt2.Api.Infraestructura;
using Ci2dt2.Application.Aliados;
using Ci2dt2.Application.Common;
using Ci2dt2.Application.Seguridad;
using Ci2dt2.Domain;

namespace Ci2dt2.Api.Endpoints;

/// <summary>HU-07: catálogo de aliados (CRUD). La asociación a programas está en ProgramasEndpoints.</summary>
public sealed class AliadosEndpoints : IEndpointModule
{
    public static void Map(IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/v1/aliados")
            .WithTags("Aliados")
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        // Lectura y autocompletado (CA-2): también la usan los formularios de usuarios y de financiación.
        grupo.MapGet("/", async (AliadosQueries consultas, string? busqueda, int? pagina, CancellationToken ct) =>
                Results.Ok(await consultas.ListarAsync(busqueda, pagina ?? 1, ct)))
            .WithSummary("Lista paginada de aliados con búsqueda por nombre o NIT")
            .RequireAuthorization(p => p.RequireRole(Roles.Administrador, Roles.Director, Roles.Coordinador, Roles.Investigador))
            .Produces<Pagina<AliadoItem>>();

        grupo.MapGet("/{id:int}", async (int id, AliadosQueries consultas, CancellationToken ct) =>
                await consultas.ObtenerAsync(id, ct) is { } aliado
                    ? Results.Ok(aliado)
                    : Results.NotFound(new ErrorApi("El aliado no existe.")))
            .WithSummary("Detalle de un aliado")
            .RequireAuthorization(p => p.RequireRole(Roles.Administrador, Roles.Director, Roles.Coordinador, Roles.Investigador))
            .Produces<AliadoItem>()
            .Produces<ErrorApi>(StatusCodes.Status404NotFound);

        grupo.MapPost("/", async (AliadoRequest request, AliadosService servicio, AliadosQueries consultas, CancellationToken ct) =>
            {
                var id = await servicio.CrearAsync(request, ct);
                return Results.Created($"/api/v1/aliados/{id}", await consultas.ObtenerAsync(id, ct));
            })
            .Validar<AliadoRequest>()
            .WithSummary("Registra un aliado (NIT único; obligatorio con dígito de verificación si es colombiano)")
            .RequireAuthorization(Politicas.GestionarAliadosConvocatorias)
            .Produces<AliadoItem>(StatusCodes.Status201Created)
            .Produces<ErrorApi>(StatusCodes.Status422UnprocessableEntity);

        grupo.MapPut("/{id:int}", async (int id, AliadoRequest request, AliadosService servicio, AliadosQueries consultas, CancellationToken ct) =>
            {
                await servicio.ActualizarAsync(id, request, ct);
                return Results.Ok(await consultas.ObtenerAsync(id, ct));
            })
            .Validar<AliadoRequest>()
            .WithSummary("Actualiza un aliado")
            .RequireAuthorization(Politicas.GestionarAliadosConvocatorias)
            .Produces<AliadoItem>()
            .Produces<ErrorApi>(StatusCodes.Status404NotFound)
            .Produces<ErrorApi>(StatusCodes.Status422UnprocessableEntity);

        grupo.MapDelete("/{id:int}", async (int id, AliadosService servicio, CancellationToken ct) =>
            {
                await servicio.EliminarAsync(id, ct);
                return Results.NoContent();
            })
            .WithSummary("Elimina un aliado (borrado lógico)")
            .RequireAuthorization(Politicas.GestionarAliadosConvocatorias)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorApi>(StatusCodes.Status404NotFound);
    }
}
