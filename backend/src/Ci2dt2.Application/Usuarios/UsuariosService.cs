using Ci2dt2.Application.Common;
using Ci2dt2.Application.Seguridad;
using Ci2dt2.Domain;
using Ci2dt2.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ci2dt2.Application.Usuarios;

/// <summary>
/// Comandos de administración de usuarios y roles (HU-04). La auditoría de cada cambio la
/// registra el middleware de HU-28 con los valores anteriores y nuevos (sin el hash).
/// </summary>
public sealed class UsuariosService(IApplicationDbContext db, AuthService auth, IPasswordHasher hasher)
{
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
        // El Administrador define la contraseña inicial; nunca se muestra ni se envía por correo.
        var usuario = new Usuario { TieneAcceso = true, PasswordHash = hasher.Hash(request.Contrasena) };
        Aplicar(usuario, request, emailNormalizado);
        usuario.Roles = roles.Select(r => new UsuarioRol { Rol = r }).ToList();

        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync(ct);
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

        Aplicar(usuario, request, emailNormalizado);
        await db.SaveChangesAsync(ct);
        return ADetalle(usuario);
    }

    /// <summary>Activa o desactiva (TieneAcceso). Desactivar revoca de inmediato todos los tokens.</summary>
    public async Task<UsuarioDetalle> CambiarEstadoAsync(int id, bool tieneAcceso, int actorId, CancellationToken ct = default)
    {
        var usuario = await CargarAsync(id, ct);
        if (!tieneAcceso && id == actorId)
            throw new ReglaNegocioException("No puede desactivar su propia cuenta.");
        if (tieneAcceso && usuario.PasswordHash is null)
            throw new ReglaNegocioException("El usuario no tiene contraseña: asígnele una antes de activarlo.");

        usuario.TieneAcceso = tieneAcceso;
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

        var nuevos = await db.Roles.Where(r => roles.Contains(r.Nombre)).ToListAsync(ct);
        foreach (var quitado in usuario.Roles.Where(ur => nuevos.All(r => r.Id != ur.RolId)).ToList())
        {
            usuario.Roles.Remove(quitado);
            db.UsuarioRoles.Remove(quitado);
        }
        foreach (var rol in nuevos.Where(r => usuario.Roles.All(ur => ur.RolId != r.Id)))
            usuario.Roles.Add(new UsuarioRol { UsuarioId = id, RolId = rol.Id, Rol = rol });

        await db.SaveChangesAsync(ct);
        return ADetalle(usuario);
    }

    /// <summary>Asigna una nueva contraseña y revoca todas las sesiones activas del usuario (HU-04 CA-6).</summary>
    public async Task AsignarContrasenaAsync(int id, string contrasena, CancellationToken ct = default)
    {
        var usuario = await CargarAsync(id, ct);
        usuario.PasswordHash = hasher.Hash(contrasena);
        await db.SaveChangesAsync(ct);
        await auth.RevocarTodosAsync(id, ct);
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
        if (aliadoId is not null && !await db.Aliados.AnyAsync(a => a.Id == aliadoId && a.Activo, ct))
            throw new ReglaNegocioException("El aliado seleccionado no existe.");
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

    private static UsuarioDetalle ADetalle(Usuario u) => new(
        u.Id, u.Nombres, u.Apellidos, u.TipoDocumento, u.NumeroDocumento, u.Email, u.Telefono,
        u.Cargo, u.Dependencia, u.CvLacUrl, u.Orcid, u.AliadoId, u.TieneAcceso, NombresRoles(u));
}
