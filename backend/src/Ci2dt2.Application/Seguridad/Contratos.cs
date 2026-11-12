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

/// <summary>Envío de correo (SMTP) para las notificaciones (HU-23) y el restablecimiento de contraseña (HU-03).</summary>
public interface IEmailSender
{
    Task EnviarAsync(string para, string asunto, string cuerpoHtml, CancellationToken ct = default);
}

/// <summary>
/// Tokens de descarga temporales: la descarga nunca usa una ruta pública (HU-21). El recurso
/// distingue documentos de adjuntos de avances, para que un token no sirva para el otro.
/// </summary>
public interface IEnlaceDescarga
{
    string Generar(int id, TimeSpan vigencia, string recurso = RecursosDescarga.Documento);

    /// <summary>Id del recurso, o null si el token es inválido, expiró o es de otro recurso.</summary>
    int? Validar(string token, string recurso = RecursosDescarga.Documento);
}

public static class RecursosDescarga
{
    public const string Documento = "Documento";
    public const string AdjuntoAvance = "AdjuntoAvance";
}

/// <summary>
/// Enlace de restablecimiento de contraseña (HU-03): firmado con la Data Protection API, válido
/// 30 minutos y sellado con el hash actual de la contraseña, por lo que deja de servir al usarse.
/// </summary>
public interface ITokenRestablecimiento
{
    string Generar(int usuarioId, string passwordHash, TimeSpan vigencia);

    /// <summary>Id del usuario y sello, o null si el token es inválido o expiró.</summary>
    (int UsuarioId, string Sello)? Leer(string token);

    string Sello(string passwordHash);
}
