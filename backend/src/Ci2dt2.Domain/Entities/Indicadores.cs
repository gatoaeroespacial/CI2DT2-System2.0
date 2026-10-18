namespace Ci2dt2.Domain.Entities;

// M13 Indicadores y reportes
public class Indicador : EntidadAuditable
{
    public int Id { get; set; }

    /// <summary>null = indicador del Centro.</summary>
    public int? ProgramaId { get; set; }
    public string? Codigo { get; set; }
    public string Nombre { get; set; } = "";
    public string? Unidad { get; set; }

    /// <summary>Consulta Dapper que lo calcula.</summary>
    public string? ConsultaClave { get; set; }
    public decimal? UmbralVerdePct { get; set; }
    public decimal? UmbralAmarilloPct { get; set; }
    public string? Periodicidad { get; set; }

    public Programa? Programa { get; set; }
}

public class IndicadorMedicion : EntidadAuditable
{
    public int Id { get; set; }
    public int IndicadorId { get; set; }
    public string? Periodo { get; set; }
    public decimal Meta { get; set; }
    public decimal? ValorActual { get; set; }
    public string? Semaforo { get; set; }
    public DateTime? FechaCalculo { get; set; }

    public Indicador? Indicador { get; set; }
}

public class ReporteGenerado : EntidadAuditable
{
    public int Id { get; set; }
    public int? ProgramaId { get; set; }
    public int GeneradoPorId { get; set; }
    public int? DocumentoId { get; set; }
    public string Tipo { get; set; } = "";
    public string? Periodo { get; set; }
    public string? Formato { get; set; }

    public Programa? Programa { get; set; }
    public Usuario? GeneradoPor { get; set; }
    public Documento? Documento { get; set; }
}
