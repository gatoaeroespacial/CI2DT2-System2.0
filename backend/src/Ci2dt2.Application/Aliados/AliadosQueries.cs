using Ci2dt2.Application.Common;
using Dapper;

namespace Ci2dt2.Application.Aliados;

public sealed record AliadoOpcion(int Id, string Nombre);

/// <summary>Lista de aliados para asociar usuarios Externos (el CRUD completo es HU-07).</summary>
public sealed class AliadosQueries(IDbConnectionFactory conexiones)
{
    public async Task<IReadOnlyList<AliadoOpcion>> ListarAsync(CancellationToken ct = default)
    {
        using var conexion = conexiones.Crear();
        var filas = await conexion.QueryAsync<AliadoOpcion>(
            new CommandDefinition("SELECT Id, Nombre FROM Aliados ORDER BY Nombre", cancellationToken: ct));
        return filas.ToList();
    }
}
