using Ci2dt2.Application.Common;
using Ci2dt2.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ci2dt2.Application.Seguridad;

public sealed record LoginRequest(string Email, string Password);

public sealed record UsuarioSesion(
    int Id, string Email, string Nombres, string Apellidos,
    IReadOnlyList<string> Roles, IReadOnlyList<int> Programas);

public sealed record SesionEmitida(
    string AccessToken, DateTime AccessTokenExpiraEn,
    string RefreshToken, DateTime RefreshTokenExpiraEn,
    UsuarioSesion Usuario);

public enum ResultadoLogin { Exitoso, CredencialesInvalidas, CuentaInactiva, CuentaBloqueada }

public sealed record LoginResultado(ResultadoLogin Resultado, SesionEmitida? Sesion = null, TimeSpan? TiempoBloqueo = null);

/// <summary>Inicio de sesión, renovación con rotación, cierre de sesión y definición de contraseña.</summary>
public sealed class AuthService(
    IApplicationDbContext db,
    IPasswordHasher hasher,
    ITokenService tokens,
    ControlIntentosLogin intentos,
    Auditoria auditoria,
    TimeProvider reloj)
{
    public static readonly TimeSpan VigenciaRefresco = TimeSpan.FromDays(7);

    // Hash de referencia para igualar el tiempo de respuesta cuando el correo no existe.
    private static string? _hashFicticio;

    public async Task<LoginResultado> LoginAsync(LoginRequest request, string? ip, CancellationToken ct = default)
    {
        var email = NormalizarEmail(request.Email);

        var bloqueo = intentos.TiempoRestanteBloqueo(email);
        if (bloqueo is not null)
        {
            AuditarLogin(email, null, 429, "Bloqueada");
            return new LoginResultado(ResultadoLogin.CuentaBloqueada, TiempoBloqueo: bloqueo);
        }

        var usuario = await db.Usuarios
            .Include(u => u.Roles).ThenInclude(ur => ur.Rol)
            .FirstOrDefaultAsync(u => u.Email == email, ct);

        bool credencialesValidas;
        if (usuario?.PasswordHash is { } hash)
        {
            credencialesValidas = hasher.Verificar(hash, request.Password);
        }
        else
        {
            _hashFicticio ??= hasher.Hash(Guid.NewGuid().ToString());
            hasher.Verificar(_hashFicticio, request.Password);
            credencialesValidas = false;
        }

        if (!credencialesValidas)
        {
            intentos.RegistrarFallo(email);
            AuditarLogin(email, usuario?.Id, 401, "CredencialesInvalidas");
            return new LoginResultado(ResultadoLogin.CredencialesInvalidas);
        }

        if (!usuario!.TieneAcceso)
        {
            AuditarLogin(email, usuario.Id, 403, "CuentaInactiva");
            return new LoginResultado(ResultadoLogin.CuentaInactiva);
        }

        intentos.Reiniciar(email);
        var sesion = await EmitirSesionAsync(usuario, ip, ct);
        auditoria.Registrar("Auth", "LOGIN", usuario.Id.ToString(), 200,
            valoresNuevos: new { email, resultado = "Exitoso" }, usuarioId: usuario.Id);
        await db.SaveChangesAsync(ct);
        return new LoginResultado(ResultadoLogin.Exitoso, sesion);
    }

    /// <summary>
    /// Rotación obligatoria: el token usado se revoca y se emite uno nuevo. Si se presenta
    /// un token ya revocado se revocan todos los tokens activos del usuario.
    /// </summary>
    public async Task<SesionEmitida?> RefrescarAsync(string? refreshToken, string? ip, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return null;

        var hash = Criptografia.Sha256Hex(refreshToken);
        var token = await db.RefreshTokens
            .Include(t => t.Usuario).ThenInclude(u => u!.Roles).ThenInclude(ur => ur.Rol)
            .FirstOrDefaultAsync(t => t.TokenHash == hash, ct);
        if (token is null)
            return null;

        var ahora = Ahora;
        if (token.RevocadoEn is not null)
        {
            await RevocarTodosAsync(token.UsuarioId, ct);
            return null;
        }

        if (token.ExpiraEn <= ahora || !token.Usuario!.TieneAcceso)
            return null;

        // Revocación atómica: si otra petición ya lo rotó, se trata como reutilización.
        var revocados = await db.RefreshTokens
            .Where(t => t.Id == token.Id && t.RevocadoEn == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevocadoEn, ahora), ct);
        if (revocados == 0)
        {
            await RevocarTodosAsync(token.UsuarioId, ct);
            return null;
        }

        var sesion = await EmitirSesionAsync(token.Usuario, ip, ct);
        await db.SaveChangesAsync(ct);
        return sesion;
    }

    public async Task CerrarSesionAsync(string? refreshToken, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return;

        var hash = Criptografia.Sha256Hex(refreshToken);
        var ahora = Ahora;
        await db.RefreshTokens
            .Where(t => t.TokenHash == hash && t.RevocadoEn == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevocadoEn, ahora), ct);
    }

    public Task RevocarTodosAsync(int usuarioId, CancellationToken ct = default)
    {
        var ahora = Ahora;
        return db.RefreshTokens
            .Where(t => t.UsuarioId == usuarioId && t.RevocadoEn == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevocadoEn, ahora), ct);
    }

    public static string NormalizarEmail(string? email) => (email ?? "").Trim().ToLowerInvariant();

    private DateTime Ahora => reloj.GetUtcNow().UtcDateTime;

    private async Task<SesionEmitida> EmitirSesionAsync(Usuario usuario, string? ip, CancellationToken ct)
    {
        var roles = usuario.Roles.Select(r => r.Rol!.Nombre).OrderBy(r => r).ToList();
        var hoy = DateOnly.FromDateTime(Ahora);
        var programas = await db.ProgramaIntegrantes
            .Where(pi => pi.UsuarioId == usuario.Id && pi.Activo
                && (pi.FechaInicio == null || pi.FechaInicio <= hoy)
                && (pi.FechaFin == null || pi.FechaFin > hoy))
            .Select(pi => pi.ProgramaId)
            .Distinct()
            .OrderBy(id => id)
            .ToListAsync(ct);

        var acceso = tokens.CrearTokenAcceso(usuario, roles, programas);

        var refresco = Criptografia.GenerarTokenRefresco();
        var expiraRefresco = Ahora.Add(VigenciaRefresco);
        db.RefreshTokens.Add(new RefreshToken
        {
            UsuarioId = usuario.Id,
            TokenHash = Criptografia.Sha256Hex(refresco),
            ExpiraEn = expiraRefresco,
            IpOrigen = ip is { Length: > 45 } ? ip[..45] : ip,
        });

        return new SesionEmitida(acceso.Token, acceso.ExpiraEn, refresco, expiraRefresco,
            new UsuarioSesion(usuario.Id, usuario.Email, usuario.Nombres, usuario.Apellidos, roles, programas));
    }

    // Evento de autenticación (HU-28 CA-3): IP, fecha UTC y resultado. Se escribe de forma asíncrona.
    private void AuditarLogin(string email, int? usuarioId, int estado, string resultado) =>
        auditoria.Registrar("Auth", "LOGIN", usuarioId?.ToString(), estado,
            valoresNuevos: new { email, resultado }, usuarioId: usuarioId);
}
