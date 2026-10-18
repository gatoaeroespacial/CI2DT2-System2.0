using Ci2dt2.Application.Common;
using Ci2dt2.Application.Seguridad;
using Ci2dt2.Domain;
using Dapper;

namespace Ci2dt2.Application.Proyectos;

public sealed record FiltroProyectos(
    int? ProgramaId, int? EstadoId, int? TipoProyectoId, int? Anio, int? InvestigadorPrincipalId,
    string? Busqueda, string? Orden, bool Descendente, int Pagina = 1);

public sealed class ProyectoListadoItem
{
    public int Id { get; init; }
    public string? Codigo { get; init; }
    public string Titulo { get; init; } = "";
    public int ProgramaId { get; init; }
    public string Programa { get; init; } = "";
    public int InvestigadorPrincipalId { get; init; }
    public string InvestigadorPrincipal { get; init; } = "";
    public int EstadoId { get; init; }
    public string Estado { get; init; } = "";
    public string TipoProyecto { get; init; } = "";
    public decimal? PorcentajeAvance { get; init; }
    public DateTime? FechaFin { get; init; }
    public DateTime? ProximoHitoFecha { get; init; }
    public string? ProximoHitoTitulo { get; init; }
}

public sealed class ProyectoDetalle
{
    public int Id { get; init; }
    public string? Codigo { get; init; }
    public string Titulo { get; init; } = "";
    public string? Resumen { get; init; }
    public int ProgramaId { get; init; }
    public string Programa { get; init; } = "";
    public int TipoProyectoId { get; init; }
    public string TipoProyecto { get; init; } = "";
    public int EstadoId { get; init; }
    public string Estado { get; init; } = "";
    public bool EstadoEsFinal { get; init; }
    public int InvestigadorPrincipalId { get; init; }
    public string InvestigadorPrincipal { get; init; } = "";
    public DateTime? FechaInicio { get; init; }
    public DateTime? FechaFin { get; init; }
    public decimal? PorcentajeAvance { get; init; }
    public string? Moneda { get; init; }
    public decimal? PresupuestoTotal { get; init; }
    public decimal? TasaCambio { get; init; }
    public decimal? PresupuestoTotalCop { get; init; }
    public IReadOnlyList<PostulacionItem> Convocatorias { get; set; } = [];
    public IReadOnlyList<HitoItem> Cronograma { get; set; } = [];
    public IReadOnlyList<AvanceItem> Avances { get; set; } = [];
    public IReadOnlyList<RevisionItem> Revisiones { get; set; } = [];
}

public sealed record PostulacionItem(int Id, string Convocatoria, DateTime? FechaPresentacion, decimal? MontoSolicitadoCop, string? Resultado, string? FaseExterna);
public sealed record HitoItem(int Id, string Titulo, string TipoHito, DateTime FechaLimite, string Estado, string? Responsable, DateTime? FechaCompletado);
public sealed record AvanceItem(int Id, string Titulo, string Autor, decimal? PorcentajeAvance, DateTime? FechaCaptura, string? EstadoValidacion);
public sealed record RevisionItem(int Id, string Etapa, string Decision, string Revisor, string EstadoNuevo, string? Observaciones, DateTime Fecha);

/// <summary>Consultas de proyectos con Dapper (HU-13), con el alcance por rol de HU-05.</summary>
public sealed class ProyectosQueries(IDbConnectionFactory conexiones)
{
    public const int TamanoPagina = 20;

    private const string CoordinadorVigente = """
        EXISTS (SELECT 1 FROM ProgramaIntegrantes pi
                WHERE pi.ProgramaId = p.ProgramaId AND pi.UsuarioId = @usuarioId AND pi.Activo = 1
                  AND pi.RolEnPrograma = N'Coordinador'
                  AND (pi.FechaInicio IS NULL OR pi.FechaInicio <= CAST(SYSUTCDATETIME() AS date))
                  AND (pi.FechaFin IS NULL OR pi.FechaFin > CAST(SYSUTCDATETIME() AS date)))
        """;

    private const string EsOFueIntegrante = """
        EXISTS (SELECT 1 FROM ProyectoIntegrantes x WHERE x.ProyectoId = p.Id AND x.UsuarioId = @usuarioId AND x.Activo = 1)
        """;

