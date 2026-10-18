using Ci2dt2.Application.Common;
using Dapper;

namespace Ci2dt2.Application.Aliados;

public sealed class AliadoItem
{
    public int Id { get; init; }
    public int TipoAliadoId { get; init; }
    public string TipoAliado { get; init; } = "";
    public string Nombre { get; init; } = "";
    public string? Nit { get; init; }
    public string? Pais { get; init; }
    public string? Ciudad { get; init; }
    public string? Sector { get; init; }
    public string? RepresentanteLegal { get; init; }
    public string Email { get; init; } = "";
}

/// <summary>Consultas del catálogo de aliados con Dapper (HU-07).</summary>
public sealed class AliadosQueries(IDbConnectionFactory conexiones)
{
    public const int TamanoPagina = 20;

    private const string Columnas = """
        a.Id, a.TipoAliadoId, t.Nombre AS TipoAliado, a.Nombre, a.Nit, a.Pais, a.Ciudad, a.Sector,
        a.RepresentanteLegal, a.Email
        """;

    /// <summary>Listado paginado; con búsqueda por nombre o NIT sirve también al autocompletado (CA-2).</summary>
    public async Task<Pagina<AliadoItem>> ListarAsync(string? busqueda, int pagina, CancellationToken ct = default)
    {
        pagina = Math.Max(1, pagina);
        var filtro = string.IsNullOrWhiteSpace(busqueda) ? null : $"%{EscaparLike(busqueda.Trim())}%";
        var sql = $"""
            SELECT COUNT(*) FROM Aliados a
            WHERE a.Activo = 1 AND (@filtro IS NULL OR a.Nombre LIKE @filtro ESCAPE '\' OR a.Nit LIKE @filtro ESCAPE '\');

            SELECT {Columnas}
            FROM Aliados a JOIN TiposAliado t ON t.Id = a.TipoAliadoId
            WHERE a.Activo = 1 AND (@filtro IS NULL OR a.Nombre LIKE @filtro ESCAPE '\' OR a.Nit LIKE @filtro ESCAPE '\')
            ORDER BY a.Nombre, a.Id
            OFFSET @offset ROWS FETCH NEXT @tamano ROWS ONLY;
            """;
        using var conexion = conexiones.Crear();
        using var r = await conexion.QueryMultipleAsync(new CommandDefinition(sql,
            new { filtro, offset = (pagina - 1) * TamanoPagina, tamano = TamanoPagina }, cancellationToken: ct));
        var total = await r.ReadSingleAsync<int>();
        var items = (await r.ReadAsync<AliadoItem>()).ToList();
        return new Pagina<AliadoItem>(items, total, pagina, TamanoPagina);
    }

    public async Task<AliadoItem?> ObtenerAsync(int id, CancellationToken ct = default)
    {
        var sql = $"SELECT {Columnas} FROM Aliados a JOIN TiposAliado t ON t.Id = a.TipoAliadoId WHERE a.Id = @id AND a.Activo = 1";
        using var conexion = conexiones.Crear();
        return await conexion.QuerySingleOrDefaultAsync<AliadoItem>(new CommandDefinition(sql, new { id }, cancellationToken: ct));
    }

    private static string EscaparLike(string valor) =>
        valor.Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_").Replace("[", @"\[");
}
