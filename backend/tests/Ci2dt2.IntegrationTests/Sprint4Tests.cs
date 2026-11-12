using System.Net;
using System.Net.Http.Json;
using Ci2dt2.Application.Cronogramas;
using Ci2dt2.Application.Importacion;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ci2dt2.IntegrationTests;

public sealed record AvanceGuardadoDto(int Id, bool Existente);
public sealed record AvancePendienteDto(int Id, int ProyectoId, string RolValidador);
public sealed record BandejaAvancesDto(int Total, AvancePendienteDto[] Items);
public sealed record AlertaDto(int HitoId, string Estado, int ProyectoId);
public sealed record TarjetasDto(int ProyectosEnEjecucion, int PropuestasEnElaboracion, int PropuestasEnRevision);
public sealed record TableroDto(TarjetasDto Tarjetas);
public sealed record ProyectoPublicoDto(string Codigo, string Titulo, string Estado);
public sealed record ProyectoPublicoDetalleDto(ProyectoPublicoDto Proyecto);
public sealed record ProgramaPublicoDto(string Nombre, string Slug);

/// <summary>Sprint 4: HU-14, HU-18, HU-19, HU-20, HU-24, HU-25, HU-26 y HU-27.</summary>
[Collection(ColeccionApi.Nombre)]
public class Sprint4Tests(ApiFactory api)
{
    private static DateOnly HoyColombia => DateOnly.FromDateTime(DateTime.UtcNow.AddHours(-5));

    private async Task<ResultadoAlertas> EjecutarAlertasAsync()
    {
        using var scope = api.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<AlertasService>().EjecutarAsync(HoyColombia);
    }

    private Task<int> ContarAsync(int usuarioId, string tipo, int hitoId) =>
        api.EnBaseDatosAsync(db => db.Notificaciones.CountAsync(n => n.UsuarioId == usuarioId && n.Tipo == tipo && n.ReferenciaId == hitoId));

    // ----- HU-18 -----

    [Fact]
    public async Task HU18_alerta_proxima_idempotente_y_vencido_notifica_al_coordinador()
    {
        var e = await api.EscenarioAsync();
        var id = await e.PropuestaCompletaAsync();
        var cronograma = await api.ConCronogramaAsync(id, 6);
        var responsable = await api.UsuarioAsync("resp", "Investigador");
        await e.CliIp.PostAsJsonAsync($"/api/v1/proyectos/{id}/integrantes", new { usuarioId = responsable.Id, rolEnProyecto = "Co-investigador" });
        var proximo = await api.HitoAsync(cronograma, responsable.Id, HoyColombia.AddDays(7), alertarDias: 7);
        var vencido = await api.HitoAsync(cronograma, responsable.Id, HoyColombia.AddDays(-1));

        await EjecutarAlertasAsync();

        // CA-1: el responsable y el IP reciben la alerta del hito próximo.
        Assert.Equal(1, await ContarAsync(responsable.Id, AlertasService.TipoProximo, proximo));
        Assert.Equal(1, await ContarAsync(e.Ip.Id, AlertasService.TipoProximo, proximo));
        // CA-2: el vencido cambia de estado y se notifica también al Coordinador.
        Assert.Equal("Vencido", await api.EnBaseDatosAsync(db => db.Hitos.Where(h => h.Id == vencido).Select(h => h.Estado).SingleAsync()));
        Assert.Equal(1, await ContarAsync(e.Coordinador.Id, AlertasService.TipoVencido, vencido));

        // CA-3: reejecutar el mismo día no duplica notificaciones.
        await EjecutarAlertasAsync();
        Assert.Equal(1, await ContarAsync(responsable.Id, AlertasService.TipoProximo, proximo));
        Assert.Equal(1, await ContarAsync(e.Coordinador.Id, AlertasService.TipoVencido, vencido));

        // CA-4: "Mis alertas" del responsable.
        var cliente = await api.ClienteDeAsync(responsable);
        var alertas = await cliente.GetFromJsonAsync<AlertaDto[]>("/api/v1/alertas/pendientes");
        Assert.Contains(alertas!, a => a.HitoId == proximo);
        Assert.Contains(alertas!, a => a.HitoId == vencido && a.Estado == "Vencido");
    }