    private static readonly Dictionary<string, string> Ordenes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["codigo"] = "p.Codigo",
        ["titulo"] = "p.Titulo",
        ["programa"] = "pr.Nombre",
        ["ip"] = "InvestigadorPrincipal",
        ["estado"] = "e.Orden",
        ["avance"] = "p.PorcentajeAvance",
        ["fechaFin"] = "p.FechaFin",
        ["proximoHito"] = "ProximoHitoFecha",
    };

    /// <summary>
    /// Listado con filtros, búsqueda, orden y paginación en el servidor (HU-13 CA-1 y CA-2).
    /// Alcance: Admin y Director todos; Coordinador su programa; Investigador sus proyectos.
    /// </summary>
    public Task<Pagina<ProyectoListadoItem>> ListarAsync(FiltroProyectos filtro, UsuarioAutenticado usuario, CancellationToken ct = default) =>
        ListarConAlcanceAsync(filtro, usuario, AlcanceDe(usuario), ct);

    /// <summary>"Mis proyectos": vinculaciones vigentes e históricas del usuario.</summary>
    public Task<Pagina<ProyectoListadoItem>> MiosAsync(FiltroProyectos filtro, UsuarioAutenticado usuario, CancellationToken ct = default) =>
        ListarConAlcanceAsync(filtro, usuario, EsOFueIntegrante, ct);

    private static string AlcanceDe(UsuarioAutenticado u)
    {
        if (u.Tiene(Roles.Administrador) || u.Tiene(Roles.Director))
            return "1 = 1";
        var condiciones = new List<string>();
        if (u.Tiene(Roles.Coordinador))
            condiciones.Add(CoordinadorVigente);
        if (u.Tiene(Roles.Investigador))
            condiciones.Add(EsOFueIntegrante);
        return condiciones.Count == 0 ? "1 = 0" : "(" + string.Join(" OR ", condiciones) + ")";
    }

    private async Task<Pagina<ProyectoListadoItem>> ListarConAlcanceAsync(
        FiltroProyectos f, UsuarioAutenticado usuario, string alcance, CancellationToken ct)
    {
        var pagina = Math.Max(1, f.Pagina);
        // Sin orden explícito: los más recientes primero.
        var orden = f.Orden is not null && Ordenes.TryGetValue(f.Orden, out var columna)
            ? $"{columna} {(f.Descendente ? "DESC" : "ASC")}, p.Id DESC"
            : "p.Id DESC";
        var busqueda = string.IsNullOrWhiteSpace(f.Busqueda) ? null : $"%{EscaparLike(f.Busqueda.Trim())}%";

        const string uniones = """
            FROM Proyectos p
            JOIN Programas pr ON pr.Id = p.ProgramaId
            JOIN EstadosProyecto e ON e.Id = p.EstadoId
            JOIN TiposProyecto t ON t.Id = p.TipoProyectoId
            JOIN Usuarios ip ON ip.Id = p.InvestigadorPrincipalId
            """;
        var filtros = $"""
            WHERE p.Activo = 1 AND {alcance}
              AND (@programaId IS NULL OR p.ProgramaId = @programaId)
              AND (@estadoId IS NULL OR p.EstadoId = @estadoId)
              AND (@tipoProyectoId IS NULL OR p.TipoProyectoId = @tipoProyectoId)
              AND (@anio IS NULL OR YEAR(COALESCE(p.FechaInicio, CAST(p.CreadoEn AS date))) = @anio)
              AND (@ipId IS NULL OR p.InvestigadorPrincipalId = @ipId)
              AND (@busqueda IS NULL OR p.Titulo LIKE @busqueda ESCAPE '\' OR p.Codigo LIKE @busqueda ESCAPE '\')
            """;

        var sql = $"""
            SELECT COUNT(*) {uniones} {filtros};

            SELECT p.Id, p.Codigo, p.Titulo, p.ProgramaId, pr.Nombre AS Programa,
                   p.InvestigadorPrincipalId, ip.Nombres + ' ' + ip.Apellidos AS InvestigadorPrincipal,
                   p.EstadoId, e.Nombre AS Estado, t.Nombre AS TipoProyecto, p.PorcentajeAvance,
                   CAST(p.FechaFin AS datetime2) AS FechaFin,
                   CAST(h.FechaLimite AS datetime2) AS ProximoHitoFecha, h.Titulo AS ProximoHitoTitulo
            {uniones}
            OUTER APPLY (SELECT TOP 1 hi.FechaLimite, hi.Titulo
                         FROM Hitos hi JOIN Cronogramas c ON c.Id = hi.CronogramaId
                         WHERE c.ProyectoId = p.Id AND c.Activo = 1 AND hi.Activo = 1
                           AND hi.Estado IN (N'Pendiente', N'En curso')
                         ORDER BY hi.FechaLimite) h
            {filtros}
            ORDER BY {orden}
            OFFSET @offset ROWS FETCH NEXT @tamano ROWS ONLY;
            """;

        using var conexion = conexiones.Crear();
        using var r = await conexion.QueryMultipleAsync(new CommandDefinition(sql, new
        {
            usuarioId = usuario.Id,
            programaId = f.ProgramaId,
            estadoId = f.EstadoId,
            tipoProyectoId = f.TipoProyectoId,
            anio = f.Anio,
            ipId = f.InvestigadorPrincipalId,
            busqueda,
            offset = (pagina - 1) * TamanoPagina,
            tamano = TamanoPagina,
        }, cancellationToken: ct));
        var total = await r.ReadSingleAsync<int>();
        var items = (await r.ReadAsync<ProyectoListadoItem>()).ToList();
        return new Pagina<ProyectoListadoItem>(items, total, pagina, TamanoPagina);
    }

    /// <summary>Ficha del proyecto con las pestañas de solo lectura (HU-13 CA-3).</summary>
    public async Task<ProyectoDetalle?> ObtenerAsync(int id, CancellationToken ct = default)
    {
        const string sql = """
            SELECT p.Id, p.Codigo, p.Titulo, p.Resumen, p.ProgramaId, pr.Nombre AS Programa,
                   p.TipoProyectoId, t.Nombre AS TipoProyecto, p.EstadoId, e.Nombre AS Estado, e.EsFinal AS EstadoEsFinal,
                   p.InvestigadorPrincipalId, ip.Nombres + ' ' + ip.Apellidos AS InvestigadorPrincipal,
                   CAST(p.FechaInicio AS datetime2) AS FechaInicio, CAST(p.FechaFin AS datetime2) AS FechaFin,
                   p.PorcentajeAvance, p.Moneda, p.PresupuestoTotal, p.TasaCambio, p.PresupuestoTotalCop
            FROM Proyectos p
            JOIN Programas pr ON pr.Id = p.ProgramaId
            JOIN EstadosProyecto e ON e.Id = p.EstadoId
            JOIN TiposProyecto t ON t.Id = p.TipoProyectoId
            JOIN Usuarios ip ON ip.Id = p.InvestigadorPrincipalId
            WHERE p.Id = @id AND p.Activo = 1;

            SELECT pc.Id, c.Nombre AS Convocatoria, CAST(pc.FechaPresentacion AS datetime2) AS FechaPresentacion,
                   pc.MontoSolicitadoCop, pc.Resultado, pc.FaseExterna
            FROM PropuestasConvocatoria pc JOIN Convocatorias c ON c.Id = pc.ConvocatoriaId
            WHERE pc.ProyectoId = @id AND pc.Activo = 1 ORDER BY pc.FechaPresentacion DESC;

            SELECT h.Id, h.Titulo, th.Nombre AS TipoHito, CAST(h.FechaLimite AS datetime2) AS FechaLimite, h.Estado,
                   u.Nombres + ' ' + u.Apellidos AS Responsable, CAST(h.FechaCompletado AS datetime2) AS FechaCompletado
            FROM Hitos h JOIN Cronogramas c ON c.Id = h.CronogramaId JOIN TiposHito th ON th.Id = h.TipoHitoId
            LEFT JOIN Usuarios u ON u.Id = h.ResponsableId
            WHERE c.ProyectoId = @id AND c.Activo = 1 AND h.Activo = 1 ORDER BY h.FechaLimite, h.Orden;

            SELECT a.Id, a.Titulo, u.Nombres + ' ' + u.Apellidos AS Autor, a.PorcentajeAvance, a.FechaCaptura, a.EstadoValidacion
            FROM Avances a JOIN Usuarios u ON u.Id = a.AutorId
            WHERE a.ProyectoId = @id AND a.Activo = 1 ORDER BY a.FechaCaptura DESC;

            SELECT r.Id, r.Etapa, r.Decision, u.Nombres + ' ' + u.Apellidos AS Revisor, e.Nombre AS EstadoNuevo, r.Observaciones, r.Fecha
            FROM ProyectoRevisiones r JOIN Usuarios u ON u.Id = r.RevisorId JOIN EstadosProyecto e ON e.Id = r.EstadoNuevoId
            WHERE r.ProyectoId = @id AND r.Activo = 1 ORDER BY r.Fecha DESC;
            """;
        using var conexion = conexiones.Crear();
        using var r = await conexion.QueryMultipleAsync(new CommandDefinition(sql, new { id }, cancellationToken: ct));
        var detalle = await r.ReadSingleOrDefaultAsync<ProyectoDetalle>();
        if (detalle is null)
            return null;
        detalle.Convocatorias = (await r.ReadAsync<PostulacionItem>()).ToList();
        detalle.Cronograma = (await r.ReadAsync<HitoItem>()).ToList();
        detalle.Avances = (await r.ReadAsync<AvanceItem>()).ToList();
        detalle.Revisiones = (await r.ReadAsync<RevisionItem>()).ToList();
        return detalle;
    }

    /// <summary>Pestaña "Equipo" (HU-09 CA-4).</summary>
    public async Task<IReadOnlyList<IntegranteProyectoItem>> IntegrantesAsync(int proyectoId, CancellationToken ct = default)
    {
        const string sql = """
            SELECT pi.Id, pi.UsuarioId, u.Nombres + ' ' + u.Apellidos AS Nombre, u.Email, pi.RolEnProyecto,
                   CAST(pi.FechaInicio AS datetime2) AS FechaInicio, CAST(pi.FechaFin AS datetime2) AS FechaFin,
                   CAST(CASE WHEN pi.FechaFin IS NULL OR pi.FechaFin > CAST(SYSUTCDATETIME() AS date) THEN 1 ELSE 0 END AS bit) AS Vigente,
                   u.CvLacUrl, u.Orcid
            FROM ProyectoIntegrantes pi JOIN Usuarios u ON u.Id = pi.UsuarioId
            WHERE pi.ProyectoId = @proyectoId AND pi.Activo = 1
            ORDER BY CASE WHEN pi.RolEnProyecto = N'IP' THEN 0 ELSE 1 END, u.Apellidos, u.Nombres
            """;
        using var conexion = conexiones.Crear();
        return (await conexion.QueryAsync<IntegranteProyectoItem>(new CommandDefinition(sql, new { proyectoId }, cancellationToken: ct))).ToList();
    }

    /// <summary>Pestaña "Financiación": fuentes y totales en COP por tipo de fuente y de aporte (HU-10).</summary>
    public async Task<FinanciacionProyecto> FinanciacionAsync(int proyectoId, CancellationToken ct = default)
    {
        const string sql = """
            SELECT f.Id, f.AliadoId, a.Nombre AS Aliado, f.TipoFuente, f.TipoAporte, f.Moneda, f.Monto, f.TasaCambio, f.MontoCop, f.Estado
            FROM FuentesFinanciamiento f LEFT JOIN Aliados a ON a.Id = f.AliadoId
            WHERE f.ProyectoId = @proyectoId AND f.Activo = 1
            ORDER BY f.Id;

            SELECT PresupuestoTotalCop FROM Proyectos WHERE Id = @proyectoId;
            """;
        using var conexion = conexiones.Crear();
        using var r = await conexion.QueryMultipleAsync(new CommandDefinition(sql, new { proyectoId }, cancellationToken: ct));
        var fuentes = (await r.ReadAsync<FuenteItem>()).ToList();
        var presupuesto = await r.ReadSingleOrDefaultAsync<decimal?>();

        var total = fuentes.Sum(f => f.MontoCop ?? 0);
        var porFuente = fuentes.GroupBy(f => f.TipoFuente ?? "Sin tipo")
            .Select(g => new TotalCategoria(g.Key, g.Sum(f => f.MontoCop ?? 0))).OrderBy(t => t.Categoria).ToList();
        var porAporte = fuentes.GroupBy(f => f.TipoAporte ?? "Sin tipo")
            .Select(g => new TotalCategoria(g.Key, g.Sum(f => f.MontoCop ?? 0))).OrderBy(t => t.Categoria).ToList();
        var diferencia = presupuesto is null ? (decimal?)null : total - presupuesto.Value;
        return new FinanciacionProyecto(fuentes, porFuente, porAporte, total, presupuesto, diferencia);
    }

    private static string EscaparLike(string valor) =>
        valor.Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_").Replace("[", @"\[");
}
