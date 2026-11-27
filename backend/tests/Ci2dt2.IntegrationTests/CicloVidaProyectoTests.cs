using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;

namespace Ci2dt2.IntegrationTests;

public sealed record FlujoCierreDto(bool PuedeFinalizar, bool PuedeGestionarHitos, bool PuedeRegistrarAvances);
public sealed record FichaCierreDto(EstadoFichaDto Proyecto, FlujoCierreDto Flujo);
public sealed record HitoCreadoDto(int Id);

/// <summary>
/// Ciclo de vida completo de un proyecto, sin atajos en la base de datos: el IP formula y envía, el
/// Coordinador aprueba, el Director avala, aprueba con el acto, inicia la ejecución y finaliza; en la
/// ejecución el IP define hitos y registra avances que valida el Coordinador.
/// </summary>
[Collection(ColeccionApi.Nombre)]
public class CicloVidaProyectoTests(ApiFactory api)
{
    private static readonly DateOnly Hoy = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(-5));

    private static async Task<string> EstadoAsync(HttpClient cliente, int id) => (await cliente.FichaAsync(id)).Proyecto.Estado;

    [Fact]
    public async Task Ciclo_completo_de_la_formulacion_a_la_finalizacion()
    {
        var e = await api.EscenarioAsync();

        // 1. Investigador (IP): formula la propuesta y la envía a revisión.
        var id = await e.PropuestaCompletaAsync();
        Assert.Equal("En elaboración", await EstadoAsync(e.CliIp, id));
        Assert.Equal(HttpStatusCode.NoContent, (await e.CliIp.PostAsync($"/api/v1/proyectos/{id}/enviar-revision", null)).StatusCode);
        Assert.Equal("En revisión", await EstadoAsync(e.CliIp, id));
        Assert.True(await api.TieneNotificacionAsync(e.Coordinador.Id, "PropuestaEnRevision", id));

        // 2. Coordinador: aprueba y la envía al Director.
        Assert.Equal(HttpStatusCode.NoContent, (await e.CliCoordinador.RevisarAsync(id, "Aprueba", "Propuesta pertinente para el programa.")).StatusCode);
        Assert.Equal("Director", (await e.CliDirector.FichaAsync(id)).Flujo.EtapaRevision);
        Assert.True(await api.TieneNotificacionAsync(e.Director.Id, "PropuestaAprobadaCoordinador", id));

        // 3. Director: otorga el aval institucional → Presentado.
        Assert.Equal(HttpStatusCode.NoContent, (await e.CliDirector.RevisarAsync(id, "Avala")).StatusCode);
        Assert.Equal("Presentado", await EstadoAsync(e.CliDirector, id));
        Assert.True(await api.TieneNotificacionAsync(e.Ip.Id, "PropuestaAvalada", id));

        // 4. Director: con el acto administrativo registra la aprobación → Aprobado y cronograma creado.
        Assert.Equal(HttpStatusCode.Created, (await e.CliDirector.SubirDocumentoAsync("Proyecto", id, 8, "acto.pdf")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent,
            (await e.CliDirector.PostAsJsonAsync($"/api/v1/proyectos/{id}/aprobacion", new { aprobado = true })).StatusCode);
        Assert.Equal("Aprobado", await EstadoAsync(e.CliDirector, id));
        Assert.True(await api.TieneNotificacionAsync(e.Ip.Id, "ProyectoAprobado", id));

        // 5. IP: define el hito del cronograma.
        var creado = await e.CliIp.PostAsJsonAsync($"/api/v1/proyectos/{id}/hitos", new
        {
            tipoHitoId = 1,
            titulo = "Informe final",
            fechaLimite = Hoy.AddDays(60),
            responsableId = e.Ip.Id,
            alertarDias = 7,
        });
        Assert.Equal(HttpStatusCode.Created, creado.StatusCode);
        var hito = (await creado.Content.ReadFromJsonAsync<HitoCreadoDto>())!.Id;

        // 6. Director: con el plan de trabajo inicia la ejecución.
        Assert.Equal(HttpStatusCode.Created, (await e.CliDirector.SubirDocumentoAsync("Proyecto", id, 2, "plan.pdf")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await e.CliDirector.PostAsJsonAsync($"/api/v1/proyectos/{id}/iniciar-ejecucion",
            new { fechaInicio = Hoy.ToString("yyyy-MM-dd") })).StatusCode);
        Assert.Equal("En ejecución", await EstadoAsync(e.CliDirector, id));

        // 7. IP registra el avance final; el Coordinador lo valida y el proyecto queda al 100 %.
        var registro = await e.CliIp.PostAsJsonAsync($"/api/v1/proyectos/{id}/avances",
            new { titulo = "Resultados finales", porcentajeAvance = 100m, hitoId = hito });
        Assert.Equal(HttpStatusCode.Created, registro.StatusCode);
        var avance = (await registro.Content.ReadFromJsonAsync<AvanceGuardadoDto>())!.Id;

        // Con el avance pendiente y el hito abierto no se puede finalizar.
        var prematura = await e.CliDirector.PostAsJsonAsync($"/api/v1/proyectos/{id}/finalizacion",
            new { observaciones = "Cierre del proyecto con todos los productos." });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, prematura.StatusCode);
        Assert.Contains("hitos", (await prematura.Content.ReadFromJsonAsync<ErrorDto>())!.Error);

        Assert.Equal(HttpStatusCode.NoContent,
            (await e.CliCoordinador.PostAsJsonAsync($"/api/v1/avances/{avance}/validaciones", new { resultado = "Validado" })).StatusCode);
        Assert.Equal(100m, (await e.CliIp.FichaAsync(id)).Proyecto.PorcentajeAvance);

        // 8. IP completa el hito con el avance como evidencia.
        Assert.Equal(HttpStatusCode.NoContent,
            (await e.CliIp.PostAsJsonAsync($"/api/v1/hitos/{hito}/completar", new { avanceId = avance })).StatusCode);

        // 9. Director finaliza: el IP no puede y sin observaciones no se permite.
        Assert.Equal(HttpStatusCode.Forbidden, (await e.CliIp.PostAsJsonAsync($"/api/v1/proyectos/{id}/finalizacion",
            new { observaciones = "Cierre del proyecto con todos los productos." })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,
            (await e.CliDirector.PostAsJsonAsync($"/api/v1/proyectos/{id}/finalizacion", new { observaciones = "Listo" })).StatusCode);
        Assert.True((await e.CliDirector.GetFromJsonAsync<FichaCierreDto>($"/api/v1/proyectos/{id}"))!.Flujo.PuedeFinalizar);
        Assert.Equal(HttpStatusCode.NoContent, (await e.CliDirector.PostAsJsonAsync($"/api/v1/proyectos/{id}/finalizacion",
            new { observaciones = "Cierre del proyecto con todos los productos entregados." })).StatusCode);

        var final = (await e.CliIp.GetFromJsonAsync<FichaCierreDto>($"/api/v1/proyectos/{id}"))!;
        Assert.Equal("Finalizado", final.Proyecto.Estado);
        Assert.False(final.Flujo.PuedeGestionarHitos);
        Assert.False(final.Flujo.PuedeRegistrarAvances);
        Assert.True(await api.TieneNotificacionAsync(e.Ip.Id, "ProyectoFinalizado", id));
        Assert.True(await api.TieneNotificacionAsync(e.Coordinador.Id, "ProyectoFinalizado", id));

        // Historial completo y estado final: no admite más transiciones.
        var decisiones = await api.EnBaseDatosAsync(db => db.ProyectoRevisiones.Where(r => r.ProyectoId == id).OrderBy(r => r.Id)
            .Select(r => r.Decision).ToListAsync());
        Assert.Equal(["Aprueba", "Avala", "Aprobado", "Inicia ejecución", "Finaliza"], decisiones);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, (await e.CliDirector.PostAsJsonAsync($"/api/v1/proyectos/{id}/suspension",
            new { observaciones = "Intento de suspender un proyecto finalizado." })).StatusCode);

        // 10. Visitante: el proyecto finalizado se publica en el portal.
        var codigo = await api.EnBaseDatosAsync(db => db.Proyectos.Where(p => p.Id == id).Select(p => p.Codigo!).SingleAsync());
        var publico = await api.CreateClient().GetFromJsonAsync<ProyectoPublicoDetalleDto>($"/api/v1/publico/proyectos/{codigo.ToLowerInvariant()}");
        Assert.Equal("Finalizado", publico!.Proyecto.Estado);
    }
}
