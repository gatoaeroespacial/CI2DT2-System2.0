using System.Globalization;
using System.Text;
using Ci2dt2.Application.Common;
using Ci2dt2.Application.Convocatorias;
using Ci2dt2.Application.Proyectos;
using Dapper;

namespace Ci2dt2.Application.Publico;

public sealed record CifrasPortal(int ProyectosEnEjecucion, int ProyectosFinalizados, int Aliados, int Programas);

public sealed record ProgramaPublico(int Id, string Nombre, string Slug, string? Descripcion, string? Coordinador, int ProyectosPublicos);

public sealed record ConvocatoriaPublica(int Id, string Nombre, string? Numero, string TipoConvocatoria, string? EntidadConvocante,
    string Plataforma, string? Url, DateTime? FechaApertura, DateTime? FechaCierre, string? Moneda, decimal? MontoMaximo);

public sealed record ResumenPortal(CifrasPortal Cifras, IReadOnlyList<ProgramaPublico> Programas, IReadOnlyList<ConvocatoriaPublica> ConvocatoriasAbiertas);

public sealed record PersonaPublica(string Nombre, string? CvLacUrl, string? Orcid);

public sealed record ProyectoPublicoItem(string Codigo, string Slug, string Titulo, string? Resumen, string Programa, string InvestigadorPrincipal,
    string Estado, decimal? PorcentajeAvance, DateTime? FechaInicio, DateTime? FechaFin);

public sealed record ProgramaPublicoDetalle(
    string Nombre, string Slug, string? Descripcion, IReadOnlyList<string> Objetivos, PersonaPublica? Coordinador,
    IReadOnlyList<PersonaPublica> Investigadores, IReadOnlyList<string> Aliados, IReadOnlyList<ProyectoPublicoItem> Proyectos);

public sealed record ProyectoPublicoDetalle(ProyectoPublicoItem Proyecto, IReadOnlyList<PersonaPublica> Equipo);

public sealed record FiltroPortal(int? ProgramaId, int? EstadoId, int? Anio, string? Busqueda, int Pagina = 1);

/// <summary>
/// HU-25, HU-26 y HU-27: API pública del portal. Solo publica proyectos Aprobados, En ejecución o
/// Finalizados y nunca correos, teléfonos, documentos de identidad, presupuestos ni fuentes.
/// </summary>
public sealed class PortalQueries(IDbConnectionFactory conexiones, ConvocatoriasService convocatorias)
{
    public const int TamanoPagina = 12;
    private static readonly string EstadosPublicos = string.Join(",", ValoresProyecto.EstadosPublicos);

