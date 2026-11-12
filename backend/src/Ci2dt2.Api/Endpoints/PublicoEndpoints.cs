using Ci2dt2.Api.Infraestructura;
using Ci2dt2.Application.Common;
using Ci2dt2.Application.Publico;

namespace Ci2dt2.Api.Endpoints;

/// <summary>HU-25, HU-26 y HU-27: API pública (sin autenticación) del portal institucional.</summary>
public sealed class PublicoEndpoints : IEndpointModule
{
    public static void Map(IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/v1/publico").WithTags("Portal público").AllowAnonymous();

        grupo.MapGet("/resumen", async (PortalQueries portal, CancellationToken ct) => Results.Ok(await portal.ResumenAsync(ct)))
            .WithSummary("Cifras del centro, programas y convocatorias abiertas")
            .Produces<ResumenPortal>();

        grupo.MapGet("/programas", async (PortalQueries portal, CancellationToken ct) => Results.Ok(await portal.ProgramasAsync(ct)))
            .WithSummary("Programas de investigación con su URL amigable")
            .Produces<IReadOnlyList<ProgramaPublico>>();

        grupo.MapGet("/programas/{slug}", async (string slug, PortalQueries portal, CancellationToken ct) =>
                await portal.ProgramaAsync(slug, ct) is { } programa
                    ? Results.Ok(programa)
                    : Results.NotFound(new ErrorApi("El programa no existe.")))
            .WithSummary("Detalle público del programa: objetivos, equipo, aliados y proyectos públicos")
            .Produces<ProgramaPublicoDetalle>()
            .Produces<ErrorApi>(StatusCodes.Status404NotFound);

        grupo.MapGet("/proyectos", async (int? programaId, int? estadoId, int? anio, string? busqueda, int? pagina,
                PortalQueries portal, CancellationToken ct) =>
                Results.Ok(await portal.ProyectosAsync(new FiltroPortal(programaId, estadoId, anio, busqueda, pagina ?? 1), ct)))
            .WithSummary("Portafolio público (solo Aprobado, En ejecución y Finalizado), paginado")
            .Produces<Pagina<ProyectoPublicoItem>>();

        grupo.MapGet("/proyectos/{codigo}", async (string codigo, PortalQueries portal, CancellationToken ct) =>
                await portal.ProyectoAsync(codigo, ct) is { } proyecto
                    ? Results.Ok(proyecto)
                    : Results.NotFound(new ErrorApi("El proyecto no existe.")))
            .WithSummary("Detalle público de un proyecto por su código")
            .Produces<ProyectoPublicoDetalle>()
            .Produces<ErrorApi>(StatusCodes.Status404NotFound);
    }
}
