using System.Net;
using Ci2dt2.Application.Common;
using Ci2dt2.Application.Seguridad;
using Ci2dt2.Domain;
using Ci2dt2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ci2dt2.Application.Usuarios;

/// <summary>Comandos de administración de usuarios y roles (HU-04).</summary>
public sealed class UsuariosService(
    IApplicationDbContext db,
    AuthService auth,
    IEnlaceContrasenaService enlaces,
    IEmailSender email,
    IOptions<OpcionesFrontend> frontend,
    Auditoria auditoria,
    ILogger<UsuariosService> logger)
{
    public const string Recurso = "Usuarios";
    public static readonly TimeSpan VigenciaEnlaceBienvenida = TimeSpan.FromHours(72);

    public async Task<UsuarioDetalle> ObtenerAsync(int id, CancellationToken ct = default)
    {
        var usuario = await CargarAsync(id, ct);
        return ADetalle(usuario);
    }

    public async Task<UsuarioDetalle> CrearAsync(CrearUsuarioRequest request, CancellationToken ct = default)
    {
        var emailNormalizado = AuthService.NormalizarEmail(request.Email);
        await ValidarEmailUnicoAsync(emailNormalizado, null, ct);
        await ValidarAliadoAsync(request.AliadoId, ct);

        var roles = await db.Roles.Where(r => request.Roles.Contains(r.Nombre)).ToListAsync(ct);
        var usuario = new Usuario { TieneAcceso = true };
        Aplicar(usuario, request, emailNormalizado);
        usuario.Roles = roles.Select(r => new UsuarioRol { Rol = r }).ToList();

        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync(ct);

        auditoria.Registrar(Recurso, "POST", usuario.Id.ToString(), 201, valoresNuevos: Instantanea(usuario));
        await db.SaveChangesAsync(ct);

        await EnviarBienvenidaAsync(usuario, ct);
        return ADetalle(usuario);
    }

    public async Task<UsuarioDetalle> ActualizarAsync(int id, ActualizarUsuarioRequest request, CancellationToken ct = default)
    {
        var usuario = await CargarAsync(id, ct);
        var emailNormalizado = AuthService.NormalizarEmail(request.Email);
        await ValidarEmailUnicoAsync(emailNormalizado, id, ct);
        await ValidarAliadoAsync(request.AliadoId, ct);
        if (request.AliadoId is null && TieneRol(usuario, Roles.Externo))
            throw new ReglaNegocioException("Un usuario Externo debe estar asociado a un aliado.");

        var anterior = Instantanea(usuario);
        Aplicar(usuario, request, emailNormalizado);
        auditoria.Registrar(Recurso, "PUT", id.ToString(), 200, anterior, Instantanea(usuario));
        await db.SaveChangesAsync(ct);
        return ADetalle(usuario);
    }

    /// <summary>Activa o desactiva (TieneAcceso). Desactivar revoca de inmediato todos los tokens.</summary>
    public async Task<UsuarioDetalle> CambiarEstadoAsync(int id, bool tieneAcceso, int actorId, CancellationToken ct = default)
    {
        var usuario = await CargarAsync(id, ct);
        if (!tieneAcceso && id == actorId)
            throw new ReglaNegocioException("No puede desactivar su propia cuenta.");

        var anterior = Instantanea(usuario);
        usuario.TieneAcceso = tieneAcceso;
        auditoria.Registrar(Recurso, "PATCH", id.ToString(), 200, anterior, Instantanea(usuario));
        await db.SaveChangesAsync(ct);

        if (!tieneAcceso)
            await auth.RevocarTodosAsync(id, ct);
        return ADetalle(usuario);
    }

    public async Task<UsuarioDetalle> AsignarRolesAsync(int id, IReadOnlyList<string> roles, CancellationToken ct = default)
    {
        var usuario = await CargarAsync(id, ct);

        if (TieneRol(usuario, Roles.Administrador) && !roles.Contains(Roles.Administrador) && usuario.TieneAcceso)
        {
            var otrosAdminsActivos = await db.Usuarios.CountAsync(u =>
                u.Id != id && u.TieneAcceso && u.Roles.Any(ur => ur.Rol!.Nombre == Roles.Administrador), ct);
            if (otrosAdminsActivos == 0)
                throw new ReglaNegocioException("No se puede quitar el rol al último Administrador activo.");
        }

        if (roles.Contains(Roles.Externo) && usuario.AliadoId is null)
            throw new ReglaNegocioException("Un usuario Externo debe estar asociado a un aliado.");

        var anterior = Instantanea(usuario);
        var nuevos = await db.Roles.Where(r => roles.Contains(r.Nombre)).ToListAsync(ct);
        foreach (var quitado in usuario.Roles.Where(ur => nuevos.All(r => r.Id != ur.RolId)).ToList())
        {
            usuario.Roles.Remove(quitado);
            db.UsuarioRoles.Remove(quitado);
        }
        foreach (var rol in nuevos.Where(r => usuario.Roles.All(ur => ur.RolId != r.Id)))
            usuario.Roles.Add(new UsuarioRol { UsuarioId = id, RolId = rol.Id, Rol = rol });

        auditoria.Registrar(Recurso, "PUT", id.ToString(), 200, anterior, Instantanea(usuario));
        await db.SaveChangesAsync(ct);
        return ADetalle(usuario);
    }

    private async Task<Usuario> CargarAsync(int id, CancellationToken ct) =>
        await db.Usuarios.Include(u => u.Roles).ThenInclude(ur => ur.Rol)
            .FirstOrDefaultAsync(u => u.Id == id, ct)
        ?? throw new NoEncontradoException("El usuario no existe.");

    private async Task ValidarEmailUnicoAsync(string email, int? excluirId, CancellationToken ct)
    {
        if (await db.Usuarios.AnyAsync(u => u.Email == email && u.Id != excluirId, ct))
            throw new ReglaNegocioException("Ya existe un usuario con este correo");
    }

    private async Task ValidarAliadoAsync(int? aliadoId, CancellationToken ct)
    {
        if (aliadoId is not null && !await db.Aliados.AnyAsync(a => a.Id == aliadoId, ct))
            throw new ReglaNegocioException("El aliado seleccionado no existe.");
    }

    private async Task EnviarBienvenidaAsync(Usuario usuario, CancellationToken ct)
    {
        try
        {
            var token = enlaces.Generar(usuario.Id, Criptografia.SelloContrasena(usuario.PasswordHash), VigenciaEnlaceBienvenida);
            var enlace = $"{frontend.Value.Url.TrimEnd('/')}/definir-contrasena?token={Uri.EscapeDataString(token)}";
            var nombre = WebUtility.HtmlEncode(usuario.Nombres);
            var cuerpo =
                $"<p>Hola {nombre}:</p>" +
                "<p>Se creó su cuenta en la plataforma del CI²DT² de la Universidad de Caldas.</p>" +
                $"<p>Para definir su contraseña ingrese al siguiente enlace (válido durante 72 horas):</p>" +
                $"<p><a href=\"{enlace}\">Definir mi contraseña</a></p>";
            await email.EnviarAsync(usuario.Email, "Bienvenido a la plataforma CI²DT²", cuerpo, ct);
        }
        catch (Exception ex)
        {
            // La creación del usuario no se revierte si falla el envío del correo.
            logger.LogError(ex, "No se pudo enviar el correo de bienvenida al usuario {UsuarioId}", usuario.Id);
        }
    }

    private static void Aplicar(Usuario usuario, IDatosUsuario datos, string emailNormalizado)
    {
        usuario.Nombres = datos.Nombres.Trim();
        usuario.Apellidos = datos.Apellidos.Trim();
        usuario.TipoDocumento = Limpiar(datos.TipoDocumento);
        usuario.NumeroDocumento = Limpiar(datos.NumeroDocumento);
        usuario.Email = emailNormalizado;
        usuario.Telefono = Limpiar(datos.Telefono);
        usuario.Cargo = Limpiar(datos.Cargo);
        usuario.Dependencia = Limpiar(datos.Dependencia);
        usuario.CvLacUrl = Limpiar(datos.CvLacUrl);
        usuario.Orcid = Limpiar(datos.Orcid);
        usuario.AliadoId = datos.AliadoId;
    }

    private static string? Limpiar(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

    private static bool TieneRol(Usuario usuario, string rol) => usuario.Roles.Any(ur => ur.Rol?.Nombre == rol);

    private static List<string> NombresRoles(Usuario usuario) =>
        usuario.Roles.Select(ur => ur.Rol!.Nombre).OrderBy(r => r).ToList();

    /// <summary>Valores auditados del usuario: nunca incluye el hash de la contraseña ni tokens.</summary>
    private static object Instantanea(Usuario u) => new
    {
        u.Nombres,
        u.Apellidos,
        u.TipoDocumento,
        u.NumeroDocumento,
        u.Email,
        u.Telefono,
        u.Cargo,
        u.Dependencia,
        u.CvLacUrl,
        u.Orcid,
        u.AliadoId,
        u.TieneAcceso,
        Roles = NombresRoles(u),
    };

    private static UsuarioDetalle ADetalle(Usuario u) => new(
        u.Id, u.Nombres, u.Apellidos, u.TipoDocumento, u.NumeroDocumento, u.Email, u.Telefono,
        u.Cargo, u.Dependencia, u.CvLacUrl, u.Orcid, u.AliadoId, u.TieneAcceso, NombresRoles(u));
}
