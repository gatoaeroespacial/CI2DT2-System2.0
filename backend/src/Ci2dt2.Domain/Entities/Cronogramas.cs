namespace Ci2dt2.Domain.Entities;

// M05 Cronogramas y seguimiento
public class Cronograma : EntidadAuditable
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";

    // Exactamente una de las tres llaves debe tener valor (CHECK).
    public int? ProyectoId { get; set; }
    public int? FormacionId { get; set; }
    public int? EventoId { get; set; }

    public Proyecto? Proyecto { get; set; }
    public Formacion? Formacion { get; set; }
    public Evento? Evento { get; set; }
}

public class TipoHito : EntidadAuditable
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
}

public class Hito : EntidadAuditable
{
    public int Id { get; set; }
    public int CronogramaId { get; set; }
    public int TipoHitoId { get; set; }
    public string Titulo { get; set; } = "";
    public DateOnly FechaLimite { get; set; }
    public int? AlertarDias { get; set; }
    public int? ResponsableId { get; set; }

    /// <summary>Pendiente | En curso | Completado | Vencido</summary>
    public string Estado { get; set; } = "Pendiente";
    public DateOnly? FechaCompletado { get; set; }
    public int? Orden { get; set; }

    public Cronograma? Cronograma { get; set; }
    public TipoHito? TipoHito { get; set; }
    public Usuario? Responsable { get; set; }
}

public class Avance : EntidadAuditable
{
    public int Id { get; set; }
    public int ProyectoId { get; set; }
    public int? HitoId { get; set; }
    public int AutorId { get; set; }
    public string Titulo { get; set; } = "";
    public string? Descripcion { get; set; }
    public decimal? PorcentajeAvance { get; set; }
    public string? OrigenRegistro { get; set; }
    public decimal? Latitud { get; set; }
    public decimal? Longitud { get; set; }
    public decimal? PrecisionGpsM { get; set; }
    public DateTime? FechaCaptura { get; set; }

    /// <summary>Evita duplicados al sincronizar la app móvil (UNIQUE).</summary>
    public Guid? IdCliente { get; set; }
    public string? EstadoValidacion { get; set; }

    public Proyecto? Proyecto { get; set; }
    public Hito? Hito { get; set; }
    public Usuario? Autor { get; set; }
}

public class AvanceAdjunto : EntidadAuditable
{
    public int Id { get; set; }
    public int AvanceId { get; set; }
    public string UrlArchivo { get; set; } = "";
    public string? TipoMime { get; set; }

    public Avance? Avance { get; set; }
}

public class AvanceValidacion : EntidadAuditable
{
    public int Id { get; set; }
    public int AvanceId { get; set; }
    public int ValidadorId { get; set; }
    public string RolValidador { get; set; } = "";
    public string? Periodo { get; set; }
    public string? Resultado { get; set; }
    public decimal? PuntajeRubrica { get; set; }
    public string? Observaciones { get; set; }
    public DateTime Fecha { get; set; }

    public Avance? Avance { get; set; }
    public Usuario? Validador { get; set; }
}
