using Ci2dt2.Api.Infraestructura;
using Ci2dt2.Application.Catalogos;
using Ci2dt2.Application.Documentos;
using Ci2dt2.Application.Seguridad;
using Microsoft.AspNetCore.Mvc;

namespace Ci2dt2.Api.Endpoints;

/// <summary>HU-21: repositorio documental (carga, listado y descarga con URL firmada temporal).</summary>
public sealed class DocumentosEndpoints : IEndpointModule
{
    public static void Map(IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/v1/documentos")
            .WithTags("Documentos")
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        grupo.MapPost("/", async ([FromForm] IFormFile? archivo, [FromForm] int tipoDocumentoId, [FromForm] string? entidadTipo,
                [FromForm] int entidadId, DocumentosService servicio, HttpContext http, CancellationToken ct) =>
            {
                if (archivo is null || string.IsNullOrWhiteSpace(entidadTipo))
                    return Results.BadRequest(new ErrorApi("Debe adjuntar el archivo e indicar la entidad."));
                await using var contenido = archivo.OpenReadStream();
                var id = await servicio.CargarAsync(entidadTipo, entidadId, tipoDocumentoId, archivo.FileName, archivo.Length,
                    contenido, http.User.UsuarioAutenticado(), ct);
                return Results.Created($"/api/v1/documentos/{id}", new { id });
            })
            .DisableAntiforgery() // API con token Bearer: no usa cookies de sesión para autenticar.
            .WithSummary("Carga un documento (máx. 25 MB; PDF, DOCX, XLSX, PPTX, JPG, PNG o ZIP) y calcula su SHA-256")
            .RequireAuthorization(Politicas.GestionarDocumentos)
            .Produces(StatusCodes.Status201Created)
            .Produces<ErrorApi>(StatusCodes.Status422UnprocessableEntity);

        grupo.MapGet("/", async (string entidadTipo, int entidadId, DocumentosService servicio, HttpContext http, CancellationToken ct) =>
                Results.Ok(await servicio.ListarAsync(entidadTipo, entidadId, http.User.UsuarioAutenticado(), ct)))
            .WithSummary("Documentos de una entidad (Proyecto, Convocatoria, PropuestaConvocatoria o Avance)")
            .RequireAuthorization(Politicas.GestionarDocumentos)
            .Produces<IReadOnlyList<DocumentoItem>>();

        grupo.MapGet("/{id:int}/descarga", async (int id, DocumentosService servicio, HttpContext http, CancellationToken ct) =>
                Results.Ok(await servicio.GenerarDescargaAsync(id, http.User.UsuarioAutenticado(), ct)))
            .WithSummary("Genera una URL firmada de descarga válida por 5 minutos")
            .RequireAuthorization(Politicas.GestionarDocumentos)
            .Produces<EnlaceDescarga>()
            .Produces<ErrorApi>(StatusCodes.Status404NotFound);

        // El token firmado reemplaza la autenticación: la descarga nunca usa una ruta pública del archivo.
        grupo.MapGet("/archivo", async (string token, DocumentosService servicio, CancellationToken ct) =>
            {
                var archivo = await servicio.AbrirAsync(token, ct);
                return Results.File(archivo.Contenido, archivo.TipoContenido, archivo.NombreArchivo);
            })
            .AllowAnonymous()
            .WithSummary("Descarga el archivo con el token de la URL firmada")
            .Produces(StatusCodes.Status200OK)
            .Produces<ErrorApi>(StatusCodes.Status404NotFound);

        // Catálogos de datos semilla para formularios y filtros.
        app.MapGet("/api/v1/catalogos", async (CatalogosQueries consultas, CancellationToken ct) => Results.Ok(await consultas.ObtenerAsync(ct)))
            .WithTags("Catálogos")
            .WithSummary("Programas, tipos de proyecto, estados, tipos de documento y tipos de aliado")
            .Produces<Catalogos>();
    }
}
