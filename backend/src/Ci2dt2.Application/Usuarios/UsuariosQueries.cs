using Ci2dt2.Application.Common;
using Dapper;

namespace Ci2dt2.Application.Usuarios;

/// <summary>Consultas de usuarios con Dapper.</summary>
public sealed class UsuariosQueries(IDbConnectionFactory conexiones)
{
    public const int TamanoPagina = 20;

    private const string Filtro = """
        FROM Usuarios u
        WHERE (@Busqueda IS NULL
               OR (u.Nombres + ' ' + u.Apellidos) LIKE @Busqueda ESCAPE '\'
               OR u.Email LIKE @Busqueda ESCAPE '\'
               OR u.NumeroDocumento LIKE @Busqueda ESCAPE '\')
          AND (@Rol IS NULL OR EXISTS (
               SELECT 1 FROM UsuarioRoles ur JOIN Roles r ON r.Id = ur.RolId
               WHERE ur.UsuarioId = u.Id AND r.Nombre = @Rol))
          AND (@TieneAcceso IS NULL OR u.TieneAcceso = @TieneAcceso)
        """;

    private sealed class Fila
    {
        public int Id { get; init; }
        public string Nombres { get; init; } = "";
        public string Apellidos { get; init; } = "";
        public string Email { get; init; } = "";
        public string? TipoDocumento { get; init; }
        public string? NumeroDocumento { get; init; }
        public bool TieneAcceso { get; init; }
        public string? Roles { get; init; }
    }

    /// <summary>Listado paginado (20 por página) con búsqueda por nombre, correo o documento.</summary>
    public async Task<Pagina<UsuarioListadoItem>> ListarAsync(FiltroUsuarios filtro, CancellationToken ct = default)
    {
        var pagina = Math.Max(1, filtro.Pagina);
        var busqueda = string.IsNullOrWhiteSpace(filtro.Busqueda) ? null : $"%{EscaparLike(filtro.Busqueda.Trim())}%";
        var parametros = new
        {
            Busqueda = busqueda,
            Rol = string.IsNullOrWhiteSpace(filtro.Rol) ? null : filtro.Rol,
            filtro.TieneAcceso,
            Offset = (pagina - 1) * TamanoPagina,
            Tamano = TamanoPagina,
        };

        var sql = $"""
            SELECT COUNT(*) {Filtro};

            SELECT u.Id, u.Nombres, u.Apellidos, u.Email, u.TipoDocumento, u.NumeroDocumento, u.TieneAcceso,
                   (SELECT STRING_AGG(r.Nombre, ',') WITHIN GROUP (ORDER BY r.Nombre)
                    FROM UsuarioRoles ur JOIN Roles r ON r.Id = ur.RolId
                    WHERE ur.UsuarioId = u.Id) AS Roles
            {Filtro}
            ORDER BY u.Apellidos, u.Nombres, u.Id
            OFFSET @Offset ROWS FETCH NEXT @Tamano ROWS ONLY;
            """;

        using var conexion = conexiones.Crear();
        using var resultado = await conexion.QueryMultipleAsync(new CommandDefinition(sql, parametros, cancellationToken: ct));
        var total = await resultado.ReadSingleAsync<int>();
        var filas = await resultado.ReadAsync<Fila>();

        var items = filas.Select(f => new UsuarioListadoItem
        {
            Id = f.Id,
            Nombres = f.Nombres,
            Apellidos = f.Apellidos,
            Email = f.Email,
            TipoDocumento = f.TipoDocumento,
            NumeroDocumento = f.NumeroDocumento,
            TieneAcceso = f.TieneAcceso,
            Roles = string.IsNullOrEmpty(f.Roles) ? [] : f.Roles.Split(','),
        }).ToList();

        return new Pagina<UsuarioListadoItem>(items, total, pagina, TamanoPagina);
    }

    /// <summary>
    /// Búsqueda de usuarios activos con acceso para vincularlos a programas o proyectos
    /// (HU-06 y HU-09: "el integrante debe existir como usuario").
    /// </summary>
    public async Task<IReadOnlyList<UsuarioOpcion>> BuscarAsync(string? texto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(texto) || texto.Trim().Length < 2)
            return [];
        const string sql = """
            SELECT TOP 10 u.Id, u.Nombres + ' ' + u.Apellidos AS Nombre, u.Email
            FROM Usuarios u
            WHERE u.Activo = 1 AND u.TieneAcceso = 1
              AND ((u.Nombres + ' ' + u.Apellidos) LIKE @filtro ESCAPE '\' OR u.Email LIKE @filtro ESCAPE '\'
                   OR u.NumeroDocumento LIKE @filtro ESCAPE '\')
            ORDER BY u.Apellidos, u.Nombres
            """;
        using var conexion = conexiones.Crear();
        return (await conexion.QueryAsync<UsuarioOpcion>(new CommandDefinition(sql,
            new { filtro = $"%{EscaparLike(texto.Trim())}%" }, cancellationToken: ct))).ToList();
    }

    private static string EscaparLike(string valor) =>
        valor.Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_").Replace("[", @"\[");
}
