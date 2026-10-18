namespace Ci2dt2.Domain;

/// <summary>Roles del sistema (datos semilla con Id fijo).</summary>
public static class Roles
{
    public const string Administrador = "Admin";
    public const string Director = "Director";
    public const string Coordinador = "Coordinador";
    public const string Investigador = "Investigador";
    public const string Externo = "Externo";

    // Roles del catálogo que aún no tienen permisos en el MVP (llegan con sus módulos).
    public const string Estudiante = "Estudiante";
    public const string Docente = "Docente";
    public const string Logistica = "Logistica";
    public const string Secretaria = "Secretaria";

    /// <summary>Roles que se pueden asignar desde la administración de usuarios en el MVP.</summary>
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
    public const string InvestigadorPrincipal = "IP";
}
