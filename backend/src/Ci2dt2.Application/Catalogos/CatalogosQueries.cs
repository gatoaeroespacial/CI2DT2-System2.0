using Ci2dt2.Application.Common;
using Dapper;

namespace Ci2dt2.Application.Catalogos;

public sealed record OpcionCatalogo(int Id, string Nombre);

public sealed record Catalogos(
    IReadOnlyList<OpcionCatalogo> Programas,
    IReadOnlyList<OpcionCatalogo> TiposProyecto,
    IReadOnlyList<OpcionCatalogo> EstadosProyecto,
    IReadOnlyList<OpcionCatalogo> TiposDocumento,
    IReadOnlyList<OpcionCatalogo> TiposAliado);

/// <summary>Catálogos de datos semilla que alimentan los formularios y filtros.</summary>
public sealed class CatalogosQueries(IDbConnectionFactory conexiones)
{
    public async Task<Catalogos> ObtenerAsync(CancellationToken ct = default)
    {
        const string sql = """
            SELECT Id, Nombre FROM Programas WHERE Activo = 1 ORDER BY Id;
            SELECT Id, Nombre FROM TiposProyecto WHERE Activo = 1 ORDER BY Id;
            SELECT Id, Nombre FROM EstadosProyecto WHERE Activo = 1 ORDER BY Orden, Id;
            SELECT Id, Nombre FROM TiposDocumento WHERE Activo = 1 ORDER BY Nombre;
            SELECT Id, Nombre FROM TiposAliado WHERE Activo = 1 ORDER BY Id;
            """;
        using var conexion = conexiones.Crear();
        using var r = await conexion.QueryMultipleAsync(new CommandDefinition(sql, cancellationToken: ct));
        return new Catalogos(
            (await r.ReadAsync<OpcionCatalogo>()).ToList(),
            (await r.ReadAsync<OpcionCatalogo>()).ToList(),
            (await r.ReadAsync<OpcionCatalogo>()).ToList(),
            (await r.ReadAsync<OpcionCatalogo>()).ToList(),
            (await r.ReadAsync<OpcionCatalogo>()).ToList());
    }
}
