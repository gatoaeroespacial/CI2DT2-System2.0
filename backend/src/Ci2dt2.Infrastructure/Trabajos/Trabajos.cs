using System.Threading.Channels;
using Ci2dt2.Application.Cronogramas;
using Ci2dt2.Application.Notificaciones;
using Ci2dt2.Application.Seguridad;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Ci2dt2.Infrastructure.Trabajos;

public sealed record CorreoDirecto(string Para, string Asunto, string CuerpoHtml);

/// <summary>Cola en memoria de correos directos (p. ej. restablecer contraseña, HU-03).</summary>
internal sealed class ColaCorreos : IColaCorreos
{
    private readonly Channel<CorreoDirecto> _canal = Channel.CreateUnbounded<CorreoDirecto>(new UnboundedChannelOptions { SingleReader = true });

    public ChannelReader<CorreoDirecto> Lector => _canal.Reader;

    public void Encolar(string para, string asunto, string cuerpoHtml) => _canal.Writer.TryWrite(new CorreoDirecto(para, asunto, cuerpoHtml));
}

/// <summary>Envía los correos directos de la cola en cuanto llegan.</summary>
internal sealed class CorreosDirectosWorker(ColaCorreos cola, IEmailSender correo, ILogger<CorreosDirectosWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (var c in cola.Lector.ReadAllAsync(stoppingToken))
            {
                for (var intento = 1; intento <= DespachadorCorreos.MaxIntentos; intento++)
                {
                    try
                    {
                        await correo.EnviarAsync(c.Para, c.Asunto, c.CuerpoHtml, stoppingToken);
                        break;
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        logger.LogWarning(ex, "Falló el envío del correo \"{Asunto}\" (intento {Intento})", c.Asunto, intento);
                        if (intento < DespachadorCorreos.MaxIntentos)
                            await Task.Delay(TimeSpan.FromSeconds(10 * intento), stoppingToken);
                    }
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
/// HU-23: despacha cada minuto las notificaciones pendientes de envío por correo. Si el hosting
/// recicla la aplicación, el envío se reanuda en el siguiente arranque o con el endpoint de jobs.
/// </summary>
internal sealed class NotificacionesCorreoWorker(IServiceScopeFactory scopes, IConfiguration configuracion, ILogger<NotificacionesCorreoWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalo = TimeSpan.FromSeconds(configuracion.GetValue("Trabajos:IntervaloCorreoSegundos", 60));
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopes.CreateScope();
                await scope.ServiceProvider.GetRequiredService<DespachadorCorreos>().ProcesarPendientesAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Error al despachar las notificaciones por correo");
            }

            try
            {
                await Task.Delay(intervalo, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }
}

/// <summary>
/// HU-18: ejecuta el motor de alertas a diario a partir de las 6:00 a. m. (hora de Colombia) mientras
/// la aplicación esté activa. En hosting compartido se complementa con un programador externo que
/// llama a POST /api/v1/jobs/alertas; ambos son seguros de repetir (idempotentes).
/// </summary>
internal sealed class AlertasProgramadasWorker(IServiceScopeFactory scopes, TimeProvider reloj, IConfiguration configuracion, ILogger<AlertasProgramadasWorker> logger)
    : BackgroundService
{
    private DateOnly? _ultimaEjecucion;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!configuracion.GetValue("Trabajos:AlertasAutomaticas", true))
            return;
        while (!stoppingToken.IsCancellationRequested)
        {
            var colombia = reloj.GetUtcNow().UtcDateTime.AddHours(-5);
            var hoy = DateOnly.FromDateTime(colombia);
            if (colombia.Hour >= 6 && _ultimaEjecucion != hoy)
            {
                try
                {
                    using var scope = scopes.CreateScope();
                    var resultado = await scope.ServiceProvider.GetRequiredService<AlertasService>().EjecutarAsync(hoy, stoppingToken);
                    _ultimaEjecucion = hoy;
                    logger.LogInformation("Alertas de hitos ejecutadas: {Resultado}", resultado);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogError(ex, "Error al ejecutar las alertas de hitos");
                }
            }

            try
            {
                await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }
}
