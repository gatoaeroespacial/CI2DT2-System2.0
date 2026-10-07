using Ci2dt2.Domain.Entities;

namespace Ci2dt2.Application.Seguridad;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verificar(string hash, string password);
}

public sealed record TokenAcceso(string Token, DateTime ExpiraEn);

public interface ITokenService
{
    /// <summary>JWT de 15 minutos con id, correo, roles y programas del usuario.</summary>
    TokenAcceso CrearTokenAcceso(Usuario usuario, IReadOnlyCollection<string> roles, IReadOnlyCollection<int> programas);
}

/// <summary>Enlaces firmados para definir o restablecer la contraseña.</summary>
public interface IEnlaceContrasenaService
{
    string Generar(int usuarioId, string sello, TimeSpan vigencia);

    /// <summary>Devuelve null si el token es inválido o expiró.</summary>
    (int UsuarioId, string Sello)? Validar(string token);
}

public interface IEmailSender
{
    Task EnviarAsync(string para, string asunto, string cuerpoHtml, CancellationToken ct = default);
}

public sealed class OpcionesFrontend
{
    public const string Seccion = "Frontend";

    /// <summary>URL pública del frontend, usada en los enlaces de los correos.</summary>
    public string Url { get; set; } = "http://localhost:3000";
}
