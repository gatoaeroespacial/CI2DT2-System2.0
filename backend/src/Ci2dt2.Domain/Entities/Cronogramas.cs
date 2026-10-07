namespace Ci2dt2.Domain.Entities;

// M05 Cronogramas y Seguimiento
public class Cronograma
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public int? ProyectoId { get; set; }
    // FormacionId y EventoId quedan nulos en el MVP (M08 y M09 llegan en Release 2).
    public int? FormacionId { get; set; }
    public int? EventoId { get; set; }

    public Proyecto? Proyecto { get; set; }
}

public class TipoHito
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
}

public class Hito
{
    public int Id { get; set; }
    public int CronogramaId { get; set; }
    public int TipoHitoId { get; set; }
    public string Titulo { get; set; } = "";
    public DateOnly FechaLimite { get; set; }
    public int AlertarDias { get; set; } = 7;
    public int? ResponsableId { get; set; }
    public string Estado { get; set; } = "Pendiente";
    public DateOnly? FechaCompletado { get; set; }
    public int Orden { get; set; }

    public Cronograma? Cronograma { get; set; }
    public TipoHito? TipoHito { get; set; }
    public Usuario? Responsable { get; set; }
}

public class Avance
{
    public int Id { get; set; }
    public int ProyectoId { get; set; }
    public int? HitoId { get; set; }
    public int AutorId { get; set; }
    public string Titulo { get; set; } = "";
    public string? Descripcion { get; set; }
    public decimal? PorcentajeAvance { get; set; }
    public string OrigenRegistro { get; set; } = "Web";
    public decimal? Latitud { get; set; }
    public decimal? Longitud { get; set; }
    public decimal? PrecisionGpsM { get; set; }
    public DateTime FechaCaptura { get; set; }
    public Guid? IdCliente { get; set; }
    public string? EstadoValidacion { get; set; }

    public Proyecto? Proyecto { get; set; }
    public Hito? Hito { get; set; }
    public Usuario? Autor { get; set; }
}

public class AvanceAdjunto
{
    public int Id { get; set; }
    public int AvanceId { get; set; }
    public string UrlArchivo { get; set; } = "";
    public string? TipoMime { get; set; }

    public Avance? Avance { get; set; }
}

public class AvanceValidacion
{
    public int Id { get; set; }
    public int AvanceId { get; set; }
    public int ValidadorId { get; set; }
    public string? RolValidador { get; set; }
    public string? Periodo { get; set; }
    public string Resultado { get; set; } = "";
    public decimal? PuntajeRubrica { get; set; }
    public string? Observaciones { get; set; }
    public DateTime Fecha { get; set; }

    public Avance? Avance { get; set; }
    public Usuario? Validador { get; set; }
}
