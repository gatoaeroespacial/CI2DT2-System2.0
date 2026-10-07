using System.Security.Claims;
using System.Text;
using Ci2dt2.Application.Seguridad;
using Ci2dt2.Domain.Entities;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Ci2dt2.Infrastructure.Seguridad;

public sealed class OpcionesJwt
{
    public const string Seccion = "Jwt";

    /// <summary>Clave HMAC de al menos 32 caracteres; se configura fuera del código.</summary>
    public string Clave { get; set; } = "";
    public string Emisor { get; set; } = "ci2dt2-api";
    public string Audiencia { get; set; } = "ci2dt2-web";
    public int MinutosAcceso { get; set; } = 15;

    public SymmetricSecurityKey ObtenerLlave()
    {
        if (Encoding.UTF8.GetByteCount(Clave) < 32)
            throw new InvalidOperationException("Jwt:Clave debe configurarse con al menos 32 caracteres.");
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Clave));
    }
}

/// <summary>Nombres de los claims del token de acceso.</summary>
public static class ClaimsCi2dt2
{
    public const string Id = JwtRegisteredClaimNames.Sub;
    public const string Email = JwtRegisteredClaimNames.Email;
    public const string Rol = "role";
    public const string Programa = "programa";
}

internal sealed class JwtTokenService(IOptions<OpcionesJwt> opciones, TimeProvider reloj) : ITokenService
{
    private readonly JsonWebTokenHandler _handler = new();

    public TokenAcceso CrearTokenAcceso(Usuario usuario, IReadOnlyCollection<string> roles, IReadOnlyCollection<int> programas)
    {
        var o = opciones.Value;
        var ahora = reloj.GetUtcNow().UtcDateTime;
        var expira = ahora.AddMinutes(o.MinutosAcceso);

        var claims = new List<Claim>
        {
            new(ClaimsCi2dt2.Id, usuario.Id.ToString()),
            new(ClaimsCi2dt2.Email, usuario.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimsCi2dt2.Rol, r)));
        claims.AddRange(programas.Select(p => new Claim(ClaimsCi2dt2.Programa, p.ToString())));

        var token = _handler.CreateToken(new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = o.Emisor,
            Audience = o.Audiencia,
            IssuedAt = ahora,
            NotBefore = ahora,
            Expires = expira,
            SigningCredentials = new SigningCredentials(o.ObtenerLlave(), SecurityAlgorithms.HmacSha256),
        });
        return new TokenAcceso(token, expira);
    }
}

/// <summary>Hash adaptativo PBKDF2 de ASP.NET Identity.</summary>
internal sealed class IdentityPasswordHasher : Application.Seguridad.IPasswordHasher
{
    private static readonly object Usuario = new();
    private readonly PasswordHasher<object> _hasher = new();

    public string Hash(string password) => _hasher.HashPassword(Usuario, password);

    public bool Verificar(string hash, string password) =>
        _hasher.VerifyHashedPassword(Usuario, hash, password) != PasswordVerificationResult.Failed;
}

/// <summary>Enlaces firmados con la Data Protection API (propósito + sello del PasswordHash).</summary>
internal sealed class EnlaceContrasenaService(IDataProtectionProvider proveedor) : IEnlaceContrasenaService
{
    private readonly ITimeLimitedDataProtector _protector =
        proveedor.CreateProtector("Ci2dt2.Contrasena.Reset").ToTimeLimitedDataProtector();

    public string Generar(int usuarioId, string sello, TimeSpan vigencia) =>
        _protector.Protect($"{usuarioId}:{sello}", vigencia);

    public (int UsuarioId, string Sello)? Validar(string token)
    {
        try
        {
            var partes = _protector.Unprotect(token).Split(':');
            return partes.Length == 2 && int.TryParse(partes[0], out var id) ? (id, partes[1]) : null;
        }
        catch (System.Security.Cryptography.CryptographicException)
        {
            return null;
        }
    }
}
