using Ci2dt2.Api.Infraestructura;
using Ci2dt2.Application.Avances;
using Ci2dt2.Application.Common;
using Ci2dt2.Application.Convocatorias;
using Ci2dt2.Application.Cronogramas;
using Ci2dt2.Application.Proyectos;
using Ci2dt2.Application.Seguridad;
using Ci2dt2.Domain;

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

public sealed record FichaProyecto(ProyectoDetalle Proyecto, PermisosProyecto Permisos, FlujoProyecto Flujo);

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

        grupo.MapGet("/{id:int}", async (int id, ProyectosQueries consultas, ProyectosService servicio, FlujoProyectoService flujo,
                AccesoRecursos acceso, HttpContext http, CancellationToken ct) =>
            {
                var usuario = http.User.UsuarioAutenticado();
                // HU-05 CA-2: si no puede verlo responde 404 para no revelar su existencia.
                if (!await acceso.PuedeVerProyectoAsync(usuario, id, ct) || await consultas.ObtenerAsync(id, ct) is not { } detalle)
                    return Results.NotFound(new ErrorApi("El proyecto no existe."));
                return Results.Ok(new FichaProyecto(detalle, await servicio.PermisosAsync(id, usuario, ct), await flujo.FlujoAsync(id, usuario, ct)));
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

        // ----- HU-11: envío a revisión y aval -----
        grupo.MapPost("/{id:int}/enviar-revision", async (int id, FlujoProyectoService flujo, HttpContext http, CancellationToken ct) =>
            {
                await flujo.EnviarRevisionAsync(id, http.User.UsuarioAutenticado(), ct);
                return Results.NoContent();
            })
            .WithSummary("El IP envía la propuesta a revisión (etapa Coordinador) si cumple los requisitos")
            .RequireAuthorization(Politicas.EditarPropuestas)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorApi>(StatusCodes.Status422UnprocessableEntity);

        grupo.MapPost("/{id:int}/revisiones", async (int id, RevisionRequest request, FlujoProyectoService flujo, HttpContext http, CancellationToken ct) =>
            {
                await flujo.RevisarAsync(id, request, http.User.UsuarioAutenticado(), ct);
                return Results.NoContent();
            })
            .Validar<RevisionRequest>()
            .WithSummary("Decisión de revisión: Aprueba o Devuelve (Coordinador), Avala o Devuelve (Director)")
            .RequireAuthorization(p => p.RequireRole(Roles.Administrador, Roles.Director, Roles.Coordinador))
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorApi>(StatusCodes.Status422UnprocessableEntity);

        // ----- HU-12: aprobación e inicio de ejecución -----
        grupo.MapPost("/{id:int}/aprobacion", async (int id, AprobacionRequest request, FlujoProyectoService flujo, HttpContext http, CancellationToken ct) =>
            {
                await flujo.RegistrarAprobacionAsync(id, request, http.User.UsuarioAutenticado(), ct);
                return Results.NoContent();
            })
            .Validar<AprobacionRequest>()
            .WithSummary("Registra el resultado de la postulación: Aprobado (con acto administrativo) o No aprobado")
            .RequireAuthorization(Politicas.AvalarAprobarProyectos)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorApi>(StatusCodes.Status422UnprocessableEntity);

        grupo.MapPost("/{id:int}/iniciar-ejecucion", async (int id, IniciarEjecucionRequest request, FlujoProyectoService flujo, HttpContext http, CancellationToken ct) =>
            {
                await flujo.IniciarEjecucionAsync(id, request, http.User.UsuarioAutenticado(), ct);
                return Results.NoContent();
            })
            .Validar<IniciarEjecucionRequest>()
            .WithSummary("Pasa el proyecto aprobado a En ejecución con la fecha de inicio y el plan de trabajo")
            .RequireAuthorization(Politicas.AvalarAprobarProyectos)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorApi>(StatusCodes.Status422UnprocessableEntity);

        // ----- HU-16: postulaciones -----
        grupo.MapGet("/{id:int}/postulaciones", async (int id, PostulacionesService servicio, AccesoRecursos acceso, HttpContext http, CancellationToken ct) =>
                await acceso.PuedeVerProyectoAsync(http.User.UsuarioAutenticado(), id, ct)
                    ? Results.Ok(await servicio.ListarAsync(id, ct))
                    : Results.NotFound(new ErrorApi("El proyecto no existe.")))
            .WithSummary("Postulaciones del proyecto a convocatorias")
            .RequireAuthorization(Politicas.VerProyectos)
            .Produces<IReadOnlyList<PostulacionDetalle>>();

        grupo.MapPost("/{id:int}/postulaciones", async (int id, PostulacionRequest request, PostulacionesService servicio, HttpContext http, CancellationToken ct) =>
            {
                var postulacionId = await servicio.CrearAsync(id, request, http.User.UsuarioAutenticado(), ct);
                return Results.Created($"/api/v1/proyectos/{id}/postulaciones/{postulacionId}", new { id = postulacionId });
            })
            .Validar<PostulacionRequest>()
            .WithSummary("Vincula la propuesta a una convocatoria")
            .RequireAuthorization(Politicas.EditarPropuestas)
            .Produces(StatusCodes.Status201Created)
            .Produces<ErrorApi>(StatusCodes.Status422UnprocessableEntity);

        grupo.MapPut("/{id:int}/postulaciones/{postulacionId:int}", async (int id, int postulacionId, PostulacionRequest request,
                PostulacionesService servicio, HttpContext http, CancellationToken ct) =>
            {
                await servicio.ActualizarAsync(id, postulacionId, request, http.User.UsuarioAutenticado(), ct);
                return Results.NoContent();
            })
            .Validar<PostulacionRequest>()
            .WithSummary("Actualiza la radicación externa, la fase y el resultado de la postulación")
            .RequireAuthorization(Politicas.EditarPropuestas)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorApi>(StatusCodes.Status422UnprocessableEntity);

        grupo.MapDelete("/{id:int}/postulaciones/{postulacionId:int}", async (int id, int postulacionId,
                PostulacionesService servicio, HttpContext http, CancellationToken ct) =>
            {
                await servicio.EliminarAsync(id, postulacionId, http.User.UsuarioAutenticado(), ct);
                return Results.NoContent();
            })
            .WithSummary("Elimina una postulación (borrado lógico)")
            .RequireAuthorization(Politicas.EditarPropuestas)
            .Produces(StatusCodes.Status204NoContent);

        // ----- HU-17: hitos -----
        grupo.MapGet("/{id:int}/hitos", async (int id, HitosService servicio, AccesoRecursos acceso, HttpContext http, CancellationToken ct) =>
                await acceso.PuedeVerProyectoAsync(http.User.UsuarioAutenticado(), id, ct)
                    ? Results.Ok(await servicio.ListarAsync(id, ct))
                    : Results.NotFound(new ErrorApi("El proyecto no existe.")))
            .WithSummary("Hitos del cronograma ordenados por fecha límite")
            .RequireAuthorization(Politicas.VerProyectos)
            .Produces<IReadOnlyList<HitoDetalle>>();

        grupo.MapPost("/{id:int}/hitos", async (int id, HitoRequest request, HitosService servicio, HttpContext http, CancellationToken ct) =>
            {
                var guardado = await servicio.CrearAsync(id, request, http.User.UsuarioAutenticado(), ct);
                return Results.Created($"/api/v1/hitos/{guardado.Id}", guardado);
            })
            .Validar<HitoRequest>()
            .WithSummary("Agrega un hito con tipo, fecha límite, responsable y días de alerta")
            .RequireAuthorization(Politicas.GestionarHitos)
            .Produces<HitoGuardado>(StatusCodes.Status201Created)
            .Produces<ErrorApi>(StatusCodes.Status422UnprocessableEntity);

        // ----- HU-19: avances -----
        grupo.MapGet("/{id:int}/avances", async (int id, AvancesService servicio, AccesoRecursos acceso, HttpContext http, CancellationToken ct) =>
                await acceso.PuedeVerProyectoAsync(http.User.UsuarioAutenticado(), id, ct)
                    ? Results.Ok(await servicio.ListarAsync(id, ct))
                    : Results.NotFound(new ErrorApi("El proyecto no existe.")))
            .WithSummary("Avances del proyecto con adjuntos e historial de validaciones")
            .RequireAuthorization(Politicas.VerProyectos)
            .Produces<IReadOnlyList<AvanceDetalle>>();

        grupo.MapPost("/{id:int}/avances", async (int id, AvanceRequest request, AvancesService servicio, HttpContext http, CancellationToken ct) =>
            {
                var guardado = await servicio.CrearAsync(id, request, http.User.UsuarioAutenticado(), ct);
                return guardado.Existente
                    ? Results.Ok(guardado)
                    : Results.Created($"/api/v1/avances/{guardado.Id}", guardado);
            })
            .Validar<AvanceRequest>()
            .WithSummary("Registra un avance (queda Pendiente); el IdCliente evita duplicados")
            .RequireAuthorization(Politicas.RegistrarAvances)
            .Produces<AvanceGuardado>(StatusCodes.Status201Created)
            .Produces<ErrorApi>(StatusCodes.Status422UnprocessableEntity);
    }
}
