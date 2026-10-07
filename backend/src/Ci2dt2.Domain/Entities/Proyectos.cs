namespace Ci2dt2.Domain.Entities;

// M03 Proyectos
public class EstadoProyecto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public int Orden { get; set; }
    public bool EsFinal { get; set; }
}

public class TipoProyecto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public bool RequiereHitosFijos { get; set; }
}

public class TipoConvocatoria
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
}

public class Proyecto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = "";
    public string Titulo { get; set; } = "";
    public string? Resumen { get; set; }
    public int ProgramaId { get; set; }
    public int TipoProyectoId { get; set; }
    public int EstadoId { get; set; }
    public int InvestigadorPrincipalId { get; set; }
    public DateOnly? FechaInicio { get; set; }
    public DateOnly? FechaFin { get; set; }
    public decimal PorcentajeAvance { get; set; }
    public string Moneda { get; set; } = "COP";
    public decimal? PresupuestoTotal { get; set; }
    public decimal? TasaCambio { get; set; }
    public decimal? PresupuestoTotalCop { get; set; }

    public Programa? Programa { get; set; }
    public TipoProyecto? TipoProyecto { get; set; }
    public EstadoProyecto? Estado { get; set; }
    public Usuario? InvestigadorPrincipal { get; set; }
}

public class ProyectoIntegrante
{
    public int Id { get; set; }
    public int ProyectoId { get; set; }
    public int UsuarioId { get; set; }
    public string RolEnProyecto { get; set; } = "";
    public DateOnly? FechaInicio { get; set; }
    public DateOnly? FechaFin { get; set; }

    public Proyecto? Proyecto { get; set; }
    public Usuario? Usuario { get; set; }
}

public class ProyectoRevision
{
    public int Id { get; set; }
    public int ProyectoId { get; set; }
    public int RevisorId { get; set; }
    public string Etapa { get; set; } = "";
    public string Decision { get; set; } = "";
    public int? EstadoAnteriorId { get; set; }
    public int? EstadoNuevoId { get; set; }
    public string? Observaciones { get; set; }
    public DateTime Fecha { get; set; }

    public Proyecto? Proyecto { get; set; }
    public Usuario? Revisor { get; set; }
    public EstadoProyecto? EstadoAnterior { get; set; }
    public EstadoProyecto? EstadoNuevo { get; set; }
}
