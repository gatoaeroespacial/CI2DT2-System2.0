using Ci2dt2.Api.Infraestructura;
using Ci2dt2.Application.Common;
using Ci2dt2.Application.Seguridad;
using Ci2dt2.Application.Usuarios;
using Ci2dt2.Domain;

namespace Ci2dt2.Api.Endpoints;

/// <summary>HU-04: administración de usuarios y roles (solo Administrador).</summary>
public sealed class UsuariosEndpoints : IEndpointModule
{
    public static void Map(IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/v1/usuarios")
            .WithTags("Usuarios")
            .RequireAuthorization(Politicas.GestionarUsuarios)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        grupo.MapGet("/", Listar)
            .WithSummary("Lista paginada (20 por página) con búsqueda y filtros por rol y estado")
            .Produces<Pagina<UsuarioListadoItem>>();

        grupo.MapGet("/{id:int}", Obtener)
            .WithSummary("Detalle de un usuario")
            .Produces<UsuarioDetalle>()
            .Produces<ErrorApi>(StatusCodes.Status404NotFound);

        grupo.MapPost("/", Crear)
            .Validar<CrearUsuarioRequest>()
            .WithSummary("Crea un usuario con su contraseña inicial")
            .Produces<UsuarioDetalle>(StatusCodes.Status201Created)
            .Produces<ErrorApi>(StatusCodes.Status422UnprocessableEntity);

        grupo.MapPut("/{id:int}", Actualizar)
            .Validar<ActualizarUsuarioRequest>()
            .WithSummary("Actualiza los datos de un usuario")
            .Produces<UsuarioDetalle>()
            .Produces<ErrorApi>(StatusCodes.Status404NotFound)
            .Produces<ErrorApi>(StatusCodes.Status422UnprocessableEntity);

        grupo.MapPatch("/{id:int}/estado", CambiarEstado)
            .Validar<CambiarEstadoUsuarioRequest>()
            .WithSummary("Activa o desactiva un usuario (desactivar revoca sus sesiones)")
            .Produces<UsuarioDetalle>()
            .Produces<ErrorApi>(StatusCodes.Status404NotFound)
            .Produces<ErrorApi>(StatusCodes.Status422UnprocessableEntity);

        grupo.MapPut("/{id:int}/roles", AsignarRoles)
            .Validar<AsignarRolesRequest>()
            .WithSummary("Reemplaza los roles de un usuario")
            .Produces<UsuarioDetalle>()
            .Produces<ErrorApi>(StatusCodes.Status404NotFound)
            .Produces<ErrorApi>(StatusCodes.Status422UnprocessableEntity);

        grupo.MapPut("/{id:int}/contrasena", AsignarContrasena)
            .Validar<AsignarContrasenaRequest>()
            .WithSummary("Asigna una nueva contraseña y revoca todas las sesiones del usuario")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorApi>(StatusCodes.Status404NotFound);

        // Búsqueda de usuarios para vincularlos a programas (HU-06) o proyectos (HU-09).
        app.MapGet("/api/v1/usuarios/buscar", async (UsuariosQueries consultas, string? texto, CancellationToken ct) =>
                Results.Ok(await consultas.BuscarAsync(texto, ct)))
            .WithTags("Usuarios")
            .WithSummary("Busca usuarios activos por nombre, correo o documento (máximo 10)")
            .RequireAuthorization(p => p.RequireRole(Roles.Administrador, Roles.Director, Roles.Coordinador, Roles.Investigador))
            .Produces<IReadOnlyList<UsuarioOpcion>>();
    }

    private static async Task<IResult> Listar(
        UsuariosQueries consultas, string? busqueda, string? rol, string? estado, int? pagina, CancellationToken ct)
    {
        bool? tieneAcceso = estado?.ToLowerInvariant() switch
        {
            "activo" => true,
            "inactivo" => false,
            _ => null,
        };
        return Results.Ok(await consultas.ListarAsync(new FiltroUsuarios(busqueda, rol, tieneAcceso, pagina ?? 1), ct));
    }

    private static async Task<IResult> Obtener(int id, UsuariosService servicio, CancellationToken ct) =>
        Results.Ok(await servicio.ObtenerAsync(id, ct));

    private static async Task<IResult> Crear(CrearUsuarioRequest request, UsuariosService servicio, CancellationToken ct)
    {
        var usuario = await servicio.CrearAsync(request, ct);
        return Results.Created($"/api/v1/usuarios/{usuario.Id}", usuario);
    }

    private static async Task<IResult> Actualizar(int id, ActualizarUsuarioRequest request, UsuariosService servicio, CancellationToken ct) =>
        Results.Ok(await servicio.ActualizarAsync(id, request, ct));

    private static async Task<IResult> CambiarEstado(
        int id, CambiarEstadoUsuarioRequest request, UsuariosService servicio, HttpContext http, CancellationToken ct) =>
        Results.Ok(await servicio.CambiarEstadoAsync(id, request.TieneAcceso!.Value, http.User.UsuarioAutenticado().Id, ct));

    private static async Task<IResult> AsignarRoles(int id, AsignarRolesRequest request, UsuariosService servicio, CancellationToken ct) =>
        Results.Ok(await servicio.AsignarRolesAsync(id, request.Roles, ct));

    private static async Task<IResult> AsignarContrasena(int id, AsignarContrasenaRequest request, UsuariosService servicio, CancellationToken ct)
    {
        await servicio.AsignarContrasenaAsync(id, request.Contrasena, ct);
        return Results.NoContent();
    }
}
