using Ci2dt2.Application.Common;
using Dapper;

namespace Ci2dt2.Application.Tablero;

public sealed record TarjetasTablero(
    int ProyectosEnEjecucion, int PropuestasEnElaboracion, int PropuestasEnRevision, int PropuestasDevueltas,
    int PropuestasPresentadas, decimal? TasaAprobacion, int PostulacionesConResultado, decimal MontoGestionadoCop);

public sealed record ConteoEstadoPrograma(string Programa, string Estado, int Cantidad);

public sealed record HitoTablero(int HitoId, string Titulo, DateTime FechaLimite, string Estado, int ProyectoId, string? Codigo, string Programa, string? Responsable);

public sealed record AvancesPendientesPrograma(string Programa, int Cantidad);

public sealed record TableroCentro(
    int? Anio,
    TarjetasTablero Tarjetas,
    IReadOnlyList<ConteoEstadoPrograma> ProyectosPorEstadoYPrograma,
    IReadOnlyList<HitoTablero> HitosProximos,
    IReadOnlyList<HitoTablero> HitosVencidos,
    IReadOnlyList<AvancesPendientesPrograma> AvancesPendientes);

/// <summary>
/// HU-24: tablero del Director con lectura directa de los módulos operativos (Dapper). El filtro
/// por año usa la misma regla del listado de proyectos (año de inicio o, si no tiene, de creación)
/// para que las cifras concilien con el listado filtrado (CA-2).
/// </summary>
public sealed class TableroQueries(IDbConnectionFactory conexiones, TimeProvider reloj)
{
    public async Task<TableroCentro> CentroAsync(int? anio, CancellationToken ct = default)
    {
        const string sql = """
            DECLARE @filtroAnio bit = CASE WHEN @anio IS NULL THEN 0 ELSE 1 END;

            SELECT
              SUM(CASE WHEN p.EstadoId = 6 THEN 1 ELSE 0 END) AS ProyectosEnEjecucion,
              SUM(CASE WHEN p.EstadoId = 1 THEN 1 ELSE 0 END) AS PropuestasEnElaboracion,
              SUM(CASE WHEN p.EstadoId = 2 THEN 1 ELSE 0 END) AS PropuestasEnRevision,
              SUM(CASE WHEN p.EstadoId = 3 THEN 1 ELSE 0 END) AS PropuestasDevueltas
            FROM Proyectos p
            WHERE p.Activo = 1 AND (@filtroAnio = 0 OR YEAR(COALESCE(p.FechaInicio, p.CreadoEn)) = @anio);

            SELECT
              COUNT(DISTINCT CASE WHEN pc.FechaPresentacion IS NOT NULL THEN pc.ProyectoId END) AS Presentadas,
              SUM(CASE WHEN pc.Resultado = N'Seleccionada' THEN 1 ELSE 0 END) AS Seleccionadas,
              SUM(CASE WHEN pc.Resultado IN (N'Seleccionada', N'No seleccionada') THEN 1 ELSE 0 END) AS ConResultado,
              COALESCE(SUM(CASE WHEN pc.Resultado = N'Seleccionada' THEN pc.MontoSolicitadoCop END), 0) AS Monto
            FROM PropuestasConvocatoria pc JOIN Proyectos p ON p.Id = pc.ProyectoId AND p.Activo = 1
            WHERE pc.Activo = 1 AND (@filtroAnio = 0 OR YEAR(pc.FechaPresentacion) = @anio);

            SELECT pr.Nombre AS Programa, e.Nombre AS Estado, COUNT(*) AS Cantidad
            FROM Proyectos p JOIN Programas pr ON pr.Id = p.ProgramaId JOIN EstadosProyecto e ON e.Id = p.EstadoId
            WHERE p.Activo = 1 AND (@filtroAnio = 0 OR YEAR(COALESCE(p.FechaInicio, p.CreadoEn)) = @anio)
            GROUP BY pr.Id, pr.Nombre, e.Id, e.Nombre, e.Orden
            ORDER BY pr.Id, e.Orden;

            SELECT h.Id AS HitoId, h.Titulo, CAST(h.FechaLimite AS datetime2) AS FechaLimite, h.Estado, p.Id AS ProyectoId, p.Codigo,
                   pr.Nombre AS Programa, u.Nombres + ' ' + u.Apellidos AS Responsable
            FROM Hitos h JOIN Cronogramas c ON c.Id = h.CronogramaId AND c.Activo = 1
            JOIN Proyectos p ON p.Id = c.ProyectoId AND p.Activo = 1 JOIN Programas pr ON pr.Id = p.ProgramaId
            LEFT JOIN Usuarios u ON u.Id = h.ResponsableId
            WHERE h.Activo = 1 AND h.Estado IN (N'Pendiente', N'En curso') AND h.FechaLimite BETWEEN @hoy AND DATEADD(DAY, 30, @hoy)
            ORDER BY h.FechaLimite;

            SELECT h.Id AS HitoId, h.Titulo, CAST(h.FechaLimite AS datetime2) AS FechaLimite, h.Estado, p.Id AS ProyectoId, p.Codigo,
                   pr.Nombre AS Programa, u.Nombres + ' ' + u.Apellidos AS Responsable
            FROM Hitos h JOIN Cronogramas c ON c.Id = h.CronogramaId AND c.Activo = 1
            JOIN Proyectos p ON p.Id = c.ProyectoId AND p.Activo = 1 JOIN Programas pr ON pr.Id = p.ProgramaId
            LEFT JOIN Usuarios u ON u.Id = h.ResponsableId
            WHERE h.Activo = 1 AND (h.Estado = N'Vencido' OR (h.Estado IN (N'Pendiente', N'En curso') AND h.FechaLimite < @hoy))
            ORDER BY h.FechaLimite;

            SELECT pr.Nombre AS Programa, COUNT(*) AS Cantidad
            FROM Avances a JOIN Proyectos p ON p.Id = a.ProyectoId AND p.Activo = 1 JOIN Programas pr ON pr.Id = p.ProgramaId
            WHERE a.Activo = 1 AND (a.EstadoValidacion = N'Pendiente' OR (a.EstadoValidacion = N'Validado' AND
                  (SELECT TOP 1 v.RolValidador FROM AvanceValidaciones v WHERE v.AvanceId = a.Id AND v.Activo = 1 ORDER BY v.Id DESC) = N'IP'))
            GROUP BY pr.Id, pr.Nombre ORDER BY pr.Id;
            """;
        var hoy = DateOnly.FromDateTime(reloj.GetUtcNow().UtcDateTime.AddHours(-5)).ToDateTime(TimeOnly.MinValue);
        using var conexion = conexiones.Crear();
        using var r = await conexion.QueryMultipleAsync(new CommandDefinition(sql, new { anio, hoy }, cancellationToken: ct));
        var estados = await r.ReadSingleAsync<FilaEstados>();
        var postulaciones = await r.ReadSingleAsync<FilaPostulaciones>();
        var porEstado = (await r.ReadAsync<ConteoEstadoPrograma>()).ToList();
        var proximos = (await r.ReadAsync<HitoTablero>()).ToList();
        var vencidos = (await r.ReadAsync<HitoTablero>()).ToList();
        var avances = (await r.ReadAsync<AvancesPendientesPrograma>()).ToList();

        var tarjetas = new TarjetasTablero(
            estados.ProyectosEnEjecucion ?? 0, estados.PropuestasEnElaboracion ?? 0, estados.PropuestasEnRevision ?? 0,
            estados.PropuestasDevueltas ?? 0, postulaciones.Presentadas,
            TasaAprobacion(postulaciones.Seleccionadas ?? 0, postulaciones.ConResultado ?? 0),
            postulaciones.ConResultado ?? 0, postulaciones.Monto);
        return new TableroCentro(anio, tarjetas, porEstado, proximos, vencidos, avances);
    }

    /// <summary>Tasa de aprobación: seleccionadas / postulaciones con resultado, en porcentaje.</summary>
    public static decimal? TasaAprobacion(int seleccionadas, int conResultado) =>
        conResultado == 0 ? null : Math.Round(seleccionadas * 100m / conResultado, 1);

    private sealed class FilaEstados
    {
        public int? ProyectosEnEjecucion { get; init; }
        public int? PropuestasEnElaboracion { get; init; }
        public int? PropuestasEnRevision { get; init; }
        public int? PropuestasDevueltas { get; init; }
    }

    private sealed class FilaPostulaciones
    {
        public int Presentadas { get; init; }
        public int? Seleccionadas { get; init; }
        public int? ConResultado { get; init; }
        public decimal Monto { get; init; }
    }
}
