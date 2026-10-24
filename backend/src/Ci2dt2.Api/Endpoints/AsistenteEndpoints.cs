using Ci2dt2.Api.Infraestructura;
using Ci2dt2.Application.Asistente;
using Ci2dt2.Application.Seguridad;

namespace Ci2dt2.Api.Endpoints;

/// <summary>Consultas de solo lectura sobre las entidades autorizadas para el usuario.</summary>
public sealed class AsistenteEndpoints : IEndpointModule
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGroup("/api/v1/asistente")
            .WithTags("Asistente")
            .MapPost("/consultas", async (
                AsistenteRequest request,
                AsistenteConsultas consultas,
                HttpContext http,
                CancellationToken ct) =>
                Results.Ok(await consultas.ConsultarAsync(request, http.User.UsuarioAutenticado(), ct)))
            .Validar<AsistenteRequest>()
            .WithSummary("Consulta entidades autorizadas en lenguaje natural con alcance según el rol")
            .RequireAuthorization(Politicas.UsarAsistente)
            .Produces<AsistenteRespuesta>()
            .Produces<ErrorApi>(StatusCodes.Status400BadRequest)
            .Produces<ErrorApi>(StatusCodes.Status403Forbidden)
            .Produces<ErrorApi>(StatusCodes.Status503ServiceUnavailable);
    }
}
