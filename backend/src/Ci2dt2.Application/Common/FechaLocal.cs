namespace Ci2dt2.Application;

/// <summary>
/// Fecha de calendario en Colombia (UTC−5, sin horario de verano). Las fechas sin hora (hoy, fecha
/// de completado, vigencias) se calculan en hora local para que una acción a las 23:00 no quede con
/// la fecha del día siguiente.
/// </summary>
public static class FechaLocal
{
    public static DateOnly Hoy(TimeProvider reloj) => DateOnly.FromDateTime(reloj.GetUtcNow().UtcDateTime.AddHours(-5));
}