    [Fact]
    public async Task HU18_el_job_externo_exige_la_clave_de_api()
    {
        var cliente = api.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await cliente.PostAsync("/api/v1/jobs/alertas", null)).StatusCode);

        var peticion = new HttpRequestMessage(HttpMethod.Post, "/api/v1/jobs/alertas");
        peticion.Headers.Add("X-Api-Key", ApiFactory.ClaveJobs);
        Assert.Equal(HttpStatusCode.OK, (await cliente.SendAsync(peticion)).StatusCode);
    }

    // ----- HU-19 y HU-20 -----

    [Fact]
    public async Task HU19_HU20_registro_idempotente_validacion_y_rechazo_con_correccion()
    {
        var e = await api.EscenarioAsync();
        var id = await e.PropuestaCompletaAsync();

        // Solo en proyectos en ejecución.
        var noEjecucion = await e.CliIp.PostAsJsonAsync($"/api/v1/proyectos/{id}/avances", new { titulo = "Avance 1", porcentajeAvance = 10m });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, noEjecucion.StatusCode);
        await api.ConCronogramaAsync(id, 6);

        // CA-1 y CA-3: queda Pendiente, se notifica y el mismo IdCliente no duplica.
        var idCliente = Guid.NewGuid();
        var primero = await e.CliIp.PostAsJsonAsync($"/api/v1/proyectos/{id}/avances", new { titulo = "Avance 1", porcentajeAvance = 20m, idCliente });
        var segundo = await e.CliIp.PostAsJsonAsync($"/api/v1/proyectos/{id}/avances", new { titulo = "Avance 1", porcentajeAvance = 20m, idCliente });
        Assert.Equal(HttpStatusCode.Created, primero.StatusCode);
        var avance = (await primero.Content.ReadFromJsonAsync<AvanceGuardadoDto>())!;
        Assert.Equal(avance.Id, (await segundo.Content.ReadFromJsonAsync<AvanceGuardadoDto>())!.Id);
        Assert.Equal(1, await api.EnBaseDatosAsync(db => db.Avances.CountAsync(a => a.ProyectoId == id)));
        Assert.True(await api.TieneNotificacionAsync(e.Coordinador.Id, "AvanceRegistrado", id));

        // HU-20 CA-1: aparece en la bandeja del Coordinador.
        var bandeja = await e.CliCoordinador.GetFromJsonAsync<BandejaAvancesDto>($"/api/v1/avances/pendientes?proyectoId={id}");
        Assert.Equal("Coordinador", Assert.Single(bandeja!.Items).RolValidador);

        // CA-3: rechazar sin observaciones no se permite.
        Assert.Equal(HttpStatusCode.BadRequest,
            (await e.CliCoordinador.PostAsJsonAsync($"/api/v1/avances/{avance.Id}/validaciones", new { resultado = "Rechazado" })).StatusCode);

        // Rechazo con observaciones → el autor corrige y vuelve a Pendiente (CA-4) conservando el historial.
        Assert.Equal(HttpStatusCode.NoContent, (await e.CliCoordinador.PostAsJsonAsync($"/api/v1/avances/{avance.Id}/validaciones",
            new { resultado = "Rechazado", observaciones = "Falta la evidencia del laboratorio" })).StatusCode);
        Assert.True(await api.TieneNotificacionAsync(e.Ip.Id, "AvanceRechazado", id));
        Assert.Equal(HttpStatusCode.NoContent,
            (await e.CliIp.PutAsJsonAsync($"/api/v1/avances/{avance.Id}", new { titulo = "Avance 1 corregido", porcentajeAvance = 25m })).StatusCode);
        Assert.Equal("Pendiente", await api.EnBaseDatosAsync(db => db.Avances.Where(a => a.Id == avance.Id).Select(a => a.EstadoValidacion).SingleAsync()));

        // CA-2: la validación del Coordinador actualiza el porcentaje del proyecto.
        Assert.Equal(HttpStatusCode.NoContent,
            (await e.CliCoordinador.PostAsJsonAsync($"/api/v1/avances/{avance.Id}/validaciones", new { resultado = "Validado" })).StatusCode);
        Assert.Equal(25m, (await e.CliIp.FichaAsync(id)).Proyecto.PorcentajeAvance);
        Assert.Equal(2, await api.EnBaseDatosAsync(db => db.AvanceValidaciones.CountAsync(v => v.AvanceId == avance.Id)));

        // HU-19 CA-2: no puede ser menor que el último validado.
        var menor = await e.CliIp.PostAsJsonAsync($"/api/v1/proyectos/{id}/avances", new { titulo = "Avance 2", porcentajeAvance = 10m });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, menor.StatusCode);
    }

    [Fact]
    public async Task HU19_adjuntos_validos_y_descarga_firmada()
    {
        var e = await api.EscenarioAsync();
        var id = await e.PropuestaCompletaAsync();
        await api.ConCronogramaAsync(id, 6);
        var creado = await e.CliIp.PostAsJsonAsync($"/api/v1/proyectos/{id}/avances", new { titulo = "Avance con evidencia", porcentajeAvance = 5m });
        var avanceId = (await creado.Content.ReadFromJsonAsync<AvanceGuardadoDto>())!.Id;

        var invalido = new MultipartFormDataContent { { new ByteArrayContent([1, 2, 3]), "archivo", "virus.exe" } };
        Assert.Equal(HttpStatusCode.UnprocessableEntity, (await e.CliIp.PostAsync($"/api/v1/avances/{avanceId}/adjuntos", invalido)).StatusCode);

        var valido = new MultipartFormDataContent { { new ByteArrayContent("a;b\n1;2"u8.ToArray()), "archivo", "datos.csv" } };
        var subido = await e.CliIp.PostAsync($"/api/v1/avances/{avanceId}/adjuntos", valido);
        Assert.Equal(HttpStatusCode.Created, subido.StatusCode);
        var adjuntoId = (await subido.Content.ReadFromJsonAsync<CreadoDto>())!.Id;

        var enlace = await e.CliCoordinador.GetFromJsonAsync<EnlaceDto>($"/api/v1/avances/adjuntos/{adjuntoId}/descarga");
        var archivo = await api.CreateClient().GetAsync(enlace!.Url);
        Assert.Equal(HttpStatusCode.OK, archivo.StatusCode);
        Assert.Equal("a;b\n1;2", await archivo.Content.ReadAsStringAsync());

        var otro = await api.EscenarioAsync();
        Assert.Equal(HttpStatusCode.Forbidden, (await otro.CliIp.GetAsync($"/api/v1/avances/adjuntos/{adjuntoId}/descarga")).StatusCode);
    }

    // ----- HU-24 -----

    [Fact]
    public async Task HU24_tablero_solo_director_y_admin_y_concilia_con_el_listado()
    {
        var e = await api.EscenarioAsync();
        Assert.Equal(HttpStatusCode.Forbidden, (await e.CliCoordinador.GetAsync("/api/v1/dashboard/centro")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await e.CliIp.GetAsync("/api/v1/dashboard/centro")).StatusCode);

        var anio = DateTime.UtcNow.Year;
        var tablero = await e.CliDirector.GetFromJsonAsync<TableroDto>($"/api/v1/dashboard/centro?anio={anio}");
        // CA-2: la tarjeta coincide con el listado filtrado equivalente.
        var listado = await e.CliDirector.GetFromJsonAsync<PaginaProyectosDto>($"/api/v1/proyectos?estadoId=1&anio={anio}");
        Assert.Equal(listado!.Total, tablero!.Tarjetas.PropuestasEnElaboracion);
        var enEjecucion = await e.CliDirector.GetFromJsonAsync<PaginaProyectosDto>($"/api/v1/proyectos?estadoId=6&anio={anio}");
        Assert.Equal(enEjecucion!.Total, tablero.Tarjetas.ProyectosEnEjecucion);
    }

    // ----- HU-25 a HU-27 -----

    [Fact]
    public async Task HU27_solo_se_publican_proyectos_aprobados_en_ejecucion_o_finalizados()
    {
        var e = await api.EscenarioAsync();
        var id = await e.PropuestaCompletaAsync();
        var codigo = (await api.EnBaseDatosAsync(db => db.Proyectos.Where(p => p.Id == id).Select(p => p.Codigo).SingleAsync()))!;
        var anonimo = api.CreateClient();

        // CA-1: en elaboración no aparece y su URL responde 404.
        Assert.Equal(HttpStatusCode.NotFound, (await anonimo.GetAsync($"/api/v1/publico/proyectos/{codigo.ToLowerInvariant()}")).StatusCode);

        await api.CambiarEstadoAsync(id, 6);
        var respuesta = await anonimo.GetAsync($"/api/v1/publico/proyectos/{codigo.ToLowerInvariant()}");
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var json = await respuesta.Content.ReadAsStringAsync();
        Assert.DoesNotContain("presupuesto", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("\"email\"", json, StringComparison.OrdinalIgnoreCase); // nunca correos
        Assert.Equal("En ejecución", (await respuesta.Content.ReadFromJsonAsync<ProyectoPublicoDetalleDto>())!.Proyecto.Estado);
    }

    [Fact]
    public async Task HU26_programa_publico_por_url_amigable()
    {
        var anonimo = api.CreateClient();
        var programas = await anonimo.GetFromJsonAsync<ProgramaPublicoDto[]>("/api/v1/publico/programas");
        var ciudades = Assert.Single(programas!, p => p.Nombre == "Ciudades Inteligentes");
        Assert.Equal("ciudades-inteligentes", ciudades.Slug);
        Assert.Equal(HttpStatusCode.OK, (await anonimo.GetAsync("/api/v1/publico/programas/ciudades-inteligentes")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await anonimo.GetAsync("/api/v1/publico/programas/no-existe")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await anonimo.GetAsync("/api/v1/publico/resumen")).StatusCode);
    }

    // ----- HU-14 -----

    [Fact]
    public async Task HU14_la_carga_inicial_es_todo_o_nada_y_reporta_errores_por_fila()
    {
        var e = await api.EscenarioAsync();
        var programa = await api.EnBaseDatosAsync(db => db.Programas.Where(p => p.Id == e.ProgramaId).Select(p => p.Nombre).SingleAsync());
        var encabezado = string.Join(";", ImportadorProyectos.Columnas);
        string Fila(string correo, string titulo) =>
            $";{titulo};Resumen importado;{programa};I+D+i;En ejecución;{correo};2026-01-15;2026-12-15;COP;50000000;;40;;Convocatoria:Efectivo:COP:50000000::Comprometido;;;;";

        var titulo = "Proyecto importado " + Guid.NewGuid().ToString("N")[..6];
        var conError = $"{encabezado}\n{Fila(e.Ip.Email, titulo)}\n{Fila("noexiste@pruebas.local", titulo + " B")}\n";

        using (var scope = api.Services.CreateScope())
        {
            var importador = scope.ServiceProvider.GetRequiredService<ImportadorProyectos>();
            // CA-2: error en la fila 3 y no se importa nada.
            var fallido = await importador.ImportarAsync(conError, confirmar: true);
            Assert.False(fallido.Valido);
            Assert.Contains("Fila 3: el usuario con correo noexiste@pruebas.local no existe", Assert.Single(fallido.Errores));
        }
        Assert.False(await api.EnBaseDatosAsync(db => db.Proyectos.AnyAsync(p => p.Titulo == titulo)));

        using (var scope = api.Services.CreateScope())
        {
            // CA-1: archivo válido → se crean proyecto, IP como integrante y fuente; queda marcado en el log.
            var correcto = await scope.ServiceProvider.GetRequiredService<ImportadorProyectos>()
                .ImportarAsync($"{encabezado}\n{Fila(e.Ip.Email, titulo)}\n", confirmar: true);
            Assert.True(correcto.Importado);
            Assert.Equal((1, 1, 1), (correcto.Proyectos, correcto.Integrantes, correcto.Fuentes));
        }
        var proyecto = await api.EnBaseDatosAsync(db => db.Proyectos.SingleAsync(p => p.Titulo == titulo));
        Assert.Equal(6, proyecto.EstadoId);
        Assert.StartsWith("CI2DT2-2026-", proyecto.Codigo);
        Assert.True(await api.EnBaseDatosAsync(db => db.LogActividades.AnyAsync(l => l.Accion == "IMPORT" && l.RecursoId == proyecto.Codigo)));
    }
}

