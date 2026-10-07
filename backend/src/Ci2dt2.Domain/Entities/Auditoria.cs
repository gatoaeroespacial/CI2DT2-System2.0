namespace Ci2dt2.Domain.Entities;

// M15 Auditoría (solo LogActividades en el MVP)
public class LogActividad
{
    public long Id { get; set; }
    public int? UsuarioId { get; set; }
    public string? IpOrigen { get; set; }
    public string Recurso { get; set; } = "";
    public string? RecursoId { get; set; }
    public string Accion { get; set; } = "";
    public string? Ruta { get; set; }
    public int? EstadoHttp { get; set; }
    public string? ValoresAnteriores { get; set; }
    public string? ValoresNuevos { get; set; }
    public DateTime FechaUtc { get; set; }
    // Nulo en el MVP; la cadena de integridad se activa en Release 2.
    public string? HashEncadenado { get; set; }

    public Usuario? Usuario { get; set; }
}
