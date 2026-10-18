using System.Threading.Channels;
using Ci2dt2.Application.Common;
using Ci2dt2.Domain.Entities;
using Ci2dt2.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Ci2dt2.Infrastructure.Auditoria;

/// <summary>Cola en memoria de registros de auditoría (HU-28: escritura asíncrona y no bloqueante).</summary>
internal sealed class ColaAuditoria : IColaAuditoria
{
    private readonly Channel<LogActividad> _canal = Channel.CreateUnbounded<LogActividad>(
        new UnboundedChannelOptions { SingleReader = true });

    public ChannelReader<LogActividad> Lector => _canal.Reader;

    public void Encolar(LogActividad registro) => _canal.Writer.TryWrite(registro);
}

/// <summary>
/// Worker en segundo plano que escribe los registros de la cola en LogActividades. Un fallo al
/// escribir no afecta la operación del usuario: queda en el log técnico (HU-28 CA-2).
/// </summary>
internal sealed class AuditoriaWorker(ColaAuditoria cola, IServiceScopeFactory scopes, ILogger<AuditoriaWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (var registro in cola.Lector.ReadAllAsync(stoppingToken))
            {
                try
                {
                    using var scope = scopes.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    db.LogActividades.Add(registro);
                    await db.SaveChangesAsync(stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogError(ex, "No se pudo escribir el registro de auditoría {Recurso} {Accion} {Ruta}",
                        registro.Recurso, registro.Accion, registro.Ruta);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Apagado de la aplicación.
        }
    }
}

/// <summary>
/// Captura, por petición, los cambios que se guardan con EF Core: valores anteriores y nuevos
/// de cada entidad, sin contraseñas, hashes, tokens ni columnas de auditoría.
/// </summary>
internal sealed class CapturaCambios : SaveChangesInterceptor, ICambiosPeticion
{
    private static readonly HashSet<string> Excluidas =
    [
        nameof(Usuario.PasswordHash), nameof(Usuario.ExpoPushToken), nameof(RefreshToken.TokenHash),
        nameof(EntidadAuditable.CreadoEn), nameof(EntidadAuditable.CreadoPorId), nameof(EntidadAuditable.ActualizadoEn),
    ];

    private sealed record Captura(EntityEntry Entrada, string Estado, Dictionary<string, object?> Anteriores, Dictionary<string, object?> Nuevos);

    private readonly List<Captura> _capturas = [];

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Capturar(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Capturar(eventData.Context);
        return ValueTask.FromResult(result);
    }

    private void Capturar(DbContext? db)
    {
        if (db is null)
            return;

        foreach (var entrada in db.ChangeTracker.Entries())
        {
            // Los propios registros de auditoría y los tokens de sesión no se auditan.
            if (entrada.Entity is LogActividad or RefreshToken)
                continue;

            var anteriores = new Dictionary<string, object?>();
            var nuevos = new Dictionary<string, object?>();
            foreach (var propiedad in entrada.Properties.Where(p => !Excluidas.Contains(p.Metadata.Name)))
            {
                var nombre = System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(propiedad.Metadata.Name);
                switch (entrada.State)
                {
                    case EntityState.Modified when propiedad.IsModified && !Equals(propiedad.OriginalValue, propiedad.CurrentValue):
                        anteriores[nombre] = propiedad.OriginalValue;
                        nuevos[nombre] = propiedad.CurrentValue;
                        break;
                    case EntityState.Deleted:
                        anteriores[nombre] = propiedad.OriginalValue;
                        break;
                }
            }

            if (entrada.State is EntityState.Added or EntityState.Deleted || (entrada.State == EntityState.Modified && nuevos.Count > 0))
                _capturas.Add(new Captura(entrada, entrada.State.ToString(), anteriores, nuevos));
        }
    }

    public IReadOnlyList<CambioEntidad> Obtener() => _capturas.Select(c =>
    {
        var clave = c.Entrada.Metadata.FindPrimaryKey()!.Properties
            .Select(p => c.Entrada.Property(p.Name).CurrentValue?.ToString());
        var nuevos = c.Nuevos;
        if (c.Estado == nameof(EntityState.Added))
        {
            // Para los registros nuevos se toman los valores ya guardados (incluye la llave generada).
            nuevos = c.Entrada.Properties
                .Where(p => !Excluidas.Contains(p.Metadata.Name))
                .ToDictionary(p => System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(p.Metadata.Name), p => p.CurrentValue);
        }
        return new CambioEntidad(c.Entrada.Metadata.ClrType.Name, c.Estado, string.Join("-", clave), c.Anteriores, nuevos);
    }).ToList();
}
