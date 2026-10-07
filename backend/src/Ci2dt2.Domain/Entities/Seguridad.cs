namespace Ci2dt2.Domain.Entities;

// M01 Seguridad
public class Rol
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string? Descripcion { get; set; }
}

public class Usuario
{
    public int Id { get; set; }
    public string Email { get; set; } = "";
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
    public bool TieneAcceso { get; set; } = true;

    public Aliado? Aliado { get; set; }
    public List<UsuarioRol> Roles { get; set; } = [];
}

public class UsuarioRol
{
    public int UsuarioId { get; set; }
    public int RolId { get; set; }

    public Usuario? Usuario { get; set; }
    public Rol? Rol { get; set; }
}

public class RefreshToken
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string TokenHash { get; set; } = "";
    public DateTime ExpiraEn { get; set; }
    public DateTime? RevocadoEn { get; set; }
    public string? IpOrigen { get; set; }

    public Usuario? Usuario { get; set; }
}
