using Ci2dt2.Api.Infraestructura;
using Ci2dt2.Application.Aliados;
using Ci2dt2.Application.Seguridad;

namespace Ci2dt2.Api.Endpoints;

/// <summary>Lectura del catálogo de aliados para asociar usuarios Externos (HU-04). El CRUD llega con HU-07.</summary>
public sealed class AliadosEndpoints : IEndpointModule
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/aliados", async (AliadosQueries consultas, CancellationToken ct) =>
                Results.Ok(await consultas.ListarAsync(ct)))
            .WithTags("Aliados")
            .WithSummary("Lista de aliados (id y nombre)")
            .RequireAuthorization(Politicas.GestionarAliadosConvocatorias)
            .Produces<IReadOnlyList<AliadoOpcion>>();
    }
}
