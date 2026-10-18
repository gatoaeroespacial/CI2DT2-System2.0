namespace Ci2dt2.Domain.Entities;

// M14 Visitas y datos personales
public class PoliticaTratamientoDatos : EntidadAuditable
{
    public int Id { get; set; }
    public string Version { get; set; } = "";
    public string TextoUrl { get; set; } = "";
    public string? HashTexto { get; set; }
    public DateOnly VigenteDesde { get; set; }
    public DateOnly? VigenteHasta { get; set; }
}

/// <summary>Inmutable: sin columnas de auditoría ni UPDATE/DELETE.</summary>
public class ConsentimientoDatos
{
    public long Id { get; set; }
    public int PoliticaId { get; set; }
    public int? UsuarioId { get; set; }
    public string? Correo { get; set; }

    /// <summary>Visita | Inscripción | Evento | Registro | Contacto</summary>
    public string Contexto { get; set; } = "";
    public bool Aceptado { get; set; }
    public DateTime FechaAceptacion { get; set; }
    public string? IpOrigen { get; set; }
    public string? HashEvidencia { get; set; }

    public PoliticaTratamientoDatos? Politica { get; set; }
    public Usuario? Usuario { get; set; }
}

public class Visita : EntidadAuditable
{
    public int Id { get; set; }
    public long ConsentimientoId { get; set; }
    public int? UsuarioId { get; set; }

    /// <summary>Secretaria; null = auto-registro.</summary>
    public int? RegistradaPorId { get; set; }
    public string Identificacion { get; set; } = "";
    public string NombreCompleto { get; set; } = "";
    public string? Correo { get; set; }
    public string? Celular { get; set; }
    public string? Dependencia { get; set; }
    public string? Cargo { get; set; }
    public string MotivoVisita { get; set; } = "";
    public DateTime FechaHoraEntrada { get; set; }
    public DateTime? FechaHoraSalida { get; set; }
    public int? MinutosPermanencia { get; set; }

    public ConsentimientoDatos? Consentimiento { get; set; }
    public Usuario? Usuario { get; set; }
    public Usuario? RegistradaPor { get; set; }
}
