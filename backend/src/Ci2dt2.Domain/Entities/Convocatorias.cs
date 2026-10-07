namespace Ci2dt2.Domain.Entities;

// M04 Convocatorias y Financiamiento
public class Convocatoria
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string? Numero { get; set; }
    public int TipoConvocatoriaId { get; set; }
    public int? EntidadConvocanteId { get; set; }
    public string? Plataforma { get; set; }
    public string? Url { get; set; }
    public DateOnly? FechaApertura { get; set; }
    public DateOnly? FechaCierre { get; set; }
    public string? Moneda { get; set; }
    public decimal? MontoMaximo { get; set; }

    public TipoConvocatoria? TipoConvocatoria { get; set; }
    public Aliado? EntidadConvocante { get; set; }
}

public class PropuestaConvocatoria
{
    public int Id { get; set; }
    public int ProyectoId { get; set; }
    public int ConvocatoriaId { get; set; }
    public DateOnly? FechaPresentacion { get; set; }
    public string? IdPostulacionExterna { get; set; }
    public string? CodigoMga { get; set; }
    public decimal? MontoSolicitado { get; set; }
    public string? Moneda { get; set; }
    public decimal? TasaCambioEstimada { get; set; }
    public decimal? MontoSolicitadoCop { get; set; }
    public string? Resultado { get; set; }
    public decimal? Puntaje { get; set; }
    public string? FaseExterna { get; set; }
    public DateOnly? FechaResultado { get; set; }
    public int? ComprobanteDocId { get; set; }

    public Proyecto? Proyecto { get; set; }
    public Convocatoria? Convocatoria { get; set; }
    public Documento? ComprobanteDoc { get; set; }
}

public class FuenteFinanciamiento
{
    public int Id { get; set; }
    public int ProyectoId { get; set; }
    public int? AliadoId { get; set; }
    public string TipoFuente { get; set; } = "";
    public string TipoAporte { get; set; } = "";
    public string Moneda { get; set; } = "COP";
    public decimal Monto { get; set; }
    public decimal? TasaCambio { get; set; }
    public decimal? MontoCop { get; set; }
    public string? Estado { get; set; }

    public Proyecto? Proyecto { get; set; }
    public Aliado? Aliado { get; set; }
}
