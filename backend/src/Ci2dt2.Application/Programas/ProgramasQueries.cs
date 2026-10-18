using Ci2dt2.Application.Common;
using Dapper;

namespace Ci2dt2.Application.Programas;

/// <summary>Consultas de programas con Dapper (HU-06).</summary>
public sealed class ProgramasQueries(IDbConnectionFactory conexiones)
{
    private sealed class FilaPrograma
    {
        public int Id { get; init; }
        public string Nombre { get; init; } = "";
        public string? Descripcion { get; init; }
    }

    private const string Vigente =
        "pi.Activo = 1 AND (pi.FechaInicio IS NULL OR pi.FechaInicio <= CAST(SYSUTCDATETIME() AS date)) " +
        "AND (pi.FechaFin IS NULL OR pi.FechaFin > CAST(SYSUTCDATETIME() AS date))";

    public async Task<IReadOnlyList<ProgramaResumen>> ListarAsync(CancellationToken ct = default)
    {
        var sql = $"""
            SELECT p.Id, p.Nombre, p.Descripcion,
                   (SELECT TOP 1 u.Nombres + ' ' + u.Apellidos
                    FROM ProgramaIntegrantes pi JOIN Usuarios u ON u.Id = pi.UsuarioId
                    WHERE pi.ProgramaId = p.Id AND pi.RolEnPrograma = N'Coordinador' AND {Vigente}) AS Coordinador,
                   (SELECT COUNT(*) FROM ProgramaIntegrantes pi WHERE pi.ProgramaId = p.Id AND {Vigente}) AS IntegrantesVigentes
            FROM Programas p
            WHERE p.Activo = 1
            ORDER BY p.Id
            """;
        using var conexion = conexiones.Crear();
        return (await conexion.QueryAsync<ProgramaResumen>(new CommandDefinition(sql, cancellationToken: ct))).ToList();
    }

    public async Task<ProgramaDetalle?> ObtenerAsync(int id, CancellationToken ct = default)
    {
        const string sql = """
            SELECT Id, Nombre, Descripcion FROM Programas WHERE Id = @id AND Activo = 1;
            SELECT Id, Descripcion, Orden FROM ObjetivosEspecificos WHERE ProgramaId = @id AND Activo = 1 ORDER BY Orden, Id;
            SELECT a.Id, a.Nombre FROM ProgramaAliados pa JOIN Aliados a ON a.Id = pa.AliadoId
            WHERE pa.ProgramaId = @id AND pa.Activo = 1 AND a.Activo = 1 ORDER BY a.Nombre;
            """;
        using var conexion = conexiones.Crear();
        using var r = await conexion.QueryMultipleAsync(new CommandDefinition(sql, new { id }, cancellationToken: ct));
        var programa = await r.ReadSingleOrDefaultAsync<FilaPrograma>();
        if (programa is null)
            return null;
        var objetivos = (await r.ReadAsync<ObjetivoItem>()).ToList();
        var aliados = (await r.ReadAsync<AliadoProgramaItem>()).ToList();
        return new ProgramaDetalle(programa.Id, programa.Nombre, programa.Descripcion, objetivos, aliados);
    }

    /// <summary>Integrantes del programa: vigentes e históricos (HU-06 CA-3).</summary>
    public async Task<IReadOnlyList<IntegranteProgramaItem>> IntegrantesAsync(int programaId, CancellationToken ct = default)
    {
        var sql = $"""
            SELECT pi.Id, pi.UsuarioId, u.Nombres + ' ' + u.Apellidos AS Nombre, u.Email, pi.RolEnPrograma,
                   CAST(pi.FechaInicio AS datetime2) AS FechaInicio, CAST(pi.FechaFin AS datetime2) AS FechaFin,
                   CAST(CASE WHEN {Vigente} THEN 1 ELSE 0 END AS bit) AS Vigente
            FROM ProgramaIntegrantes pi JOIN Usuarios u ON u.Id = pi.UsuarioId
            WHERE pi.ProgramaId = @programaId AND pi.Activo = 1
            ORDER BY pi.RolEnPrograma, u.Apellidos, u.Nombres
            """;
        using var conexion = conexiones.Crear();
        return (await conexion.QueryAsync<IntegranteProgramaItem>(new CommandDefinition(sql, new { programaId }, cancellationToken: ct))).ToList();
    }
}
