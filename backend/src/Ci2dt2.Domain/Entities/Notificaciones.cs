namespace Ci2dt2.Domain.Entities;

// M07 Notificaciones
public class Notificacion : EntidadAuditable
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string Tipo { get; set; } = "";
    public string Titulo { get; set; } = "";
    public string Mensaje { get; set; } = "";

    // Referencia polimórfica sin llave foránea.
    public string? ReferenciaTipo { get; set; }
    public int? ReferenciaId { get; set; }
    public bool Leida { get; set; }
    public bool EnviadaPush { get; set; }
    public DateTime? FechaEnvioPush { get; set; }
    public bool EnviadaEmail { get; set; }
    public DateTime? FechaEnvioEmail { get; set; }
    public int? Intentos { get; set; }
    public string? UltimoError { get; set; }

    public Usuario? Usuario { get; set; }
}

public class PreferenciaNotificacion : EntidadAuditable
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string Tipo { get; set; } = "";
    public bool RecibirEmail { get; set; }
    public bool RecibirPush { get; set; }

    public Usuario? Usuario { get; set; }
}
