using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Ci2dt2.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ci2dt2.IntegrationTests;

public sealed record ProyectoGeneralDto(
    int Id, string Codigo, string Titulo, int ProgramaId, int EstadoId, string Estado, int InvestigadorPrincipalId,
    string? Moneda, decimal? PresupuestoTotal, decimal? TasaCambio, decimal? PresupuestoTotalCop,
    HitoDto[] Cronograma, PostulacionDto[] Convocatorias, AvanceDto[] Avances, RevisionDto[] Revisiones);
public sealed record HitoDto(int Id, string Titulo);
public sealed record PostulacionDto(int Id, string Convocatoria);
public sealed record AvanceDto(int Id, string Titulo);
public sealed record RevisionDto(int Id, string Etapa);
public sealed record PermisosDto(bool PuedeEditar, bool PuedeCambiarIp, bool PuedeGestionarDocumentos);
public sealed record FichaDto(ProyectoGeneralDto Proyecto, PermisosDto Permisos);
public sealed record IntegranteProyectoDto(int Id, int UsuarioId, string Nombre, string RolEnProyecto, DateTime? FechaFin, bool Vigente, string? CvLacUrl, string? Orcid);
public sealed record TotalDto(string Categoria, decimal TotalCop);
public sealed record FuenteDto(int Id, string TipoFuente, string TipoAporte, string Moneda, decimal Monto, decimal? MontoCop);
public sealed record FinanciacionDto(FuenteDto[] Fuentes, TotalDto[] PorTipoFuente, TotalDto[] PorTipoAporte, decimal TotalCop, decimal? PresupuestoTotalCop, decimal? Diferencia);

/// <summary>HU-08, HU-09, HU-10 y HU-13, con las restricciones de HU-05.</summary>
[Collection(ColeccionApi.Nombre)]
public class ProyectosTests(ApiFactory api)
{
    /// <summary>Programa con un Coordinador vigente y un Investigador adscrito.</summary>
    private async Task<(int ProgramaId, Usuario Coordinador, Usuario Investigador)> EscenarioAsync()
    {
        var programaId = await api.NuevoProgramaAsync();
        var coordinador = await api.UsuarioAsync("coord", "Coordinador");
        var investigador = await api.UsuarioAsync("inv", "Investigador");
        await api.VincularAsync(coordinador.Id, programaId, "Coordinador");
        await api.VincularAsync(investigador.Id, programaId, "Investigador");
        return (programaId, coordinador, investigador);
    }

    [Fact]
    public async Task HU08_CA1_CA5_el_ip_registra_la_propuesta_en_elaboracion_con_codigo_y_queda_como_ip()
    {
        var (programaId, coordinador, investigador) = await EscenarioAsync();
        var cliente = await api.ClienteDeAsync(investigador);

        var id = await cliente.CrearPropuestaAsync(programaId);

        var ficha = await cliente.GetFromJsonAsync<FichaDto>($"/api/v1/proyectos/{id}");
        Assert.Matches(new Regex($@"^CI2DT2-{DateTime.UtcNow.Year}-\d{{3}}$"), ficha!.Proyecto.Codigo);
        Assert.Equal("En elaboración", ficha.Proyecto.Estado);
        Assert.Equal(investigador.Id, ficha.Proyecto.InvestigadorPrincipalId);
        Assert.True(ficha.Permisos.PuedeEditar);

        var equipo = await cliente.GetFromJsonAsync<IntegranteProyectoDto[]>($"/api/v1/proyectos/{id}/integrantes");
        var ip = Assert.Single(equipo!);
        Assert.Equal(("IP", investigador.Id), (ip.RolEnProyecto, ip.UsuarioId));

        // CA-5: queda en la auditoría y se notifica al Coordinador del programa.
        var registro = Assert.Single(await api.EsperarLogsAsync(l => l.Recurso == "proyectos" && l.Accion == "POST" && l.RecursoId == id.ToString()));
        Assert.Equal(201, registro.EstadoHttp);
        Assert.True(await api.EnBaseDatosAsync(db => db.Notificaciones.AnyAsync(n => n.UsuarioId == coordinador.Id && n.ReferenciaId == id)));
    }

    [Fact]
    public async Task HU08_los_codigos_son_consecutivos_y_unicos()
    {
        var (programaId, _, investigador) = await EscenarioAsync();
        var cliente = await api.ClienteDeAsync(investigador);

        var a = await cliente.CrearPropuestaAsync(programaId);
        var b = await cliente.CrearPropuestaAsync(programaId);

        var codigos = await api.EnBaseDatosAsync(db => db.Proyectos.Where(p => p.Id == a || p.Id == b).OrderBy(p => p.Id).Select(p => p.Codigo!).ToListAsync());
        Assert.Equal(int.Parse(codigos[0][^3..]) + 1, int.Parse(codigos[1][^3..]));
    }

