using System.Text.Encodings.Web;
using System.Text.Json;
using Ci2dt2.Domain.Entities;

namespace Ci2dt2.Application.Common;

/// <summary>
/// Construye registros de LogActividades y los encola para escritura asíncrona (HU-28).
/// Nunca se deben pasar contraseñas, hashes ni tokens.
/// </summary>
public sealed class Auditoria(IColaAuditoria cola, IContextoPeticion contexto, TimeProvider reloj)
{
    // Se guarda en la base de datos (no en HTML): se conservan las tildes sin escapar.
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public void Registrar(
        string recurso,
        string accion,
        string? recursoId,
        int estadoHttp,
        object? valoresAnteriores = null,
        object? valoresNuevos = null,
        int? usuarioId = null)
    {
        cola.Encolar(new LogActividad
        {
            UsuarioId = usuarioId ?? contexto.UsuarioId,
            IpOrigen = Recortar(contexto.Ip, 45),
            Recurso = Recortar(recurso, 60)!,
            RecursoId = Recortar(recursoId, 40),
            Accion = Recortar(accion, 10)!,
            Ruta = Recortar(contexto.Ruta, 300),
            EstadoHttp = estadoHttp,
            ValoresAnteriores = valoresAnteriores is null ? null : JsonSerializer.Serialize(valoresAnteriores, Json),
            ValoresNuevos = valoresNuevos is null ? null : JsonSerializer.Serialize(valoresNuevos, Json),
            FechaUtc = reloj.GetUtcNow().UtcDateTime,
        });
    }

    private static string? Recortar(string? valor, int max) =>
        valor is null || valor.Length <= max ? valor : valor[..max];
}