    /// <summary>URL amigable del programa: minúsculas, sin tildes y con guiones (no se almacena).</summary>
    public static string Slug(string texto)
    {
        var normalizado = texto.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalizado)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
                continue;
            sb.Append(char.IsLetterOrDigit(c) ? char.ToLowerInvariant(c) : '-');
        }
        var slug = sb.ToString();
        while (slug.Contains("--"))
            slug = slug.Replace("--", "-");
        return slug.Trim('-');
    }

    public async Task<ResumenPortal> ResumenAsync(CancellationToken ct = default)
    {
        var sql = $"""
            SELECT
              (SELECT COUNT(*) FROM Proyectos WHERE Activo = 1 AND EstadoId = {ValoresProyecto.EnEjecucion}) AS ProyectosEnEjecucion,
              (SELECT COUNT(*) FROM Proyectos WHERE Activo = 1 AND EstadoId = {ValoresProyecto.Finalizado}) AS ProyectosFinalizados,
              (SELECT COUNT(*) FROM Aliados WHERE Activo = 1) AS Aliados,
              (SELECT COUNT(*) FROM Programas WHERE Activo = 1) AS Programas;
            """;
        using var conexion = conexiones.Crear();
        var cifras = await conexion.QuerySingleAsync<CifrasPortal>(new CommandDefinition(sql, cancellationToken: ct));
        var abiertas = (await convocatorias.AbiertasAsync(ct))
            .Select(c => new ConvocatoriaPublica(c.Id, c.Nombre, c.Numero, c.TipoConvocatoria, c.EntidadConvocante, c.Plataforma, c.Url,
                c.FechaApertura, c.FechaCierre, c.Moneda, c.MontoMaximo)).ToList();
        return new ResumenPortal(cifras, await ProgramasAsync(ct), abiertas);
    }

    public async Task<IReadOnlyList<ProgramaPublico>> ProgramasAsync(CancellationToken ct = default)
    {
        var sql = $"""
            SELECT p.Id, p.Nombre, p.Descripcion,
                   (SELECT TOP 1 u.Nombres + ' ' + u.Apellidos FROM ProgramaIntegrantes pi JOIN Usuarios u ON u.Id = pi.UsuarioId
                    WHERE pi.ProgramaId = p.Id AND pi.Activo = 1 AND pi.RolEnPrograma = N'Coordinador'
                      AND (pi.FechaFin IS NULL OR pi.FechaFin > CAST(SYSUTCDATETIME() AS date))) AS Coordinador,
                   (SELECT COUNT(*) FROM Proyectos x WHERE x.ProgramaId = p.Id AND x.Activo = 1 AND x.EstadoId IN ({EstadosPublicos})) AS ProyectosPublicos
            FROM Programas p WHERE p.Activo = 1 ORDER BY p.Id
            """;
        using var conexion = conexiones.Crear();
        var filas = await conexion.QueryAsync<(int Id, string Nombre, string? Descripcion, string? Coordinador, int ProyectosPublicos)>(
            new CommandDefinition(sql, cancellationToken: ct));
        return filas.Select(f => new ProgramaPublico(f.Id, f.Nombre, Slug(f.Nombre), f.Descripcion, f.Coordinador, f.ProyectosPublicos)).ToList();
    }

    public async Task<ProgramaPublicoDetalle?> ProgramaAsync(string slug, CancellationToken ct = default)
    {
        var programa = (await ProgramasAsync(ct)).FirstOrDefault(p => p.Slug == slug.ToLowerInvariant());
        if (programa is null)
            return null;

        var sql = $"""
            SELECT Descripcion FROM ObjetivosEspecificos WHERE ProgramaId = @id AND Activo = 1 AND Descripcion IS NOT NULL ORDER BY Orden, Id;

            SELECT u.Nombres + ' ' + u.Apellidos AS Nombre, u.CvLacUrl, u.Orcid, pi.RolEnPrograma
            FROM ProgramaIntegrantes pi JOIN Usuarios u ON u.Id = pi.UsuarioId
            WHERE pi.ProgramaId = @id AND pi.Activo = 1 AND u.Activo = 1
              AND (pi.FechaInicio IS NULL OR pi.FechaInicio <= CAST(SYSUTCDATETIME() AS date))
              AND (pi.FechaFin IS NULL OR pi.FechaFin > CAST(SYSUTCDATETIME() AS date))
            ORDER BY u.Apellidos, u.Nombres;

            SELECT a.Nombre FROM ProgramaAliados pa JOIN Aliados a ON a.Id = pa.AliadoId
            WHERE pa.ProgramaId = @id AND pa.Activo = 1 AND a.Activo = 1 ORDER BY a.Nombre;
            """;
        using var conexion = conexiones.Crear();
        using var r = await conexion.QueryMultipleAsync(new CommandDefinition(sql, new { id = programa.Id }, cancellationToken: ct));
        var objetivos = (await r.ReadAsync<string>()).ToList();
        var integrantes = (await r.ReadAsync<(string Nombre, string? CvLacUrl, string? Orcid, string RolEnPrograma)>()).ToList();
        var aliados = (await r.ReadAsync<string>()).ToList();
        var coordinador = integrantes.Where(i => i.RolEnPrograma == "Coordinador")
            .Select(i => new PersonaPublica(i.Nombre, i.CvLacUrl, i.Orcid)).FirstOrDefault();
        var investigadores = integrantes.Where(i => i.RolEnPrograma != "Coordinador")
            .Select(i => new PersonaPublica(i.Nombre, i.CvLacUrl, i.Orcid)).ToList();
        var proyectos = await ProyectosAsync(new FiltroPortal(programa.Id, null, null, null, 1), todos: true, ct);
        return new ProgramaPublicoDetalle(programa.Nombre, programa.Slug, programa.Descripcion, objetivos, coordinador, investigadores, aliados, proyectos.Items);
    }

    public Task<Pagina<ProyectoPublicoItem>> ProyectosAsync(FiltroPortal f, CancellationToken ct = default) =>
        ProyectosAsync(f, todos: false, ct);

    private async Task<Pagina<ProyectoPublicoItem>> ProyectosAsync(FiltroPortal f, bool todos, CancellationToken ct)
    {
        var pagina = Math.Max(1, f.Pagina);
        var busqueda = string.IsNullOrWhiteSpace(f.Busqueda) ? null : $"%{f.Busqueda.Trim().Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]")}%";
        var filtro = $"""
            WHERE p.Activo = 1 AND p.EstadoId IN ({EstadosPublicos}) AND p.Codigo IS NOT NULL
              AND (@programaId IS NULL OR p.ProgramaId = @programaId)
              AND (@estadoId IS NULL OR p.EstadoId = @estadoId)
              AND (@anio IS NULL OR YEAR(COALESCE(p.FechaInicio, p.CreadoEn)) = @anio)
              AND (@busqueda IS NULL OR p.Titulo LIKE @busqueda OR p.Codigo LIKE @busqueda)
            """;
        var sql = $"""
            SELECT COUNT(*) FROM Proyectos p {filtro};
            SELECT p.Codigo, p.Titulo, p.Resumen, pr.Nombre AS Programa, ip.Nombres + ' ' + ip.Apellidos AS InvestigadorPrincipal,
                   e.Nombre AS Estado, p.PorcentajeAvance, CAST(p.FechaInicio AS datetime2) AS FechaInicio, CAST(p.FechaFin AS datetime2) AS FechaFin
            FROM Proyectos p JOIN Programas pr ON pr.Id = p.ProgramaId JOIN EstadosProyecto e ON e.Id = p.EstadoId
            JOIN Usuarios ip ON ip.Id = p.InvestigadorPrincipalId
            {filtro}
            ORDER BY COALESCE(p.FechaInicio, CAST(p.CreadoEn AS date)) DESC, p.Id DESC
            {(todos ? "" : "OFFSET @salto ROWS FETCH NEXT @tamano ROWS ONLY")};
            """;
        using var conexion = conexiones.Crear();
        using var r = await conexion.QueryMultipleAsync(new CommandDefinition(sql, new
        {
            f.ProgramaId,
            f.EstadoId,
            f.Anio,
            busqueda,
            salto = (pagina - 1) * TamanoPagina,
            tamano = TamanoPagina,
        }, cancellationToken: ct));
        var total = await r.ReadSingleAsync<int>();
        var items = (await r.ReadAsync<FilaProyecto>()).Select(AItem).ToList();
        return new Pagina<ProyectoPublicoItem>(items, total, pagina, todos ? Math.Max(total, 1) : TamanoPagina);
    }

    /// <summary>La URL pública del proyecto usa su código en minúsculas (CA-1: los no públicos responden 404).</summary>
    public async Task<ProyectoPublicoDetalle?> ProyectoAsync(string codigo, CancellationToken ct = default)
    {
        var sql = $"""
            SELECT p.Id, p.Codigo, p.Titulo, p.Resumen, pr.Nombre AS Programa, ip.Nombres + ' ' + ip.Apellidos AS InvestigadorPrincipal,
                   e.Nombre AS Estado, p.PorcentajeAvance, CAST(p.FechaInicio AS datetime2) AS FechaInicio, CAST(p.FechaFin AS datetime2) AS FechaFin
            FROM Proyectos p JOIN Programas pr ON pr.Id = p.ProgramaId JOIN EstadosProyecto e ON e.Id = p.EstadoId
            JOIN Usuarios ip ON ip.Id = p.InvestigadorPrincipalId
            WHERE p.Activo = 1 AND p.EstadoId IN ({EstadosPublicos}) AND UPPER(p.Codigo) = UPPER(@codigo);

            SELECT u.Nombres + ' ' + u.Apellidos AS Nombre, u.CvLacUrl, u.Orcid
            FROM ProyectoIntegrantes pi JOIN Usuarios u ON u.Id = pi.UsuarioId JOIN Proyectos p ON p.Id = pi.ProyectoId
            WHERE UPPER(p.Codigo) = UPPER(@codigo) AND pi.Activo = 1
              AND (pi.FechaFin IS NULL OR pi.FechaFin > CAST(SYSUTCDATETIME() AS date))
            ORDER BY CASE WHEN pi.RolEnProyecto = N'IP' THEN 0 ELSE 1 END, u.Apellidos, u.Nombres;
            """;
        using var conexion = conexiones.Crear();
        using var r = await conexion.QueryMultipleAsync(new CommandDefinition(sql, new { codigo }, cancellationToken: ct));
        var fila = await r.ReadSingleOrDefaultAsync<FilaProyecto>();
        if (fila is null)
            return null;
        var equipo = (await r.ReadAsync<(string Nombre, string? CvLacUrl, string? Orcid)>())
            .Select(e => new PersonaPublica(e.Nombre, e.CvLacUrl, e.Orcid)).ToList();
        return new ProyectoPublicoDetalle(AItem(fila), equipo);
    }

    private static ProyectoPublicoItem AItem(FilaProyecto f) => new(f.Codigo, f.Codigo.ToLowerInvariant(), f.Titulo, f.Resumen, f.Programa,
        f.InvestigadorPrincipal, f.Estado, f.PorcentajeAvance, f.FechaInicio, f.FechaFin);

    private sealed class FilaProyecto
    {
        public string Codigo { get; init; } = "";
        public string Titulo { get; init; } = "";
        public string? Resumen { get; init; }
        public string Programa { get; init; } = "";
        public string InvestigadorPrincipal { get; init; } = "";
        public string Estado { get; init; } = "";
        public decimal? PorcentajeAvance { get; init; }
        public DateTime? FechaInicio { get; init; }
        public DateTime? FechaFin { get; init; }
    }
}