    [Fact]
    public async Task HU08_CA2_diez_mil_usd_a_tasa_4000_son_40_millones_de_pesos()
    {
        var (programaId, _, investigador) = await EscenarioAsync();
        var cliente = await api.ClienteDeAsync(investigador);

        var respuesta = await cliente.PostAsJsonAsync("/api/v1/proyectos", new
        {
            titulo = "Propuesta con presupuesto en dólares",
            programaId,
            tipoProyectoId = 1,
            moneda = "USD",
            presupuestoTotal = 10_000,
            tasaCambio = 4_000,
        });
        var id = (await respuesta.Content.ReadFromJsonAsync<CreadoDto>())!.Id;

        var ficha = await cliente.GetFromJsonAsync<FichaDto>($"/api/v1/proyectos/{id}");
        Assert.Equal(40_000_000m, ficha!.Proyecto.PresupuestoTotalCop);

        // Sin tasa de cambio para una moneda distinta de COP: 400.
        var sinTasa = await cliente.PostAsJsonAsync("/api/v1/proyectos", new
        {
            titulo = "Propuesta sin tasa de cambio",
            programaId,
            tipoProyectoId = 1,
            moneda = "EUR",
            presupuestoTotal = 5_000,
        });
        Assert.Equal(HttpStatusCode.BadRequest, sinTasa.StatusCode);
    }

    [Fact]
    public async Task HU08_CA3_un_investigador_no_adscrito_recibe_422()
    {
        var (_, _, investigador) = await EscenarioAsync();
        var otroPrograma = await api.NuevoProgramaAsync();
        var cliente = await api.ClienteDeAsync(investigador);

        var respuesta = await cliente.PostAsJsonAsync("/api/v1/proyectos", new
        {
            titulo = "Propuesta en programa ajeno",
            programaId = otroPrograma,
            tipoProyectoId = 1,
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, respuesta.StatusCode);
    }

    [Fact]
    public async Task HU08_el_coordinador_crea_designando_un_ip_adscrito()
    {
        var (programaId, coordinador, investigador) = await EscenarioAsync();
        var noAdscrito = await api.UsuarioAsync("noads", "Investigador");
        var cliente = await api.ClienteDeAsync(coordinador);

        var id = await cliente.CrearPropuestaAsync(programaId, investigador.Id);
        Assert.Equal(investigador.Id, await api.EnBaseDatosAsync(db => db.Proyectos.Where(p => p.Id == id).Select(p => p.InvestigadorPrincipalId).SingleAsync()));

        var conNoAdscrito = await cliente.PostAsJsonAsync("/api/v1/proyectos", new
        {
            titulo = "Propuesta con IP no adscrito",
            programaId,
            tipoProyectoId = 1,
            investigadorPrincipalId = noAdscrito.Id,
        });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, conNoAdscrito.StatusCode);

        var externo = await api.ClienteComoAsync("Externo");
        Assert.Equal(HttpStatusCode.Forbidden, (await externo.PostAsJsonAsync("/api/v1/proyectos", new
        {
            titulo = "Propuesta de un externo",
            programaId,
            tipoProyectoId = 1,
        })).StatusCode);
    }

    [Fact]
    public async Task HU08_el_ip_edita_solo_en_elaboracion_o_devuelto_y_el_coordinador_despues()
    {
        var (programaId, coordinador, investigador) = await EscenarioAsync();
        var ip = await api.ClienteDeAsync(investigador);
        var coord = await api.ClienteDeAsync(coordinador);
        var id = await ip.CrearPropuestaAsync(programaId);
        var edicion = new { titulo = "Título actualizado de la propuesta", tipoProyectoId = 2 };

        Assert.Equal(HttpStatusCode.NoContent, (await ip.PutAsJsonAsync($"/api/v1/proyectos/{id}", edicion)).StatusCode);

        await api.CambiarEstadoAsync(id, 2); // En revisión
        Assert.Equal(HttpStatusCode.UnprocessableEntity, (await ip.PutAsJsonAsync($"/api/v1/proyectos/{id}", edicion)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await coord.PutAsJsonAsync($"/api/v1/proyectos/{id}", edicion)).StatusCode);

        await api.CambiarEstadoAsync(id, 3); // Devuelto
        Assert.Equal(HttpStatusCode.NoContent, (await ip.PutAsJsonAsync($"/api/v1/proyectos/{id}", edicion)).StatusCode);

        await api.CambiarEstadoAsync(id, 7); // Finalizado (estado final)
        Assert.Equal(HttpStatusCode.UnprocessableEntity, (await coord.PutAsJsonAsync($"/api/v1/proyectos/{id}", edicion)).StatusCode);
    }

