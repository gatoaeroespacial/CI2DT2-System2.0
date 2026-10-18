using Ci2dt2.Api.Infraestructura;
using Ci2dt2.Application.Common;
using Ci2dt2.Application.Proyectos;
using Ci2dt2.Application.Seguridad;

namespace Ci2dt2.Api.Endpoints;

/// <summary>Parámetros de consulta del listado de proyectos (HU-13 CA-2).</summary>
public sealed class ConsultaProyectos
{
    public int? ProgramaId { get; init; }
    public int? EstadoId { get; init; }
    public int? TipoProyectoId { get; init; }
    public int? Anio { get; init; }
    public int? InvestigadorPrincipalId { get; init; }
    public string? Busqueda { get; init; }
    public string? Orden { get; init; }
    public bool? Desc { get; init; }
    public int? Pagina { get; init; }

    public FiltroProyectos AFiltro(int? programaId = null) => new(
        programaId ?? ProgramaId, EstadoId, TipoProyectoId, Anio, InvestigadorPrincipalId, Busqueda, Orden, Desc ?? false, Pagina ?? 1);
}

public sealed record FichaProyecto(ProyectoDetalle Proyecto, PermisosProyecto Permisos);

/// <summary>HU-08 (registro), HU-09 (equipo), HU-10 (financiación) y HU-13 (consulta).</summary>
public sealed class ProyectosEndpoints : IEndpointModule
{
    public static void Map(IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/v1/proyectos")
            .WithTags("Proyectos")
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        // ----- HU-13: consulta -----
        grupo.MapGet("/", async ([AsParameters] ConsultaProyectos q, ProyectosQueries consultas, HttpContext http, CancellationToken ct) =>
                Results.Ok(await consultas.ListarAsync(q.AFiltro(), http.User.UsuarioAutenticado(), ct)))
            .WithSummary("Listado con filtros, búsqueda, orden y paginación según el alcance del rol")
            .RequireAuthorization(Politicas.VerProyectos)
            .Produces<Pagina<ProyectoListadoItem>>();

        grupo.MapGet("/mios", async ([AsParameters] ConsultaProyectos q, ProyectosQueries consultas, HttpContext http, CancellationToken ct) =>
                Results.Ok(await consultas.MiosAsync(q.AFiltro(), http.User.UsuarioAutenticado(), ct)))
            .WithSummary("Mis proyectos: vinculaciones vigentes e históricas")
            .RequireAuthorization(Politicas.VerProyectos)
            .Produces<Pagina<ProyectoListadoItem>>();

        grupo.MapGet("/programa/{programaId:int}", async (int programaId, [AsParameters] ConsultaProyectos q,
                ProyectosQueries consultas, AccesoRecursos acceso, HttpContext http, CancellationToken ct) =>
            {
                var usuario = http.User.UsuarioAutenticado();
                if (!await acceso.PuedeGestionarProgramaAsync(usuario, programaId, ct))
                    return Results.Forbid();
                return Results.Ok(await consultas.ListarAsync(q.AFiltro(programaId), usuario, ct));
            })
            .WithSummary("Proyectos del programa (Coordinador de ese programa, Director o Administrador)")
            .RequireAuthorization(Politicas.VerProyectos)
            .Produces<Pagina<ProyectoListadoItem>>();

        grupo.MapGet("/{id:int}", async (int id, ProyectosQueries consultas, ProyectosService servicio,
                AccesoRecursos acceso, HttpContext http, CancellationToken ct) =>
            {
                var usuario = http.User.UsuarioAutenticado();
                // HU-05 CA-2: si no puede verlo responde 404 para no revelar su existencia.
                if (!await acceso.PuedeVerProyectoAsync(usuario, id, ct) || await consultas.ObtenerAsync(id, ct) is not { } detalle)
                    return Results.NotFound(new ErrorApi("El proyecto no existe."));
                return Results.Ok(new FichaProyecto(detalle, await servicio.PermisosAsync(id, usuario, ct)));
            })
            .WithSummary("Ficha del proyecto: general, convocatorias, cronograma, avances y revisiones")
            .RequireAuthorization(Politicas.VerProyectos)
            .Produces<FichaProyecto>()
            .Produces<ErrorApi>(StatusCodes.Status404NotFound);

        // ----- HU-08: registro y edición -----
        grupo.MapPost("/", async (CrearProyectoRequest request, ProyectosService servicio, HttpContext http, CancellationToken ct) =>
            {
                var id = await servicio.CrearAsync(request, http.User.UsuarioAutenticado(), ct);
                return Results.Created($"/api/v1/proyectos/{id}", new { id });
            })
            .Validar<CrearProyectoRequest>()
            .WithSummary("Registra una propuesta en estado \"En elaboración\" con código automático")
            .RequireAuthorization(Politicas.CrearPropuestas)
            .Produces(StatusCodes.Status201Created)
            .Produces<ErrorApi>(StatusCodes.Status422UnprocessableEntity);

        grupo.MapPut("/{id:int}", async (int id, ActualizarProyectoRequest request, ProyectosService servicio, HttpContext http, CancellationToken ct) =>
            {
                await servicio.ActualizarAsync(id, request, http.User.UsuarioAutenticado(), ct);
                return Results.NoContent();
            })
            .Validar<ActualizarProyectoRequest>()
            .WithSummary("Edita la propuesta (IP solo en \"En elaboración\" o \"Devuelto\")")
            .RequireAuthorization(Politicas.EditarPropuestas)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorApi>(StatusCodes.Status404NotFound)
            .Produces<ErrorApi>(StatusCodes.Status422UnprocessableEntity);

        // ----- HU-09: equipo -----
        grupo.MapGet("/{id:int}/integrantes", async (int id, ProyectosQueries consultas, AccesoRecursos acceso, HttpContext http, CancellationToken ct) =>
                await acceso.PuedeVerProyectoAsync(http.User.UsuarioAutenticado(), id, ct)
                    ? Results.Ok(await consultas.IntegrantesAsync(id, ct))
                    : Results.NotFound(new ErrorApi("El proyecto no existe.")))
            .WithSummary("Equipo del proyecto con enlaces a CvLAC y ORCID")
            .RequireAuthorization(Politicas.VerProyectos)
            .Produces<IReadOnlyList<IntegranteProyectoItem>>();

        grupo.MapPost("/{id:int}/integrantes", async (int id, AgregarIntegranteRequest request, ProyectosService servicio, HttpContext http, CancellationToken ct) =>
            {
                var integranteId = await servicio.AgregarIntegranteAsync(id, request, http.User.UsuarioAutenticado(), ct);
                return Results.Created($"/api/v1/proyectos/{id}/integrantes/{integranteId}", new { id = integranteId });
            })
            .Validar<AgregarIntegranteRequest>()
            .WithSummary("Agrega un integrante; con rol IP cambia el IP (solo Coordinador o Director)")
            .RequireAuthorization(Politicas.EditarPropuestas)
            .Produces(StatusCodes.Status201Created)
            .Produces<ErrorApi>(StatusCodes.Status422UnprocessableEntity);

        grupo.MapPut("/{id:int}/integrantes/{integranteId:int}", async (int id, int integranteId, ActualizarIntegranteRequest request,
                ProyectosService servicio, HttpContext http, CancellationToken ct) =>
            {
                await servicio.ActualizarIntegranteAsync(id, integranteId, request, http.User.UsuarioAutenticado(), ct);
                return Results.NoContent();
            })
            .WithSummary("Actualiza fechas del integrante; el retiro se registra con la fecha de fin")
            .RequireAuthorization(Politicas.EditarPropuestas)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorApi>(StatusCodes.Status422UnprocessableEntity);

        // ----- HU-10: financiación -----
        grupo.MapGet("/{id:int}/fuentes", async (int id, ProyectosQueries consultas, AccesoRecursos acceso, HttpContext http, CancellationToken ct) =>
                await acceso.PuedeVerProyectoAsync(http.User.UsuarioAutenticado(), id, ct)
                    ? Results.Ok(await consultas.FinanciacionAsync(id, ct))
                    : Results.NotFound(new ErrorApi("El proyecto no existe.")))
            .WithSummary("Fuentes de financiamiento con totales en COP y diferencia frente al presupuesto")
            .RequireAuthorization(Politicas.VerProyectos)
            .Produces<FinanciacionProyecto>();

        grupo.MapPost("/{id:int}/fuentes", async (int id, FuenteRequest request, ProyectosService servicio, HttpContext http, CancellationToken ct) =>
            {
                var fuenteId = await servicio.CrearFuenteAsync(id, request, http.User.UsuarioAutenticado(), ct);
                return Results.Created($"/api/v1/proyectos/{id}/fuentes/{fuenteId}", new { id = fuenteId });
            })
            .Validar<FuenteRequest>()
            .WithSummary("Registra una fuente de financiamiento o contrapartida")
            .RequireAuthorization(Politicas.EditarPropuestas)
            .Produces(StatusCodes.Status201Created)
            .Produces<ErrorApi>(StatusCodes.Status422UnprocessableEntity);

        grupo.MapPut("/{id:int}/fuentes/{fuenteId:int}", async (int id, int fuenteId, FuenteRequest request, ProyectosService servicio, HttpContext http, CancellationToken ct) =>
            {
                await servicio.ActualizarFuenteAsync(id, fuenteId, request, http.User.UsuarioAutenticado(), ct);
                return Results.NoContent();
            })
            .Validar<FuenteRequest>()
            .WithSummary("Actualiza una fuente de financiamiento")
            .RequireAuthorization(Politicas.EditarPropuestas)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorApi>(StatusCodes.Status422UnprocessableEntity);

        grupo.MapDelete("/{id:int}/fuentes/{fuenteId:int}", async (int id, int fuenteId, ProyectosService servicio, HttpContext http, CancellationToken ct) =>
            {
                await servicio.EliminarFuenteAsync(id, fuenteId, http.User.UsuarioAutenticado(), ct);
                return Results.NoContent();
            })
            .WithSummary("Elimina una fuente de financiamiento (borrado lógico)")
            .RequireAuthorization(Politicas.EditarPropuestas)
            .Produces(StatusCodes.Status204NoContent);
    }
}
