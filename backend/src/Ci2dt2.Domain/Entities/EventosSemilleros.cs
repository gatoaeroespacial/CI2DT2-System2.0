namespace Ci2dt2.Domain.Entities;

// M09 Eventos
public class TipoEvento : EntidadAuditable
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public int? DuracionMinutosDefecto { get; set; }
}

public class Evento : EntidadAuditable
{
    public int Id { get; set; }

    /// <summary>null = evento del Centro.</summary>
    public int? ProgramaId { get; set; }
    public int TipoEventoId { get; set; }
    public int? ModalidadId { get; set; }
    public int? ResponsableLogisticaId { get; set; }
    public string Nombre { get; set; } = "";
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public string? Lugar { get; set; }
    public int? CupoMaximo { get; set; }
    public string? Agenda { get; set; }
    public string Estado { get; set; } = "";

    public Programa? Programa { get; set; }
    public TipoEvento? TipoEvento { get; set; }
    public Modalidad? Modalidad { get; set; }
    public Usuario? ResponsableLogistica { get; set; }
}

public class EventoAsistente : EntidadAuditable
{
    public int Id { get; set; }
    public int EventoId { get; set; }
    public int UsuarioId { get; set; }
    public int? AliadoId { get; set; }
    public long? ConsentimientoId { get; set; }
    public string? Cargo { get; set; }

    /// <summary>Inscrito | Asistió | No asistió</summary>
    public string Estado { get; set; } = "";
    public DateTime? HoraRegistro { get; set; }
    public string? Retroalimentacion { get; set; }

    public Evento? Evento { get; set; }
    public Usuario? Usuario { get; set; }
    public Aliado? Aliado { get; set; }
    public ConsentimientoDatos? Consentimiento { get; set; }
}

// M10 Semilleros
public class Semillero : EntidadAuditable
{
    public int Id { get; set; }
    public int ProgramaId { get; set; }
    public int? CoordinadorId { get; set; }
    public string Nombre { get; set; } = "";

    public Programa? Programa { get; set; }
    public Usuario? Coordinador { get; set; }
}

public class TipoIntegranteSemillero : EntidadAuditable
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
}

public class SemilleroIntegrante : EntidadAuditable
{
    public int Id { get; set; }
    public int SemilleroId { get; set; }
    public int UsuarioId { get; set; }
    public int TipoIntegranteId { get; set; }
    public int? ProyectoId { get; set; }
    public string? EstadoAprobacion { get; set; }
    public int? AprobadoPorId { get; set; }
    public DateOnly? FechaIngreso { get; set; }
    public DateOnly? FechaSalida { get; set; }

    public Semillero? Semillero { get; set; }
    public Usuario? Usuario { get; set; }
    public TipoIntegranteSemillero? TipoIntegrante { get; set; }
    public Proyecto? Proyecto { get; set; }
    public Usuario? AprobadoPor { get; set; }
}