    [Fact]
    public async Task HU05_CA1_CA2_coordinador_de_otro_programa_recibe_403_e_investigador_ajeno_404()
    {
        var (programaId, _, investigador) = await EscenarioAsync();
        var (_, coordinadorOtro, investigadorOtro) = await EscenarioAsync();
        var id = await (await api.ClienteDeAsync(investigador)).CrearPropuestaAsync(programaId);

        var coordOtro = await api.ClienteDeAsync(coordinadorOtro);
        var editar = await coordOtro.PutAsJsonAsync($"/api/v1/proyectos/{id}", new { titulo = "Intento desde otro programa", tipoProyectoId = 1 });
        Assert.Equal(HttpStatusCode.Forbidden, editar.StatusCode);
        Assert.Single(await api.EsperarLogsAsync(l => l.UsuarioId == coordinadorOtro.Id && l.EstadoHttp == 403 && l.Recurso == "proyectos"));

        var invOtro = await api.ClienteDeAsync(investigadorOtro);
        Assert.Equal(HttpStatusCode.NotFound, (await invOtro.GetAsync($"/api/v1/proyectos/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await coordOtro.GetAsync($"/api/v1/proyectos/{id}")).StatusCode);
    }

    [Fact]
    public async Task HU09_CA1_CA2_CA4_el_ip_conforma_el_equipo()
    {
        var (programaId, _, investigador) = await EscenarioAsync();
        var coinvestigador = await api.UsuarioAsync("coinv", "Investigador");
        await api.EnBaseDatosAsync(db => db.Usuarios.Where(u => u.Id == coinvestigador.Id).ExecuteUpdateAsync(s => s
            .SetProperty(u => u.Orcid, "0000-0002-1825-0097")
            .SetProperty(u => u.CvLacUrl, "https://scienti.minciencias.gov.co/cvlac/x")));
        var ip = await api.ClienteDeAsync(investigador);
        var id = await ip.CrearPropuestaAsync(programaId);

        var agregar = await ip.PostAsJsonAsync($"/api/v1/proyectos/{id}/integrantes", new { usuarioId = coinvestigador.Id, rolEnProyecto = "Co-investigador" });
        Assert.Equal(HttpStatusCode.Created, agregar.StatusCode);

        // CA-1: lo ve en "Mis proyectos" y recibe una notificación.
        var mios = await (await api.ClienteDeAsync(coinvestigador)).GetFromJsonAsync<PaginaProyectosDto>("/api/v1/proyectos/mios");
        Assert.Contains(mios!.Items, p => p.Id == id);
        Assert.True(await api.EnBaseDatosAsync(db => db.Notificaciones.AnyAsync(n => n.UsuarioId == coinvestigador.Id && n.Tipo == "IntegranteProyecto")));

        // CA-2: misma vinculación vigente con el mismo rol.
        Assert.Equal(HttpStatusCode.UnprocessableEntity, (await ip.PostAsJsonAsync($"/api/v1/proyectos/{id}/integrantes",
            new { usuarioId = coinvestigador.Id, rolEnProyecto = "Co-investigador" })).StatusCode);

        // CA-4: la pestaña Equipo trae CvLAC y ORCID.
        var equipo = await ip.GetFromJsonAsync<IntegranteProyectoDto[]>($"/api/v1/proyectos/{id}/integrantes");
        var co = Assert.Single(equipo!, e => e.UsuarioId == coinvestigador.Id);
        Assert.Equal("0000-0002-1825-0097", co.Orcid);
        Assert.NotNull(co.CvLacUrl);

        // Retiro con fecha de fin: se conserva el historial.
        Assert.Equal(HttpStatusCode.NoContent, (await ip.PutAsJsonAsync($"/api/v1/proyectos/{id}/integrantes/{co.Id}",
            new { fechaFin = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd") })).StatusCode);
        equipo = await ip.GetFromJsonAsync<IntegranteProyectoDto[]>($"/api/v1/proyectos/{id}/integrantes");
        Assert.False(Assert.Single(equipo!, e => e.UsuarioId == coinvestigador.Id).Vigente);
    }

    [Fact]
    public async Task HU09_el_cambio_de_ip_solo_lo_hacen_coordinador_o_director()
    {
        var (programaId, coordinador, investigador) = await EscenarioAsync();
        var nuevoIp = await api.UsuarioAsync("nuevoip", "Investigador");
        await api.VincularAsync(nuevoIp.Id, programaId, "Investigador");
        var ip = await api.ClienteDeAsync(investigador);
        var id = await ip.CrearPropuestaAsync(programaId);

        Assert.Equal(HttpStatusCode.Forbidden, (await ip.PostAsJsonAsync($"/api/v1/proyectos/{id}/integrantes",
            new { usuarioId = nuevoIp.Id, rolEnProyecto = "IP" })).StatusCode);

        var coord = await api.ClienteDeAsync(coordinador);
        Assert.Equal(HttpStatusCode.Created, (await coord.PostAsJsonAsync($"/api/v1/proyectos/{id}/integrantes",
            new { usuarioId = nuevoIp.Id, rolEnProyecto = "IP" })).StatusCode);

        var equipo = await coord.GetFromJsonAsync<IntegranteProyectoDto[]>($"/api/v1/proyectos/{id}/integrantes");
        var ipsVigentes = equipo!.Where(e => e.RolEnProyecto == "IP" && e.Vigente).ToList();
        Assert.Equal(nuevoIp.Id, Assert.Single(ipsVigentes).UsuarioId);
        Assert.Contains(equipo!, e => e.UsuarioId == investigador.Id && e.RolEnProyecto == "IP" && !e.Vigente);
        Assert.Equal(nuevoIp.Id, await api.EnBaseDatosAsync(db => db.Proyectos.Where(p => p.Id == id).Select(p => p.InvestigadorPrincipalId).SingleAsync()));
    }

    [Fact]
    public async Task HU10_CA1_CA2_CA3_fuentes_totales_y_cofinanciacion_con_aliado()
    {
        var (programaId, _, investigador) = await EscenarioAsync();
        var ip = await api.ClienteDeAsync(investigador);
        var id = await (await ip.PostAsJsonAsync("/api/v1/proyectos", new
        {
            titulo = "Propuesta con financiación completa",
            programaId,
            tipoProyectoId = 1,
            moneda = "COP",
            presupuestoTotal = 100_000_000,
        })).Content.ReadFromJsonAsync<CreadoDto>();
        var aliadoId = await api.EnBaseDatosAsync(async db =>
        {
            var aliado = new Aliado { Nombre = "Cofinanciador", TipoAliadoId = 2, Pais = "Suiza", Email = "cof@pruebas.local" };
            db.Aliados.Add(aliado);
            await db.SaveChangesAsync();
            return aliado.Id;
        });
        var url = $"/api/v1/proyectos/{id!.Id}/fuentes";

        // CA-3: Cofinanciación sin aliado.
        Assert.Equal(HttpStatusCode.UnprocessableEntity, (await ip.PostAsJsonAsync(url, new
        {
            tipoFuente = "Cofinanciación",
            tipoAporte = "Efectivo",
            moneda = "COP",
            monto = 1000,
            estado = "Estimado",
        })).StatusCode);

        Assert.Equal(HttpStatusCode.Created, (await ip.PostAsJsonAsync(url, new
        {
            aliadoId,
            tipoFuente = "Cofinanciación",
            tipoAporte = "Efectivo",
            moneda = "USD",
            monto = 5_000,
            tasaCambio = 4_000,
            estado = "Comprometido",
        })).StatusCode);
        var contrapartida = await (await ip.PostAsJsonAsync(url, new
        {
            tipoFuente = "Contrapartida",
            tipoAporte = "Especie",
            moneda = "COP",
            monto = 30_000_000,
            estado = "Estimado",
        })).Content.ReadFromJsonAsync<CreadoDto>();

        // CA-1: totales en COP por tipo de fuente y de aporte; CA-2: diferencia frente al presupuesto.
        var financiacion = await ip.GetFromJsonAsync<FinanciacionDto>(url);
        Assert.Equal(20_000_000m, Assert.Single(financiacion!.PorTipoFuente, t => t.Categoria == "Cofinanciación").TotalCop);
        Assert.Equal(30_000_000m, Assert.Single(financiacion.PorTipoAporte, t => t.Categoria == "Especie").TotalCop);
        Assert.Equal(50_000_000m, financiacion.TotalCop);
        Assert.Equal(-50_000_000m, financiacion.Diferencia);

        Assert.Equal(HttpStatusCode.NoContent, (await ip.DeleteAsync($"{url}/{contrapartida!.Id}")).StatusCode);
        financiacion = await ip.GetFromJsonAsync<FinanciacionDto>(url);
        Assert.Equal(20_000_000m, financiacion!.TotalCop);
    }

    [Fact]
    public async Task HU13_alcance_filtros_orden_y_paginacion_del_listado()
    {
        var (programaId, coordinador, investigador) = await EscenarioAsync();
        var (otroPrograma, coordOtro, invOtro) = await EscenarioAsync();
        var ip = await api.ClienteDeAsync(investigador);
        var marca = "Lst" + Guid.NewGuid().ToString("N")[..6];
        foreach (var letra in new[] { "C", "A", "B" })
            await ip.PostAsJsonAsync("/api/v1/proyectos", new { titulo = $"{marca} proyecto {letra}", programaId, tipoProyectoId = 1 });
        await (await api.ClienteDeAsync(invOtro)).PostAsJsonAsync("/api/v1/proyectos", new { titulo = $"{marca} ajeno", programaId = otroPrograma, tipoProyectoId = 1 });

        var director = await api.ClienteComoAsync("Director");
        var todos = await director.GetFromJsonAsync<PaginaProyectosDto>($"/api/v1/proyectos?busqueda={marca}");
        Assert.Equal(4, todos!.Total);

        var coord = await api.ClienteDeAsync(coordinador);
        var delPrograma = await coord.GetFromJsonAsync<PaginaProyectosDto>($"/api/v1/proyectos/programa/{programaId}?busqueda={marca}&orden=titulo");
        Assert.Equal([$"{marca} proyecto A", $"{marca} proyecto B", $"{marca} proyecto C"], delPrograma!.Items.Select(p => p.Titulo));
        Assert.Equal(HttpStatusCode.Forbidden, (await coord.GetAsync($"/api/v1/proyectos/programa/{otroPrograma}")).StatusCode);
        var alcanceCoord = await coord.GetFromJsonAsync<PaginaProyectosDto>($"/api/v1/proyectos?busqueda={marca}");
        Assert.Equal(3, alcanceCoord!.Total);

        var desc = await ip.GetFromJsonAsync<PaginaProyectosDto>($"/api/v1/proyectos?busqueda={marca}&orden=titulo&desc=true");
        Assert.Equal($"{marca} proyecto C", desc!.Items[0].Titulo);
        var filtroEstado = await director.GetFromJsonAsync<PaginaProyectosDto>($"/api/v1/proyectos?busqueda={marca}&estadoId=6");
        Assert.Equal(0, filtroEstado!.Total);
        var filtroPrograma = await director.GetFromJsonAsync<PaginaProyectosDto>($"/api/v1/proyectos?busqueda={marca}&programaId={otroPrograma}");
        Assert.Equal(1, filtroPrograma!.Total);
        var filtroIp = await director.GetFromJsonAsync<PaginaProyectosDto>($"/api/v1/proyectos?busqueda={marca}&investigadorPrincipalId={investigador.Id}&anio={DateTime.UtcNow.Year}");
        Assert.Equal(3, filtroIp!.Total);
        Assert.Equal(20, filtroIp.TamanoPagina);
    }

    [Fact]
    public async Task HU13_CA1_CA3_la_ficha_y_el_listado_muestran_cronograma_y_proximo_hito()
    {
        var (programaId, _, investigador) = await EscenarioAsync();
        var ip = await api.ClienteDeAsync(investigador);
        var id = await ip.CrearPropuestaAsync(programaId);
        var limite = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(10);
        await api.EnBaseDatosAsync(async db =>
        {
            var cronograma = new Cronograma { Nombre = "Cronograma", ProyectoId = id };
            db.Cronogramas.Add(cronograma);
            await db.SaveChangesAsync();
            db.Hitos.Add(new Hito { CronogramaId = cronograma.Id, TipoHitoId = 1, Titulo = "Primer entregable", FechaLimite = limite });
            db.Hitos.Add(new Hito { CronogramaId = cronograma.Id, TipoHitoId = 1, Titulo = "Hito completado", FechaLimite = limite.AddDays(-20), Estado = "Completado" });
            return await db.SaveChangesAsync();
        });

        var listado = await ip.GetFromJsonAsync<PaginaProyectosDto>("/api/v1/proyectos/mios");
        var item = Assert.Single(listado!.Items, p => p.Id == id);
        Assert.Equal("Primer entregable", item.ProximoHitoTitulo);

        var ficha = await ip.GetFromJsonAsync<FichaDto>($"/api/v1/proyectos/{id}");
        Assert.Equal(2, ficha!.Proyecto.Cronograma.Length);
    }
}
