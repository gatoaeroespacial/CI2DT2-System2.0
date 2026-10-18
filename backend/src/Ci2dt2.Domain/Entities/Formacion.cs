namespace Ci2dt2.Domain.Entities;

// M08 Educación continua
public class TipoFormacion : EntidadAuditable
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
}

public class Modalidad : EntidadAuditable
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
}

public class Formacion : EntidadAuditable
{
    public int Id { get; set; }
    public int ProgramaId { get; set; }
    public int TipoFormacionId { get; set; }
    public int ModalidadId { get; set; }
    public string Nombre { get; set; } = "";
    public int? IntensidadHoras { get; set; }
    public decimal? ValorMatricula { get; set; }
    public int? CupoMaximo { get; set; }

    /// <summary>Mínimo de matriculados.</summary>
    public int? PuntoEquilibrio { get; set; }
    public decimal? MetaFinancieraCop { get; set; }
    public DateOnly? FechaInicioInscripcion { get; set; }
    public DateOnly? FechaFinInscripcion { get; set; }
    public DateOnly? FechaInicio { get; set; }
    public DateOnly? FechaFin { get; set; }
    public decimal? AsistenciaMinimaPct { get; set; }
    public decimal? NotaMinima { get; set; }

    /// <summary>Parametrizada | Inscripciones | Evaluación viabilidad | Autorizada | Cancelada | En ejecución | Finalizada</summary>
    public string Estado { get; set; } = "";
    public int? AutorizadaPorId { get; set; }
    public DateOnly? FechaAutorizacion { get; set; }

    public Programa? Programa { get; set; }
    public TipoFormacion? TipoFormacion { get; set; }
    public Modalidad? Modalidad { get; set; }
    public Usuario? AutorizadaPor { get; set; }
}

public class FormacionDocente : EntidadAuditable
{
    public int Id { get; set; }
    public int FormacionId { get; set; }
    public int DocenteId { get; set; }

    public Formacion? Formacion { get; set; }
    public Usuario? Docente { get; set; }
}

public class Inscripcion : EntidadAuditable
{
    public int Id { get; set; }
    public int FormacionId { get; set; }
    public int UsuarioId { get; set; }
    public long? ConsentimientoId { get; set; }

    /// <summary>Prematriculado | Matriculado | Retirado | Aprobado | Reprobado</summary>
    public string Estado { get; set; } = "";
    public decimal? ValorPagado { get; set; }
    public int? SoportePagoDocId { get; set; }
    public decimal? PorcentajeAsistencia { get; set; }
    public decimal? NotaFinal { get; set; }
    public bool HabilitadoCertificacion { get; set; }

    public Formacion? Formacion { get; set; }
    public Usuario? Usuario { get; set; }
    public ConsentimientoDatos? Consentimiento { get; set; }
    public Documento? SoportePagoDoc { get; set; }
}

public class FormacionSesion : EntidadAuditable
{
    public int Id { get; set; }
    public int FormacionId { get; set; }
    public int? DocenteId { get; set; }
    public DateOnly Fecha { get; set; }
    public TimeOnly? HoraInicio { get; set; }
    public TimeOnly? HoraFin { get; set; }
    public string? Tema { get; set; }

    public Formacion? Formacion { get; set; }
    public Usuario? Docente { get; set; }
}

public class Asistencia : EntidadAuditable
{
    public int Id { get; set; }
    public int SesionId { get; set; }
    public int InscripcionId { get; set; }
    public bool Presente { get; set; }

    public FormacionSesion? Sesion { get; set; }
    public Inscripcion? Inscripcion { get; set; }
}

public class Calificacion : EntidadAuditable
{
    public int Id { get; set; }
    public int InscripcionId { get; set; }
    public string Actividad { get; set; } = "";
    public decimal Nota { get; set; }
    public decimal? PesoPct { get; set; }
    public DateOnly Fecha { get; set; }

    public Inscripcion? Inscripcion { get; set; }
}

public class Certificado : EntidadAuditable
{
    public int Id { get; set; }

    /// <summary>Un certificado por inscripción (UNIQUE).</summary>
    public int InscripcionId { get; set; }
    public string? CodigoVerificacion { get; set; }
    public string? HashCertificado { get; set; }
    public string? TxHashBlockchain { get; set; }

    /// <summary>Pendiente | Enviado a SparLum | Emitido | Error</summary>
    public string Estado { get; set; } = "";
    public DateTime? FechaEmision { get; set; }
    public string? UrlVerificacion { get; set; }
    public int? FirmaDirectorId { get; set; }
    public int? FirmaCoordinadorId { get; set; }
    public bool EnviadoEmail { get; set; }

    public Inscripcion? Inscripcion { get; set; }
    public Usuario? FirmaDirector { get; set; }
    public Usuario? FirmaCoordinador { get; set; }
}
