using System.Security.Cryptography;
using System.Text;

namespace Ci2dt2.Application.Seguridad;

public static class Criptografia
{
    /// <summary>Valor aleatorio de 64 bytes en Base64Url para el token de refresco.</summary>
    public static string GenerarTokenRefresco()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    public static string Sha256Hex(string valor) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(valor)));

    /// <summary>
    /// Sello del PasswordHash actual: invalida los enlaces de contraseña una vez que
    /// la contraseña cambia (enlace de un solo uso).
    /// </summary>
    public static string SelloContrasena(string? passwordHash) => Sha256Hex(passwordHash ?? "")[..16];
}
