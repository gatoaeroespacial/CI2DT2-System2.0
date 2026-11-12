using Ci2dt2.Application.Common;
using Ci2dt2.Application.Notificaciones;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Ci2dt2.Application.Seguridad;

public sealed record SolicitarRestablecimientoRequest(string Email);

public sealed record RestablecerContrasenaRequest(string Token, string Contrasena);

public sealed class SolicitarRestablecimientoRequestValidator : AbstractValidator<SolicitarRestablecimientoRequest>
{
    public SolicitarRestablecimientoRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().WithMessage("El correo es obligatorio.")
            .EmailAddress().WithMessage("El correo no tiene un formato válido.");
    }
}

public sealed class RestablecerContrasenaRequestValidator : AbstractValidator<RestablecerContrasenaRequest>
{
    public RestablecerContrasenaRequestValidator()
    {
        RuleFor(x => x.Token).NotEmpty().WithMessage("El enlace no es válido.");
        RuleFor(x => x.Contrasena).CumplePoliticaContrasena();
    }
}

/// <summary>
/// HU-03: restablecer una contraseña olvidada con un enlace enviado al correo. El enlace dura
/// 30 minutos, es de un solo uso y la respuesta es idéntica exista o no el correo.
/// </summary>
public sealed class RestablecimientoContrasenaService(
    IApplicationDbContext db,
    IPasswordHasher hasher,
    ITokenRestablecimiento tokens,
    IColaCorreos correos,
    AuthService auth,
    Auditoria auditoria,
    IOptions<OpcionesFrontend> frontend)
{
    public static readonly TimeSpan Vigencia = TimeSpan.FromMinutes(30);
    public const string MensajeSolicitud = "Si el correo está registrado, recibirá un enlace para restablecer la contraseña.";
    public const string MensajeEnlaceInvalido = "El enlace para restablecer la contraseña expiró o ya fue utilizado. Solicite uno nuevo.";

    public async Task SolicitarAsync(string email, CancellationToken ct = default)
    {
        var normalizado = AuthService.NormalizarEmail(email);
        var usuario = await db.Usuarios.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == normalizado && u.TieneAcceso && u.Activo, ct);
        if (usuario?.PasswordHash is not { } hash)
            return;

        var token = tokens.Generar(usuario.Id, hash, Vigencia);
        var enlace = $"{frontend.Value.Url.TrimEnd('/')}/restablecer-contrasena?token={Uri.EscapeDataString(token)}";
        // El correo sale por una cola en segundo plano: la respuesta no revela si el correo existe.
        correos.Encolar(usuario.Email, "CI²DT² · Restablecer contraseña", PlantillaCorreo.Html(
            "Restablecer contraseña",
            $"Hola {usuario.Nombres}: recibimos una solicitud para restablecer su contraseña. El enlace es válido durante 30 minutos y solo puede usarse una vez. Si usted no la hizo, ignore este mensaje.",
            enlace, "Restablecer contraseña"));
        auditoria.Registrar("Auth", "RESET-REQ", usuario.Id.ToString(), 200, usuarioId: usuario.Id);
    }

    /// <summary>CA-3: indica si el enlace sigue vigente y sin usar, para avisarlo al abrirlo.</summary>
    public async Task<bool> EsValidoAsync(string token, CancellationToken ct = default) =>
        await UsuarioDelTokenAsync(token, ct) is not null;

    public async Task RestablecerAsync(string token, string contrasena, CancellationToken ct = default)
    {
        var usuario = await UsuarioDelTokenAsync(token, ct) ?? throw new ReglaNegocioException(MensajeEnlaceInvalido);
        usuario.PasswordHash = hasher.Hash(contrasena);
        await db.SaveChangesAsync(ct);
        // CA-2: se revocan todas las sesiones activas del usuario.
        await auth.RevocarTodosAsync(usuario.Id, ct);
    }

    private async Task<Domain.Entities.Usuario?> UsuarioDelTokenAsync(string token, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(token) || tokens.Leer(token) is not { } datos)
            return null;
        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == datos.UsuarioId && u.TieneAcceso && u.Activo, ct);
        return usuario?.PasswordHash is { } hash && tokens.Sello(hash) == datos.Sello ? usuario : null;
    }
}
