using System.Text.Json;
using Ci2dt2.Api.Infraestructura;
using Ci2dt2.Application.Asistente;
using Ci2dt2.Application.Common;
using Ci2dt2.Application.Documentos;
using Ci2dt2.Application.Seguridad;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ci2dt2.Api.Endpoints;

/// <summary>
/// Endpoints para el Asistente IA de formulación de proyectos: ingesta de TDRs,
/// ideación guiada, redacción asistida con streaming SSE, auditoría de borradores
/// y sincronización con el Draft Canvas.
/// </summary>
public sealed class ProyectosAsistenteEndpoints : IEndpointModule
{
    public static void Map(IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/v1/proyectos/asistente")
            .WithTags("Proyectos - Asistente IA")
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        // 1. Ingesta y extracción de texto de TDR o borradores (PDF / DOCX)
        grupo.MapPost("/documentos/extraer", async (
                [FromForm] IFormFile? archivo,
                IDocumentParserService parser,
                HttpContext http,
                CancellationToken ct) =>
            {
                if (archivo is null || archivo.Length == 0)
                    return Results.BadRequest(new ErrorApi("Debe adjuntar un archivo PDF o DOCX."));

                if (archivo.Length > DocumentosService.TamanoMaximo)
                    return Results.BadRequest(new ErrorApi("El archivo supera el tamaño máximo permitido de 25 MB."));

                if (!parser.SoportaArchivo(archivo.FileName))
                    return Results.BadRequest(new ErrorApi("Formato no compatible. Solo se admiten archivos .pdf y .docx."));

                await using var stream = archivo.OpenReadStream();
                var resultado = await parser.ExtraerTextoAsync(stream, archivo.FileName, ct);

                return Results.Ok(new
                {
                    resultado.NombreArchivo,
                    resultado.Extension,
                    resultado.TotalPaginas,
                    resultado.TotalPalabras,
                    resultado.TamanoBytes,
                    resultado.TextoCompleto
                });
            })
            .DisableAntiforgery()
            .WithSummary("Extrae el texto y métricas de un archivo TDR o borrador (PDF/DOCX, máx. 25 MB)")
            .RequireAuthorization(Politicas.CrearPropuestas)
            .Produces(StatusCodes.Status200OK)
            .Produces<ErrorApi>(StatusCodes.Status400BadRequest);

        // 2. Análisis de TDR e Ideación de Proyectos alineados
        grupo.MapPost("/analizar-tdr", async (
                [FromBody] AnalizarTdrApiRequest request,
                IAgenteFormulacionService agente,
                IApplicationDbContext db,
                HttpContext http,
                CancellationToken ct) =>
            {
                var usuario = http.User.UsuarioAutenticado();
                var linea = request.LineaInvestigacion;

                if (string.IsNullOrWhiteSpace(linea))
                {
                    var integrante = await db.ProgramaIntegrantes
                        .Include(pi => pi.Programa)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(pi => pi.UsuarioId == usuario.Id && pi.Activo, ct);
                    linea = integrante?.Programa?.Nombre ?? "Tecnologías de la Información y Desarrollo Regional";
                }

                var resultado = await agente.AnalizarTdrEIdearAsync(new AnalisisTdrRequest(
                    TextoTdr: request.TextoTdr,
                    NombreConvocatoria: request.NombreConvocatoria,
                    LineaInvestigacionUsuario: linea,
                    NombrePrograma: request.NombrePrograma,
                    NombreInvestigador: http.User.Identity?.Name ?? $"Investigador #{usuario.Id}"
                ), ct);

                return Results.Ok(resultado);
            })
            .WithSummary("Analiza el TDR y genera un resumen ejecutivo junto con 3 ideas de proyectos alineadas")
            .RequireAuthorization(Politicas.CrearPropuestas)
            .Produces<AnalisisTdrResultado>()
            .Produces<ErrorApi>(StatusCodes.Status400BadRequest);

        // 3. Redacción asistida de sección con Streaming SSE
        grupo.MapPost("/redactar-seccion", async (
                [FromBody] RedactarSeccionApiRequest request,
                IAgenteFormulacionService agente,
                BorradorProyectosService borradorServicio,
                HttpContext http,
                CancellationToken ct) =>
            {
                var usuario = http.User.UsuarioAutenticado();
                var borrador = await borradorServicio.ObtenerBorradorAsync(request.BorradorId, usuario, ct);
                if (borrador is null)
                {
                    http.Response.StatusCode = StatusCodes.Status404NotFound;
                    await http.Response.WriteAsJsonAsync(new ErrorApi("Borrador no encontrado."), ct);
                    return;
                }

                var seccionesDict = borrador.Secciones
                    .Where(s => !string.IsNullOrWhiteSpace(s.Contenido))
                    .ToDictionary(s => s.CodigoSeccion, s => s.Contenido);

                http.Response.ContentType = "text/event-stream; charset=utf-8";
                http.Response.Headers.CacheControl = "no-cache";
                http.Response.Headers.Append("X-Accel-Buffering", "no");

                var reqAgente = new RedactarSeccionRequest(
                    BorradorId: request.BorradorId,
                    CodigoSeccion: request.CodigoSeccion,
                    TituloProyecto: borrador.TituloTentativo,
                    IdeaSeleccionada: request.IdeaSeleccionada,
                    ConvocatoriaMetadataJson: borrador.ConvocatoriaMetadataJson,
                    SeccionesExistentes: seccionesDict,
                    InstruccionesUsuario: request.InstruccionesUsuario);

                await foreach (var fragmento in agente.RedactarSeccionStreamAsync(reqAgente, ct))
                {
                    var data = JsonSerializer.Serialize(new { chunk = fragmento });
                    await http.Response.WriteAsync($"data: {data}\n\n", ct);
                    await http.Response.Body.FlushAsync(ct);
                }

                await http.Response.WriteAsync("data: [DONE]\n\n", ct);
                await http.Response.Body.FlushAsync(ct);
            })
            .WithSummary("Genera en streaming SSE el borrador argumentativo de una sección del proyecto")
            .RequireAuthorization(Politicas.CrearPropuestas)
            .Produces(StatusCodes.Status200OK);

        // 4. Auditoría de borrador previo (Gap Analysis)
        grupo.MapPost("/auditar-borrador", async (
                [FromBody] AuditarBorradorApiRequest request,
                IAgenteFormulacionService agente,
                HttpContext http,
                CancellationToken ct) =>
            {
                if (string.IsNullOrWhiteSpace(request.TextoBorrador))
                    return Results.BadRequest(new ErrorApi("El texto del borrador no puede estar vacío."));

                var resultado = await agente.AuditarBorradorAsync(new AuditoriaBorradorRequest(
                    TextoBorradorCompleto: request.TextoBorrador,
                    ConvocatoriaMetadataJson: request.ConvocatoriaMetadataJson
                ), ct);

                return Results.Ok(resultado);
            })
            .WithSummary("Evalúa un borrador existente e identifica secciones completas, incompletas y faltantes")
            .RequireAuthorization(Politicas.CrearPropuestas)
            .Produces<AuditoriaBorradorResultado>()
            .Produces<ErrorApi>(StatusCodes.Status400BadRequest);

        // 5. Listar borradores de proyectos en formulación
        grupo.MapGet("/borradores", async (
                BorradorProyectosService servicio,
                HttpContext http,
                CancellationToken ct) =>
            {
                var usuario = http.User.UsuarioAutenticado();
                var borradores = await servicio.ListarBorradoresUsuarioAsync(usuario, ct);
                return Results.Ok(borradores);
            })
            .WithSummary("Lista los borradores de proyectos formulados por el usuario o accesibles por su rol")
            .RequireAuthorization(Politicas.VerProyectos)
            .Produces<IReadOnlyList<BorradorResumenItem>>();

        // 6. Crear nuevo borrador de proyecto
        grupo.MapPost("/borradores", async (
                CrearBorradorRequest request,
                BorradorProyectosService servicio,
                HttpContext http,
                CancellationToken ct) =>
            {
                var usuario = http.User.UsuarioAutenticado();
                var id = await servicio.CrearBorradorAsync(request, usuario, ct);
                return Results.Created($"/api/v1/proyectos/asistente/borradores/{id}", new { id });
            })
            .WithSummary("Crea un nuevo borrador de proyecto con sus secciones modulares vacías")
            .RequireAuthorization(Politicas.CrearPropuestas)
            .Produces(StatusCodes.Status201Created)
            .Produces<ErrorApi>(StatusCodes.Status400BadRequest);

        // 7. Obtener detalle del borrador (con secciones y mensajes)
        grupo.MapGet("/borradores/{id:int}", async (
                int id,
                BorradorProyectosService servicio,
                HttpContext http,
                CancellationToken ct) =>
            {
                var usuario = http.User.UsuarioAutenticado();
                var detalle = await servicio.ObtenerBorradorAsync(id, usuario, ct);
                return detalle is null ? Results.NotFound(new ErrorApi("Borrador no encontrado.")) : Results.Ok(detalle);
            })
            .WithSummary("Obtiene el estado completo del borrador: lienzo con secciones y chat")
            .RequireAuthorization(Politicas.VerProyectos)
            .Produces<BorradorProyectoDetalle>()
            .Produces<ErrorApi>(StatusCodes.Status404NotFound);

        // 8. Actualizar sección individual en el Draft Canvas
        grupo.MapPut("/borradores/{id:int}/secciones/{codigoSeccion}", async (
                int id,
                string codigoSeccion,
                ActualizarSeccionRequest request,
                BorradorProyectosService servicio,
                HttpContext http,
                CancellationToken ct) =>
            {
                var usuario = http.User.UsuarioAutenticado();
                var actualizado = await servicio.ActualizarSeccionAsync(id, codigoSeccion, request, usuario, ct);
                return actualizado
                    ? Results.NoContent()
                    : Results.NotFound(new ErrorApi("Borrador o sección no encontrada."));
            })
            .WithSummary("Actualiza el contenido o estado de una sección específica del borrador")
            .RequireAuthorization(Politicas.CrearPropuestas)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorApi>(StatusCodes.Status404NotFound);

        // 9. Registrar mensaje conversacional en el hilo del borrador
        grupo.MapPost("/borradores/{id:int}/mensajes", async (
                int id,
                AgregarMensajeBorradorRequest request,
                BorradorProyectosService servicio,
                HttpContext http,
                CancellationToken ct) =>
            {
                var usuario = http.User.UsuarioAutenticado();
                var mensajeId = await servicio.AgregarMensajeAsync(id, request, usuario, ct);
                return Results.Ok(new { id = mensajeId });
            })
            .WithSummary("Agrega un mensaje al historial del borrador")
            .RequireAuthorization(Politicas.CrearPropuestas)
            .Produces(StatusCodes.Status200OK);

        // 10. Asociar metadata de convocatoria / TDR analizado
        grupo.MapPut("/borradores/{id:int}/convocatoria", async (
                int id,
                [FromBody] MetadataConvocatoriaRequest request,
                BorradorProyectosService servicio,
                HttpContext http,
                CancellationToken ct) =>
            {
                var usuario = http.User.UsuarioAutenticado();
                await servicio.ActualizarMetadataConvocatoriaAsync(
                    id,
                    request.ConvocatoriaNombre,
                    request.MetadataJson,
                    usuario,
                    ct);
                return Results.NoContent();
            })
            .WithSummary("Actualiza la metadata del TDR analizado asociada al borrador")
            .RequireAuthorization(Politicas.CrearPropuestas)
            .Produces(StatusCodes.Status204NoContent);

        // 11. Exportar borrador consolidado a documento Word (.docx)
        grupo.MapGet("/borradores/{id:int}/exportar-docx", async (
                int id,
                IExportadorBorradorService exportador,
                HttpContext http,
                CancellationToken ct) =>
            {
                var usuario = http.User.UsuarioAutenticado();
                var (contenido, nombreArchivo) = await exportador.GenerarDocxAsync(id, usuario, ct);

                return Results.File(
                    contenido,
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    nombreArchivo);
            })
            .WithSummary("Exporta el borrador consolidado en formato DOCX con membrete institucional")
            .RequireAuthorization(Politicas.VerProyectos)
            .Produces(StatusCodes.Status200OK)
            .Produces<ErrorApi>(StatusCodes.Status404NotFound);
    }
}

public sealed record AnalizarTdrApiRequest(
    string TextoTdr,
    string? NombreConvocatoria,
    string? LineaInvestigacion,
    string? NombrePrograma);

public sealed record RedactarSeccionApiRequest(
    int BorradorId,
    string CodigoSeccion,
    string? InstruccionesUsuario,
    string? IdeaSeleccionada);

public sealed record AuditarBorradorApiRequest(
    string TextoBorrador,
    string? ConvocatoriaMetadataJson);

public sealed record MetadataConvocatoriaRequest(
    string ConvocatoriaNombre,
    string MetadataJson);
