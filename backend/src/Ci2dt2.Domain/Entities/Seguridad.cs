namespace Ci2dt2.Domain.Entities;

// M01 Seguridad y acceso
public class Rol : EntidadAuditable
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string? Descripcion { get; set; }
}

public class Usuario : EntidadAuditable
{
    public int Id { get; set; }
    public string Email { get; set; } = "";

    /// <summary>Solo puede ser null si TieneAcceso = 0.</summary>
    public string? PasswordHash { get; set; }
    public string Nombres { get; set; } = "";
    public string Apellidos { get; set; } = "";
    public string? TipoDocumento { get; set; }
    public string? NumeroDocumento { get; set; }
    public string? Telefono { get; set; }
    public string? Cargo { get; set; }
    public string? Dependencia { get; set; }
    public int? AliadoId { get; set; }
    public string? CvLacUrl { get; set; }
    public string? Orcid { get; set; }
    public string? ExpoPushToken { get; set; }

    /// <summary>0 = persona registrada sin login (jurado, coinventor, aspirante o pendiente de definir contraseña).</summary>
    public bool TieneAcceso { get; set; }

    public Aliado? Aliado { get; set; }
    public List<UsuarioRol> Roles { get; set; } = [];
}

public class UsuarioRol : EntidadAuditable
{
    public int UsuarioId { get; set; }
    public int RolId { get; set; }

    public Usuario? Usuario { get; set; }
    public Rol? Rol { get; set; }
}

public class RefreshToken : EntidadAuditable
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string TokenHash { get; set; } = "";
    public DateTime ExpiraEn { get; set; }
    public DateTime? RevocadoEn { get; set; }
    public string? IpOrigen { get; set; }

    public Usuario? Usuario { get; set; }
}
