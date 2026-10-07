using System.Net;
using System.Net.Mail;
using Ci2dt2.Application.Seguridad;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ci2dt2.Infrastructure.Email;

public sealed class OpcionesSmtp
{
    public const string Seccion = "Smtp";

    /// <summary>Si está vacío, los correos solo se escriben en el log (desarrollo).</summary>
    public string? Host { get; set; }
    public int Puerto { get; set; } = 587;
    public bool UsarSsl { get; set; } = true;
    public string? Usuario { get; set; }
    public string? Contrasena { get; set; }
    public string Remitente { get; set; } = "no-responder@ucaldas.edu.co";
    public string NombreRemitente { get; set; } = "CI²DT² Universidad de Caldas";
}

internal sealed class SmtpEmailSender(IOptions<OpcionesSmtp> opciones) : IEmailSender
{
    public async Task EnviarAsync(string para, string asunto, string cuerpoHtml, CancellationToken ct = default)
    {
        var o = opciones.Value;
        using var mensaje = new MailMessage
        {
            From = new MailAddress(o.Remitente, o.NombreRemitente),
            Subject = asunto,
            Body = cuerpoHtml,
            IsBodyHtml = true,
        };
        mensaje.To.Add(para);

        using var cliente = new SmtpClient(o.Host, o.Puerto) { EnableSsl = o.UsarSsl };
        if (!string.IsNullOrEmpty(o.Usuario))
            cliente.Credentials = new NetworkCredential(o.Usuario, o.Contrasena);
        await cliente.SendMailAsync(mensaje, ct);
    }
}

/// <summary>Remplazo para desarrollo: registra el correo en el log en lugar de enviarlo.</summary>
internal sealed class LogEmailSender(ILogger<LogEmailSender> logger) : IEmailSender
{
    public Task EnviarAsync(string para, string asunto, string cuerpoHtml, CancellationToken ct = default)
    {
        logger.LogWarning("SMTP no configurado. Correo para {Para} con asunto {Asunto}: {Cuerpo}", para, asunto, cuerpoHtml);
        return Task.CompletedTask;
    }
}
