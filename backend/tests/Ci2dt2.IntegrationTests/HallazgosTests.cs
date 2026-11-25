using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;

namespace Ci2dt2.IntegrationTests;

public sealed record ReprogramacionDto(string? FechaAnterior, string? FechaNueva, string? Justificacion);
public sealed record HitoConHistorialDto(int Id, string Estado, ReprogramacionDto[] Reprogramaciones);
public sealed record FlujoEjecucionDto(bool PuedeSuspender, bool PuedeReanudar);
public sealed record FichaEjecucionDto(EstadoFichaDto Proyecto, FlujoEjecucionDto Flujo);

/// <summary>Correcciones de los hallazgos del informe de funcionamiento por rol.</summary>
[Collection(ColeccionApi.Nombre)]
public class HallazgosTests(ApiFactory api)
{
    private const string Justificacion = "Retraso en la entrega de equipos por parte del proveedor.";

    [Fact]
    public async Task HU20_el_IP_no_valida_su_propio_avance_y_pasa_directo_al_Coordinador()
    {
        var e = await api.EscenarioAsync();
        var id = await e.PropuestaCompletaAsync();
        await api.ConCronogramaAsync(id, 6);

        var registro = await e.CliIp.PostAsJsonAsync($"/api/v1/proyectos/{id}/avances", new { titulo = "Avance propio", porcentajeAvance = 15m });
        var avance = (await registro.Content.ReadFromJsonAsync<AvanceGuardadoDto>())!;

        // No aparece en la bandeja del IP (autor) y el IP no puede validarlo.
        var bandejaIp = await e.CliIp.GetFromJsonAsync<BandejaAvancesDto>($"/api/v1/avances/pendientes?proyectoId={id}");
        Assert.Empty(bandejaIp!.Items);
        var autovalidacion = await e.CliIp.PostAsJsonAsync($"/api/v1/avances/{avance.Id}/validaciones", new { resultado = "Validado" });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, autovalidacion.StatusCode);

        // Sí está en la bandeja del Coordinador, que da la validación final.
        var bandejaCoordinador = await e.CliCoordinador.GetFromJsonAsync<BandejaAvancesDto>($"/api/v1/avances/pendientes?proyectoId={id}");
        Assert.Equal(avance.Id, Assert.Single(bandejaCoordinador!.Items).Id);
    }

    [Fact]
    public async Task El_Director_suspende_y_reanuda_un_proyecto_en_ejecucion_con_justificacion()
    {
        var e = await api.EscenarioAsync();
        var id = await e.PropuestaCompletaAsync();
        await api.ConCronogramaAsync(id, 6);

        var ficha = await e.CliDirector.GetFromJsonAsync<FichaEjecucionDto>($"/api/v1/proyectos/{id}");
        Assert.True(ficha!.Flujo.PuedeSuspender);
        Assert.False((await e.CliIp.GetFromJsonAsync<FichaEjecucionDto>($"/api/v1/proyectos/{id}"))!.Flujo.PuedeSuspender);

        // Sin justificación no se permite; el IP no puede suspender.
        Assert.Equal(HttpStatusCode.BadRequest,
            (await e.CliDirector.PostAsJsonAsync($"/api/v1/proyectos/{id}/suspension", new { observaciones = "corta" })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,
            (await e.CliIp.PostAsJsonAsync($"/api/v1/proyectos/{id}/suspension", new { observaciones = Justificacion })).StatusCode);

        Assert.Equal(HttpStatusCode.NoContent,
            (await e.CliDirector.PostAsJsonAsync($"/api/v1/proyectos/{id}/suspension", new { observaciones = Justificacion })).StatusCode);
        var suspendido = await e.CliDirector.GetFromJsonAsync<FichaEjecucionDto>($"/api/v1/proyectos/{id}");
        Assert.Equal(9, suspendido!.Proyecto.EstadoId);
        Assert.True(suspendido.Flujo.PuedeReanudar);
        Assert.Contains(suspendido.Proyecto.Revisiones, r => r.Etapa == "Director");
        Assert.True(await api.TieneNotificacionAsync(e.Ip.Id, "ProyectoSuspendido", id));

        // Suspendido no admite avances.
        Assert.Equal(HttpStatusCode.UnprocessableEntity,
            (await e.CliIp.PostAsJsonAsync($"/api/v1/proyectos/{id}/avances", new { titulo = "Avance", porcentajeAvance = 10m })).StatusCode);

        Assert.Equal(HttpStatusCode.NoContent,
            (await e.CliDirector.PostAsJsonAsync($"/api/v1/proyectos/{id}/reanudacion", new { observaciones = "Se recibieron los equipos y se retoma el plan." })).StatusCode);
        Assert.Equal(6, (await e.CliDirector.GetFromJsonAsync<FichaEjecucionDto>($"/api/v1/proyectos/{id}"))!.Proyecto.EstadoId);
        Assert.True(await api.TieneNotificacionAsync(e.Ip.Id, "ProyectoReanudado", id));
    }

    [Fact]
    public async Task HU17_el_cronograma_muestra_el_historial_de_reprogramaciones()
    {
        var e = await api.EscenarioAsync();
        var id = await e.PropuestaCompletaAsync();
        var cronograma = await api.ConCronogramaAsync(id, 6);
        var fecha = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(20));
        var hito = await api.HitoAsync(cronograma, e.Ip.Id, fecha);

        var nueva = fecha.AddDays(30);
        var respuesta = await e.CliIp.PutAsJsonAsync($"/api/v1/hitos/{hito}", new
        {
            tipoHitoId = 1,
            titulo = "Hito de prueba",
            fechaLimite = nueva,
            responsableId = e.Ip.Id,
            alertarDias = 7,
            justificacion = Justificacion,
        });
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        await api.EsperarLogsAsync(l => l.Recurso == "hitos" && l.Accion == "REPROGRAMA" && l.RecursoId == hito.ToString());

        var hitos = await e.CliIp.GetFromJsonAsync<HitoConHistorialDto[]>($"/api/v1/proyectos/{id}/hitos");
        var reprogramacion = Assert.Single(Assert.Single(hitos!, h => h.Id == hito).Reprogramaciones);
        Assert.Equal(fecha.ToString("yyyy-MM-dd"), reprogramacion.FechaAnterior);
        Assert.Equal(nueva.ToString("yyyy-MM-dd"), reprogramacion.FechaNueva);
        Assert.Equal(Justificacion, reprogramacion.Justificacion);
    }
}
