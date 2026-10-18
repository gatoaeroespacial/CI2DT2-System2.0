namespace Ci2dt2.Domain.Entities;

/// <summary>
/// Columnas de auditoría comunes (regla general 4 del diccionario de datos). Las tablas
/// inmutables (LogActividades, DocumentoFirmas y ConsentimientosDatos) no heredan de esta clase.
/// </summary>
public abstract class EntidadAuditable
{
    public DateTime CreadoEn { get; set; }
    public int? CreadoPorId { get; set; }
    public DateTime? ActualizadoEn { get; set; }

    /// <summary>Borrado lógico.</summary>
    public bool Activo { get; set; } = true;
}
