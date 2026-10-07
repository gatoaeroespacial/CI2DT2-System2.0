namespace Ci2dt2.Application.Usuarios;

public sealed record CrearUsuarioRequest(
    string Nombres,
    string Apellidos,
    string? TipoDocumento,
    string? NumeroDocumento,
    string Email,
    string? Telefono,
    string? Cargo,
    string? Dependencia,
    string? CvLacUrl,
    string? Orcid,
    int? AliadoId,
    IReadOnlyList<string> Roles) : IDatosUsuario;

public sealed record ActualizarUsuarioRequest(
    string Nombres,
    string Apellidos,
    string? TipoDocumento,
    string? NumeroDocumento,
    string Email,
    string? Telefono,
    string? Cargo,
    string? Dependencia,
    string? CvLacUrl,
    string? Orcid,
    int? AliadoId) : IDatosUsuario;

public sealed record CambiarEstadoUsuarioRequest(bool? TieneAcceso);

public sealed record AsignarRolesRequest(IReadOnlyList<string> Roles);

/// <summary>Campos comunes de los formularios de creación y edición.</summary>
public interface IDatosUsuario
{
    string Nombres { get; }
    string Apellidos { get; }
    string? TipoDocumento { get; }
    string? NumeroDocumento { get; }
    string Email { get; }
    string? Telefono { get; }
    string? Cargo { get; }
    string? Dependencia { get; }
    string? CvLacUrl { get; }
    string? Orcid { get; }
    int? AliadoId { get; }
}

public sealed record UsuarioDetalle(
    int Id,
    string Nombres,
    string Apellidos,
    string? TipoDocumento,
    string? NumeroDocumento,
    string Email,
    string? Telefono,
    string? Cargo,
    string? Dependencia,
    string? CvLacUrl,
    string? Orcid,
    int? AliadoId,
    bool TieneAcceso,
    IReadOnlyList<string> Roles);

public sealed class UsuarioListadoItem
{
    public int Id { get; init; }
    public string Nombres { get; init; } = "";
    public string Apellidos { get; init; } = "";
    public string Email { get; init; } = "";
    public string? TipoDocumento { get; init; }
    public string? NumeroDocumento { get; init; }
    public bool TieneAcceso { get; init; }
    public IReadOnlyList<string> Roles { get; init; } = [];
}

public sealed record FiltroUsuarios(string? Busqueda, string? Rol, bool? TieneAcceso, int Pagina = 1);
