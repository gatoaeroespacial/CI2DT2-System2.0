using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Ci2dt2.Application.Notificaciones;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ci2dt2.IntegrationTests;

public sealed record ConvocatoriaDto(int Id, string Nombre, string Estado);
public sealed record PaginaConvocatoriasDto(ConvocatoriaDto[] Items, int Total);
public sealed record HitoGuardadoDto(int Id, string? Advertencia);
public sealed record NotificacionDto(int Id, string Tipo, string Titulo, bool Leida, string? Ruta);
public sealed record PaginaNotificacionesDto(NotificacionDto[] Items, int Total);
public sealed record BandejaDto(PaginaNotificacionesDto Pagina, int NoLeidas);
public sealed record ConsentimientoDto(int? PoliticaId, string? Version, bool Aceptado);

/// <summary>Sprint 3: HU-03, HU-11, HU-12, HU-15, HU-16, HU-17, HU-22, HU-23 y consentimiento de datos.</summary>
[Collection(ColeccionApi.Nombre)]
public class Sprint3Tests(ApiFactory api)
{
    // ----- HU-11 -----

    [Fact]
    public async Task HU11_CA6_con_requisitos_incompletos_no_se_puede_enviar_y_se_listan_los_pendientes()
    {
        var e = await api.EscenarioAsync();
        var id = await e.CliIp.CrearPropuestaAsync(e.ProgramaId);

        var ficha = await e.CliIp.FichaAsync(id);
        // El IP puede enviar, pero el frontend deshabilita el botón mientras haya pendientes.
        Assert.True(ficha.Flujo.PuedeEnviarRevision);
        Assert.Contains(ficha.Flujo.Requisitos, r => !r.Cumple);

        var envio = await e.CliIp.PostAsync($"/api/v1/proyectos/{id}/enviar-revision", null);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, envio.StatusCode);
        Assert.Contains("requisitos pendientes", (await envio.Content.ReadFromJsonAsync<ErrorDto>())!.Error);
    }

    [Fact]
    public async Task HU11_flujo_completo_envio_devolucion_reenvio_aprobacion_y_aval()
    {
        var e = await api.EscenarioAsync();
        var id = await e.PropuestaCompletaAsync();

        // CA-1: envío → En revisión (etapa Coordinador), se notifica al Coordinador y el IP ya no edita.
        Assert.Equal(HttpStatusCode.NoContent, (await e.CliIp.PostAsync($"/api/v1/proyectos/{id}/enviar-revision", null)).StatusCode);
        var ficha = await e.CliIp.FichaAsync(id);
        Assert.Equal(("En revisión", "Coordinador"), (ficha.Proyecto.Estado, ficha.Flujo.EtapaRevision));
        Assert.True(await api.TieneNotificacionAsync(e.Coordinador.Id, "PropuestaEnRevision", id));
        var edicion = await e.CliIp.PutAsJsonAsync($"/api/v1/proyectos/{id}", new { titulo = "Título modificado en revisión", tipoProyectoId = 1 });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, edicion.StatusCode);

        // CA-5: devolver sin observaciones (o con menos de 20 caracteres) no se permite.
        Assert.Equal(HttpStatusCode.BadRequest, (await e.CliCoordinador.RevisarAsync(id, "Devuelve")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await e.CliCoordinador.RevisarAsync(id, "Devuelve", "Muy corta")).StatusCode);

        // CA-2: devolución con observaciones → Devuelto y notificación al IP.
        Assert.Equal(HttpStatusCode.NoContent,
            (await e.CliCoordinador.RevisarAsync(id, "Devuelve", "Ajustar el presupuesto y los objetivos específicos")).StatusCode);
        Assert.Equal("Devuelto", (await e.CliIp.FichaAsync(id)).Proyecto.Estado);
        Assert.True(await api.TieneNotificacionAsync(e.Ip.Id, "PropuestaDevuelta", id));

        // Reenvío → vuelve a En revisión, etapa Coordinador.
        Assert.Equal(HttpStatusCode.NoContent, (await e.CliIp.PostAsync($"/api/v1/proyectos/{id}/enviar-revision", null)).StatusCode);
        Assert.Equal("Coordinador", (await e.CliIp.FichaAsync(id)).Flujo.EtapaRevision);

        // El Coordinador no puede avalar.
        Assert.Equal(HttpStatusCode.UnprocessableEntity, (await e.CliCoordinador.RevisarAsync(id, "Avala")).StatusCode);

        // CA-3: aprobación del Coordinador → etapa Director, sigue En revisión y se notifica al Director.
        Assert.Equal(HttpStatusCode.NoContent, (await e.CliCoordinador.RevisarAsync(id, "Aprueba")).StatusCode);
        ficha = await e.CliDirector.FichaAsync(id);
        Assert.Equal(("En revisión", "Director"), (ficha.Proyecto.Estado, ficha.Flujo.EtapaRevision));
        Assert.Contains("Avala", ficha.Flujo.DecisionesDisponibles);
        Assert.True(await api.TieneNotificacionAsync(e.Director.Id, "PropuestaAprobadaCoordinador", id));

        // En la etapa Director el Coordinador ya no decide.
        Assert.Equal(HttpStatusCode.Forbidden, (await e.CliCoordinador.RevisarAsync(id, "Aprueba")).StatusCode);

        // CA-4: aval del Director → Presentado; se notifica al IP y al Coordinador.
        Assert.Equal(HttpStatusCode.NoContent, (await e.CliDirector.RevisarAsync(id, "Avala")).StatusCode);
        ficha = await e.CliDirector.FichaAsync(id);
        Assert.Equal("Presentado", ficha.Proyecto.Estado);
        Assert.True(await api.TieneNotificacionAsync(e.Ip.Id, "PropuestaAvalada", id));
        Assert.True(await api.TieneNotificacionAsync(e.Coordinador.Id, "PropuestaAvalada", id));

        // CA-7: historial cronológico con todas las decisiones.
        Assert.Equal(3, ficha.Proyecto.Revisiones.Length);
        var decisiones = await api.EnBaseDatosAsync(db => db.ProyectoRevisiones.Where(r => r.ProyectoId == id).OrderBy(r => r.Id)
            .Select(r => r.Decision).ToListAsync());
        Assert.Equal(["Devuelve", "Aprueba", "Avala"], decisiones);
    }

    [Fact]
    public async Task HU11_un_coordinador_de_otro_programa_no_puede_revisar()
    {
        var e = await api.EscenarioAsync();
        var id = await e.PropuestaCompletaAsync();
        await e.CliIp.PostAsync($"/api/v1/proyectos/{id}/enviar-revision", null);

        var otro = await api.EscenarioAsync();
        Assert.Equal(HttpStatusCode.Forbidden, (await otro.CliCoordinador.RevisarAsync(id, "Aprueba")).StatusCode);
    }

    // ----- HU-12 -----

    [Fact]
    public async Task HU12_aprobacion_exige_acto_administrativo_crea_cronograma_e_inicio_exige_plan()
    {
        var e = await api.EscenarioAsync();
        var id = await e.PropuestaCompletaAsync();

        // CA-5: transición no válida (En elaboración → En ejecución).
        var invalida = await e.CliDirector.PostAsJsonAsync($"/api/v1/proyectos/{id}/iniciar-ejecucion", new { fechaInicio = "2027-02-01" });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, invalida.StatusCode);
        Assert.Equal("Transición de estado no permitida.", (await invalida.Content.ReadFromJsonAsync<ErrorDto>())!.Error);

        await api.CambiarEstadoAsync(id, 4); // Presentado

        // CA-2: sin acto administrativo → 422.
        var sinActo = await e.CliDirector.PostAsJsonAsync($"/api/v1/proyectos/{id}/aprobacion", new { aprobado = true });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, sinActo.StatusCode);

        // El Coordinador no registra aprobaciones.
        Assert.Equal(HttpStatusCode.Forbidden,
            (await e.CliCoordinador.PostAsJsonAsync($"/api/v1/proyectos/{id}/aprobacion", new { aprobado = true })).StatusCode);

        // CA-1 y CA-4: con el acto → Aprobado, cronograma creado, presupuesto ajustado y notificación al IP.
        Assert.Equal(HttpStatusCode.Created, (await e.CliDirector.SubirDocumentoAsync("Proyecto", id, 8, "acto.pdf")).StatusCode);
        var aprobacion = await e.CliDirector.PostAsJsonAsync($"/api/v1/proyectos/{id}/aprobacion", new { aprobado = true, presupuestoAprobado = 90_000_000m });
        Assert.Equal(HttpStatusCode.NoContent, aprobacion.StatusCode);
        var ficha = await e.CliDirector.FichaAsync(id);
        Assert.Equal(("Aprobado", 90_000_000m), (ficha.Proyecto.Estado, ficha.Proyecto.PresupuestoTotal));
        Assert.True(await api.EnBaseDatosAsync(db => db.Cronogramas.AnyAsync(c => c.ProyectoId == id)));
        Assert.True(await api.TieneNotificacionAsync(e.Ip.Id, "ProyectoAprobado", id));

        // CA-3: iniciar la ejecución exige el plan de trabajo.
        var sinPlan = await e.CliDirector.PostAsJsonAsync($"/api/v1/proyectos/{id}/iniciar-ejecucion", new { fechaInicio = "2027-02-01" });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, sinPlan.StatusCode);
        await e.CliDirector.SubirDocumentoAsync("Proyecto", id, 2, "plan.pdf");
        Assert.Equal(HttpStatusCode.NoContent,
            (await e.CliDirector.PostAsJsonAsync($"/api/v1/proyectos/{id}/iniciar-ejecucion", new { fechaInicio = "2027-02-01" })).StatusCode);
        Assert.Equal("En ejecución", (await e.CliDirector.FichaAsync(id)).Proyecto.Estado);
    }

    [Fact]
    public async Task HU12_no_aprobado_es_estado_final()
    {
        var e = await api.EscenarioAsync();
        var id = await e.PropuestaCompletaAsync();
        await api.CambiarEstadoAsync(id, 4);

        Assert.Equal(HttpStatusCode.NoContent,
            (await e.CliDirector.PostAsJsonAsync($"/api/v1/proyectos/{id}/aprobacion", new { aprobado = false })).StatusCode);
        Assert.Equal("No aprobado", (await e.CliDirector.FichaAsync(id)).Proyecto.Estado);
        Assert.Equal(HttpStatusCode.UnprocessableEntity,
            (await e.CliDirector.PostAsJsonAsync($"/api/v1/proyectos/{id}/aprobacion", new { aprobado = true })).StatusCode);
    }

    [Fact]
    public async Task HU17_CA3_un_reto_empresarial_sin_los_hitos_fijos_no_inicia_ejecucion()
    {
        var e = await api.EscenarioAsync();
        var id = await e.PropuestaCompletaAsync();
        await api.EnBaseDatosAsync(db => db.Proyectos.Where(p => p.Id == id).ExecuteUpdateAsync(s => s.SetProperty(p => p.TipoProyectoId, 2)));
        var cronograma = await api.ConCronogramaAsync(id, 5);
        await e.CliDirector.SubirDocumentoAsync("Proyecto", id, 2, "plan.pdf");
        await api.HitoAsync(cronograma, e.Ip.Id, new DateOnly(2027, 3, 1), tipoHitoId: 5);
        await api.HitoAsync(cronograma, e.Ip.Id, new DateOnly(2027, 4, 1), tipoHitoId: 5);

        var respuesta = await e.CliDirector.PostAsJsonAsync($"/api/v1/proyectos/{id}/iniciar-ejecucion", new { fechaInicio = "2027-02-01" });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, respuesta.StatusCode);
        var error = (await respuesta.Content.ReadFromJsonAsync<ErrorDto>())!.Error;
        Assert.Contains("1 hito(s) de tipo Sprint", error);
        Assert.Contains("Entrega de MVP", error);
    }

    // ----- HU-15 y HU-16 -----

    [Fact]
    public async Task HU15_estado_calculado_validacion_de_fechas_y_permisos()
    {
        var e = await api.EscenarioAsync();
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(-5));
        var invalida = await e.CliCoordinador.PostAsJsonAsync("/api/v1/convocatorias", new
        {
            nombre = "Convocatoria con fechas invertidas",
            tipoConvocatoriaId = 1,
            plataforma = "VIP",
            fechaApertura = hoy.AddDays(10).ToString("yyyy-MM-dd"),
            fechaCierre = hoy.ToString("yyyy-MM-dd"),
        });
        Assert.Equal(HttpStatusCode.BadRequest, invalida.StatusCode);

        var nombre = "Convocatoria abierta " + Guid.NewGuid().ToString("N")[..6];
        var creada = await e.CliCoordinador.PostAsJsonAsync("/api/v1/convocatorias", new
        {
            nombre,
            tipoConvocatoriaId = 1,
            plataforma = "VIP",
            fechaApertura = hoy.AddDays(-1).ToString("yyyy-MM-dd"),
            fechaCierre = hoy.AddDays(10).ToString("yyyy-MM-dd"),
        });
        Assert.Equal(HttpStatusCode.Created, creada.StatusCode);

        // El investigador consulta (para vincular su propuesta) pero no registra.
        var lista = await e.CliIp.GetFromJsonAsync<PaginaConvocatoriasDto>($"/api/v1/convocatorias?busqueda={Uri.EscapeDataString(nombre)}&estado=Abierta");
        Assert.Equal("Abierta", Assert.Single(lista!.Items).Estado);
        Assert.Equal(HttpStatusCode.Forbidden,
            (await e.CliIp.PostAsJsonAsync("/api/v1/convocatorias", new { nombre, tipoConvocatoriaId = 1, plataforma = "VIP" })).StatusCode);
    }

    [Fact]
    public async Task HU16_sgr_exige_mga_monto_maximo_y_seleccionada_notifica_al_director()
    {
        var e = await api.EscenarioAsync();
        var id = await e.PropuestaCompletaAsync();
        var sgr = await e.CliDirector.PostAsJsonAsync("/api/v1/convocatorias", new
        {
            nombre = "SGR de prueba " + Guid.NewGuid().ToString("N")[..6],
            tipoConvocatoriaId = 5,
            plataforma = "MGA Web",
            moneda = "COP",
            montoMaximo = 50_000_000m,
        });
        var convocatoriaId = (await sgr.Content.ReadFromJsonAsync<CreadoDto>())!.Id;

        // CA-1: sin código MGA → 422.
        var sinMga = await e.CliIp.PostAsJsonAsync($"/api/v1/proyectos/{id}/postulaciones", new { convocatoriaId, montoSolicitado = 10_000_000m, moneda = "COP" });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, sinMga.StatusCode);

        // CA-2: monto mayor al máximo → 422 con el máximo permitido.
        var excede = await e.CliIp.PostAsJsonAsync($"/api/v1/proyectos/{id}/postulaciones",
            new { convocatoriaId, codigoMga = "MGA-001", montoSolicitado = 60_000_000m, moneda = "COP" });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, excede.StatusCode);
        Assert.Contains("50.000.000", (await excede.Content.ReadFromJsonAsync<ErrorDto>())!.Error.Replace(",", "."));

        var creada = await e.CliIp.PostAsJsonAsync($"/api/v1/proyectos/{id}/postulaciones",
            new { convocatoriaId, codigoMga = "MGA-001", montoSolicitado = 40_000_000m, moneda = "COP", faseExterna = "Banco de elegibles" });
        Assert.Equal(HttpStatusCode.Created, creada.StatusCode);
        var postulacionId = (await creada.Content.ReadFromJsonAsync<CreadoDto>())!.Id;

        // Con convocatoria SGR los requisitos de envío incluyen el Documento Conceptual MGA.
        Assert.Contains((await e.CliIp.FichaAsync(id)).Flujo.Requisitos, r => r.Descripcion.Contains("MGA") && !r.Cumple);

        // Radicada (con fecha) exige id externo y comprobante adjunto.
        var sinComprobante = await e.CliIp.PutAsJsonAsync($"/api/v1/proyectos/{id}/postulaciones/{postulacionId}",
            new { convocatoriaId, codigoMga = "MGA-001", montoSolicitado = 40_000_000m, moneda = "COP", fechaPresentacion = "2026-11-01", idPostulacionExterna = "SIGP-77" });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, sinComprobante.StatusCode);
        await e.CliIp.SubirDocumentoAsync("PropuestaConvocatoria", postulacionId, 11, "radicado.pdf");

        // CA-3: resultado "Seleccionada" → notificación al Director.
        var seleccionada = await e.CliIp.PutAsJsonAsync($"/api/v1/proyectos/{id}/postulaciones/{postulacionId}", new
        {
            convocatoriaId,
            codigoMga = "MGA-001",
            montoSolicitado = 40_000_000m,
            moneda = "COP",
            fechaPresentacion = "2026-11-01",
            idPostulacionExterna = "SIGP-77",
            resultado = "Seleccionada",
        });
        Assert.Equal(HttpStatusCode.NoContent, seleccionada.StatusCode);
        Assert.True(await api.TieneNotificacionAsync(e.Director.Id, "PostulacionSeleccionada", id));
    }

    // ----- HU-17 -----

    [Fact]
    public async Task HU17_responsable_integrante_reprogramacion_con_justificacion_y_completar()
    {
        var e = await api.EscenarioAsync();
        var id = await e.PropuestaCompletaAsync();
        await api.ConCronogramaAsync(id, 5);
        var externo = await api.UsuarioAsync("noint", "Investigador");

        // CA-2: responsable que no pertenece al proyecto → 422.
        var ajeno = await e.CliIp.PostAsJsonAsync($"/api/v1/proyectos/{id}/hitos",
            new { tipoHitoId = 1, titulo = "Entregable 1", fechaLimite = "2027-03-01", responsableId = externo.Id });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, ajeno.StatusCode);

        // CA-1: creación con días de alerta por defecto (7) y advertencia fuera de vigencia.
        var creado = await e.CliIp.PostAsJsonAsync($"/api/v1/proyectos/{id}/hitos",
            new { tipoHitoId = 1, titulo = "Entregable 1", fechaLimite = "2028-03-01", responsableId = e.Ip.Id });
        Assert.Equal(HttpStatusCode.Created, creado.StatusCode);
        var hito = (await creado.Content.ReadFromJsonAsync<HitoGuardadoDto>())!;
        Assert.NotNull(hito.Advertencia);
        Assert.Equal(7, await api.EnBaseDatosAsync(db => db.Hitos.Where(h => h.Id == hito.Id).Select(h => h.AlertarDias).SingleAsync()));

        // CA-5: cambiar la fecha sin justificación no se permite; con justificación se notifica al Coordinador.
        var sinJustificacion = await e.CliIp.PutAsJsonAsync($"/api/v1/hitos/{hito.Id}",
            new { tipoHitoId = 1, titulo = "Entregable 1", fechaLimite = "2027-05-01", responsableId = e.Ip.Id });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, sinJustificacion.StatusCode);
        var reprogramado = await e.CliIp.PutAsJsonAsync($"/api/v1/hitos/{hito.Id}",
            new { tipoHitoId = 1, titulo = "Entregable 1", fechaLimite = "2027-05-01", responsableId = e.Ip.Id, justificacion = "Retraso del proveedor" });
        Assert.Equal(HttpStatusCode.OK, reprogramado.StatusCode);
        Assert.True(await api.TieneNotificacionAsync(e.Coordinador.Id, "HitoReprogramado", hito.Id));
        Assert.Single(await api.EsperarLogsAsync(l => l.Accion == "REPROGRAMA" && l.RecursoId == hito.Id.ToString()));

        // CA-4: completar registra la fecha del día.
        Assert.Equal(HttpStatusCode.NoContent, (await e.CliIp.PostAsJsonAsync($"/api/v1/hitos/{hito.Id}/completar", new { })).StatusCode);
        var completado = await api.EnBaseDatosAsync(db => db.Hitos.SingleAsync(h => h.Id == hito.Id));
        Assert.Equal("Completado", completado.Estado);
        Assert.NotNull(completado.FechaCompletado);
    }

    [Fact]
    public async Task HU17_un_investigador_que_no_es_ip_no_gestiona_hitos()
    {
        var e = await api.EscenarioAsync();
        var id = await e.PropuestaCompletaAsync();
        await api.ConCronogramaAsync(id, 5);
        var coinv = await api.UsuarioAsync("coinv", "Investigador");
        await e.CliIp.PostAsJsonAsync($"/api/v1/proyectos/{id}/integrantes", new { usuarioId = coinv.Id, rolEnProyecto = "Co-investigador" });
        var cliente = await api.ClienteDeAsync(coinv);

        var respuesta = await cliente.PostAsJsonAsync($"/api/v1/proyectos/{id}/hitos",
            new { tipoHitoId = 1, titulo = "Hito no autorizado", fechaLimite = "2027-03-01", responsableId = coinv.Id });
        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
    }

    // ----- HU-22 y HU-23 -----

    [Fact]
    public async Task HU22_bandeja_contador_lectura_y_HU23_envio_por_correo_con_enlace()
    {
        var e = await api.EscenarioAsync();
        var id = await e.CliIp.CrearPropuestaAsync(e.ProgramaId); // Notifica al Coordinador.

        var bandeja = await e.CliCoordinador.GetFromJsonAsync<BandejaDto>("/api/v1/notificaciones/mias");
        var notificacion = Assert.Single(bandeja!.Pagina.Items, n => n.Tipo == "PropuestaRegistrada");
        Assert.Equal($"/proyectos/{id}", notificacion.Ruta);
        Assert.True(bandeja.NoLeidas >= 1);

        // HU-23: el despachador envía el correo con la plantilla y el enlace directo, y marca el envío.
        using (var scope = api.Services.CreateScope())
            await scope.ServiceProvider.GetRequiredService<DespachadorCorreos>().ProcesarPendientesAsync();
        var correo = api.Correos.UltimoPara(e.Coordinador.Email);
        Assert.NotNull(correo);
        Assert.Contains($"https://app.pruebas.local/proyectos/{id}", correo!.Cuerpo);
        Assert.True(await api.EnBaseDatosAsync(db => db.Notificaciones.Where(n => n.Id == notificacion.Id).Select(n => n.EnviadaEmail).SingleAsync()));

        // CA-2 y CA-3: leer una y luego todas.
        Assert.Equal(HttpStatusCode.NoContent, (await e.CliCoordinador.PostAsync($"/api/v1/notificaciones/{notificacion.Id}/leer", null)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await e.CliCoordinador.PostAsync("/api/v1/notificaciones/leer-todas", null)).StatusCode);
        Assert.Equal(0, (await e.CliCoordinador.GetFromJsonAsync<BandejaDto>("/api/v1/notificaciones/mias"))!.NoLeidas);

        // Cada usuario solo ve (y marca) las suyas.
        Assert.Equal(HttpStatusCode.NotFound, (await e.CliIp.PostAsync($"/api/v1/notificaciones/{notificacion.Id}/leer", null)).StatusCode);
    }

    [Fact]
    public void HU23_reintentos_con_espera_creciente()
    {
        var ahora = new DateTime(2026, 11, 7, 12, 0, 0, DateTimeKind.Utc);
        Assert.True(DespachadorCorreos.ListaParaIntento(0, null, ahora));
        Assert.False(DespachadorCorreos.ListaParaIntento(1, ahora.AddSeconds(-30), ahora));
        Assert.True(DespachadorCorreos.ListaParaIntento(1, ahora.AddMinutes(-1), ahora));
        Assert.False(DespachadorCorreos.ListaParaIntento(2, ahora.AddMinutes(-1), ahora));
        Assert.True(DespachadorCorreos.ListaParaIntento(2, ahora.AddMinutes(-2), ahora));
    }

    // ----- HU-03 -----

    [Fact]
    public async Task HU03_restablecer_contrasena_un_solo_uso_y_respuesta_identica()
    {
        var usuario = await api.UsuarioAsync("reset", "Investigador");

        // CA-1: misma respuesta exista o no el correo.
        var existente = await api.CreateClient().PostAsJsonAsync("/api/v1/auth/forgot-password", new { email = usuario.Email });
        var inexistente = await api.CreateClient().PostAsJsonAsync("/api/v1/auth/forgot-password", new { email = "nadie@pruebas.local" });
        Assert.Equal(HttpStatusCode.OK, existente.StatusCode);
        Assert.Equal(await existente.Content.ReadAsStringAsync(), await inexistente.Content.ReadAsStringAsync());

        // El correo sale por una cola en segundo plano.
        CorreoEnviado? correo = null;
        for (var i = 0; i < 50 && correo is null; i++)
        {
            await Task.Delay(100);
            correo = api.Correos.UltimoPara(usuario.Email);
        }
        Assert.NotNull(correo);
        var token = Uri.UnescapeDataString(Regex.Match(correo!.Cuerpo, @"token=([^""&]+)").Groups[1].Value.Replace("&amp;", "&"));
        Assert.True((await api.CreateClient().GetFromJsonAsync<ValidezDto>($"/api/v1/auth/reset-password/validar?token={Uri.EscapeDataString(token)}"))!.Valido);

        // CA-4: la política se valida en el servidor.
        var debil = await api.CreateClient().PostAsJsonAsync("/api/v1/auth/reset-password", new { token, contrasena = "debil" });
        Assert.Equal(HttpStatusCode.BadRequest, debil.StatusCode);

        // CA-2: nueva contraseña y revocación de sesiones.
        var sesion = await api.IniciarSesionAsync(usuario.Email);
        Assert.Equal(HttpStatusCode.NoContent,
            (await api.CreateClient().PostAsJsonAsync("/api/v1/auth/reset-password", new { token, contrasena = "NuevaClave2026" })).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await api.RefrescarAsync(sesion.CookieRefresco)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await api.LoginAsync(usuario.Email, "NuevaClave2026")).StatusCode);

        // CA-3: el enlace ya usado deja de servir.
        var reuso = await api.CreateClient().PostAsJsonAsync("/api/v1/auth/reset-password", new { token, contrasena = "OtraClave2026x" });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, reuso.StatusCode);
        Assert.False((await api.CreateClient().GetFromJsonAsync<ValidezDto>($"/api/v1/auth/reset-password/validar?token={Uri.EscapeDataString(token)}"))!.Valido);
    }

    // ----- Política de datos -----

    [Fact]
    public async Task Consentimiento_de_la_politica_de_datos_vigente()
    {
        var usuario = await api.UsuarioAsync("consent", "Investigador");
        var cliente = await api.ClienteDeAsync(usuario);

        Assert.False((await cliente.GetFromJsonAsync<ConsentimientoDto>("/api/v1/consentimientos/estado"))!.Aceptado);
        Assert.Equal(HttpStatusCode.NoContent, (await cliente.PostAsync("/api/v1/consentimientos", null)).StatusCode);
        Assert.True((await cliente.GetFromJsonAsync<ConsentimientoDto>("/api/v1/consentimientos/estado"))!.Aceptado);
        Assert.Equal(1, await api.EnBaseDatosAsync(db => db.ConsentimientosDatos.CountAsync(c => c.UsuarioId == usuario.Id)));
    }
}

public sealed record ValidezDto(bool Valido);
