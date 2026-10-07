using System.Text.Encodings.Web;
using System.Text.Json;
using Ci2dt2.Domain.Entities;

namespace Ci2dt2.Application.Common;

/// <summary>
/// Agrega registros a LogActividades. El registro se guarda en el mismo SaveChanges
/// que la operación auditada. Nunca se deben pasar contraseñas, hashes ni tokens.
/// </summary>
public sealed class Auditoria(IApplicationDbContext db, IContextoPeticion contexto, TimeProvider reloj)
{
    // Se guarda en la base de datos (no en HTML): se conservan las tildes sin escapar.
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
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
        db.LogActividades.Add(new LogActividad
        {
            UsuarioId = usuarioId ?? contexto.UsuarioId,
            IpOrigen = Recortar(contexto.Ip, 45),
            Recurso = recurso,
            RecursoId = recursoId,
            Accion = accion,
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
