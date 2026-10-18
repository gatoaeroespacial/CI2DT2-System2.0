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

/// <summary>Envío de correo (SMTP). Lo usarán las notificaciones por correo de HU-23.</summary>
public interface IEmailSender
{
    Task EnviarAsync(string para, string asunto, string cuerpoHtml, CancellationToken ct = default);
}

/// <summary>Tokens de descarga temporales: la descarga nunca usa una ruta pública (HU-21).</summary>
public interface IEnlaceDescarga
{
    string Generar(int documentoId, TimeSpan vigencia);

    /// <summary>Id del documento, o null si el token es inválido o expiró.</summary>
    int? Validar(string token);
}
