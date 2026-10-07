namespace Ci2dt2.Domain;

/// <summary>Roles del sistema (datos semilla con Id fijo).</summary>
public static class Roles
{
    public const string Administrador = "Administrador";
    public const string Director = "Director";
    public const string Coordinador = "Coordinador";
    public const string Investigador = "Investigador";
    public const string Externo = "Externo";

    public static readonly string[] Todos = [Administrador, Director, Coordinador, Investigador, Externo];
}

/// <summary>Valores de ProgramaIntegrantes.RolEnPrograma.</summary>
public static class RolesPrograma
{
    public const string Coordinador = "Coordinador";
    public const string Investigador = "Investigador";
}

/// <summary>Valores de ProyectoIntegrantes.RolEnProyecto.</summary>
public static class RolesProyecto
{
    public const string InvestigadorPrincipal = "Investigador principal";
}
