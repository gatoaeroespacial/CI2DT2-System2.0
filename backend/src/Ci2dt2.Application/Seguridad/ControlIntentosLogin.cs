using Microsoft.Extensions.Caching.Memory;

namespace Ci2dt2.Application.Seguridad;

/// <summary>
/// Bloqueo temporal por intentos fallidos (HU-01): 5 fallos consecutivos en 15 minutos
/// bloquean la cuenta durante 15 minutos. El contador vive en la caché del servidor.
/// </summary>
public sealed class ControlIntentosLogin(IMemoryCache cache, TimeProvider reloj)
{
    public const int MaxIntentos = 5;
    public static readonly TimeSpan Ventana = TimeSpan.FromMinutes(15);
    public static readonly TimeSpan DuracionBloqueo = TimeSpan.FromMinutes(15);

    private sealed class Estado
    {
        public int Fallos;
        public DateTimeOffset PrimerFallo;
        public DateTimeOffset? BloqueadoHasta;
    }

    /// <summary>Tiempo restante de bloqueo, o null si la cuenta no está bloqueada.</summary>
    public TimeSpan? TiempoRestanteBloqueo(string email)
    {
        if (!cache.TryGetValue(Clave(email), out Estado? estado) || estado?.BloqueadoHasta is null)
            return null;

        var restante = estado.BloqueadoHasta.Value - reloj.GetUtcNow();
        return restante > TimeSpan.Zero ? restante : null;
    }

    public void RegistrarFallo(string email)
    {
        var ahora = reloj.GetUtcNow();
        var clave = Clave(email);
        lock (cache)
        {
            cache.TryGetValue(clave, out Estado? estado);
            var expirado = estado is null
                || (estado.BloqueadoHasta is { } hasta ? hasta <= ahora : ahora - estado.PrimerFallo > Ventana);
            if (expirado)
                estado = new Estado { PrimerFallo = ahora };

            estado!.Fallos++;
            if (estado.Fallos >= MaxIntentos)
                estado.BloqueadoHasta = ahora + DuracionBloqueo;

            cache.Set(clave, estado, TimeSpan.FromMinutes(30));
        }
    }

    public void Reiniciar(string email) => cache.Remove(Clave(email));

    private static string Clave(string email) => "login-fallos:" + email.Trim().ToLowerInvariant();
}
