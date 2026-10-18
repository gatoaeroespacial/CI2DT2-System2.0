using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;

namespace Ci2dt2.IntegrationTests;

public sealed record ObjetivoDto(int Id, string? Descripcion, int? Orden);
public sealed record AliadoProgramaDto(int Id, string Nombre);
public sealed record ProgramaDetalleDto(int Id, string Nombre, string? Descripcion, ObjetivoDto[] Objetivos, AliadoProgramaDto[] Aliados);
public sealed record IntegranteProgramaDto(int Id, int UsuarioId, string Nombre, string RolEnPrograma, DateTime? FechaInicio, DateTime? FechaFin, bool Vigente);
public sealed record AliadoDto(int Id, string Nombre, string? Nit, string? Pais, string TipoAliado);
public sealed record PaginaAliadosDto(AliadoDto[] Items, int Total);

/// <summary>HU-06 Administrar programas, objetivos e integrantes · HU-07 Catálogo de aliados.</summary>
[Collection(ColeccionApi.Nombre)]
public class ProgramasAliadosTests(ApiFactory api)
{
    private static string Hoy(int dias = 0) => DateOnly.FromDateTime(DateTime.UtcNow).AddDays(dias).ToString("yyyy-MM-dd");

    [Fact]
    public async Task HU06_CA1_el_director_edita_nombre_y_descripcion_y_el_coordinador_no()
    {
        var programaId = await api.NuevoProgramaAsync();
        var director = await api.ClienteComoAsync("Director");
        var coordinador = await api.ClienteComoAsync("Coordinador");

        var respuesta = await director.PutAsJsonAsync($"/api/v1/programas/{programaId}", new { nombre = "Programa editado", descripcion = "Nueva descripción" });
        Assert.Equal(HttpStatusCode.NoContent, respuesta.StatusCode);
        var detalle = await director.GetFromJsonAsync<ProgramaDetalleDto>($"/api/v1/programas/{programaId}");
        Assert.Equal("Programa editado", detalle!.Nombre);
        Assert.Equal("Nueva descripción", detalle.Descripcion);

        Assert.Equal(HttpStatusCode.Forbidden,
            (await coordinador.PutAsJsonAsync($"/api/v1/programas/{programaId}", new { nombre = "No permitido" })).StatusCode);
    }

    [Fact]
    public async Task HU06_CA2_agregar_editar_eliminar_y_reordenar_objetivos()
    {
        var programaId = await api.NuevoProgramaAsync();
        var director = await api.ClienteComoAsync("Director");
        var ids = new List<int>();
        foreach (var texto in new[] { "Objetivo uno", "Objetivo dos", "Objetivo tres" })
        {
            var r = await director.PostAsJsonAsync($"/api/v1/programas/{programaId}/objetivos", new { descripcion = texto });
            Assert.Equal(HttpStatusCode.Created, r.StatusCode);
            ids.Add((await r.Content.ReadFromJsonAsync<CreadoDto>())!.Id);
        }

        Assert.Equal(HttpStatusCode.NoContent, (await director.PutAsJsonAsync(
            $"/api/v1/programas/{programaId}/objetivos/orden", new { ids = new[] { ids[2], ids[0], ids[1] } })).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await director.PutAsJsonAsync(
            $"/api/v1/programas/{programaId}/objetivos/{ids[0]}", new { descripcion = "Objetivo uno editado" })).StatusCode);
        var objetivos = await director.GetFromJsonAsync<ObjetivoDto[]>($"/api/v1/programas/{programaId}/objetivos");
        Assert.Equal(["Objetivo tres", "Objetivo uno editado", "Objetivo dos"], objetivos!.Select(o => o.Descripcion));
        Assert.Equal([1, 2, 3], objetivos!.Select(o => o.Orden));

        // El orden incompleto se rechaza y eliminar compacta el orden de los restantes.
        Assert.Equal(HttpStatusCode.UnprocessableEntity, (await director.PutAsJsonAsync(
            $"/api/v1/programas/{programaId}/objetivos/orden", new { ids = new[] { ids[0] } })).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await director.DeleteAsync($"/api/v1/programas/{programaId}/objetivos/{ids[2]}")).StatusCode);
        objetivos = await director.GetFromJsonAsync<ObjetivoDto[]>($"/api/v1/programas/{programaId}/objetivos");
        Assert.Equal(["Objetivo uno editado", "Objetivo dos"], objetivos!.Select(o => o.Descripcion));
        Assert.Equal([1, 2], objetivos!.Select(o => o.Orden));

        var investigador = await api.ClienteComoAsync("Investigador");
        Assert.Equal(HttpStatusCode.Forbidden, (await investigador.PostAsJsonAsync(
            $"/api/v1/programas/{programaId}/objetivos", new { descripcion = "No permitido" })).StatusCode);
    }

    [Fact]
    public async Task HU06_solo_director_o_admin_asignan_coordinador_y_hay_maximo_uno_vigente()
    {
        var programaId = await api.NuevoProgramaAsync();
        var director = await api.ClienteComoAsync("Director");
        var coordinadorA = await api.UsuarioAsync("coordA", "Coordinador");
        var coordinadorB = await api.UsuarioAsync("coordB", "Coordinador");
        var soloInvestigador = await api.UsuarioAsync("soloinv", "Investigador");

        var r = await director.PostAsJsonAsync($"/api/v1/programas/{programaId}/integrantes",
            new { usuarioId = coordinadorA.Id, rolEnPrograma = "Coordinador", fechaInicio = Hoy() });
        Assert.Equal(HttpStatusCode.Created, r.StatusCode);
        var vinculacionA = (await r.Content.ReadFromJsonAsync<CreadoDto>())!.Id;

        var segundo = await director.PostAsJsonAsync($"/api/v1/programas/{programaId}/integrantes",
            new { usuarioId = coordinadorB.Id, rolEnPrograma = "Coordinador", fechaInicio = Hoy() });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, segundo.StatusCode);

        // Sin el rol de sistema Coordinador no puede coordinar.
        var sinRol = await director.PostAsJsonAsync($"/api/v1/programas/{programaId}/integrantes",
            new { usuarioId = soloInvestigador.Id, rolEnPrograma = "Coordinador", fechaInicio = Hoy() });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, sinRol.StatusCode);

        // Un Coordinador no asigna Coordinadores.
        var clienteA = await api.ClienteDeAsync(coordinadorA);
        Assert.Equal(HttpStatusCode.Forbidden, (await clienteA.PostAsJsonAsync($"/api/v1/programas/{programaId}/integrantes",
            new { usuarioId = coordinadorB.Id, rolEnPrograma = "Coordinador", fechaInicio = Hoy() })).StatusCode);

        // Cerrando la vinculación del anterior se puede asignar otro.
        Assert.Equal(HttpStatusCode.NoContent, (await director.PutAsJsonAsync($"/api/v1/programas/{programaId}/integrantes/{vinculacionA}",
            new { fechaInicio = Hoy(-30), fechaFin = Hoy() })).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await director.PostAsJsonAsync($"/api/v1/programas/{programaId}/integrantes",
            new { usuarioId = coordinadorB.Id, rolEnPrograma = "Coordinador", fechaInicio = Hoy() })).StatusCode);
    }

    [Fact]
    public async Task HU06_CA3_CA4_CA5_el_coordinador_vincula_investigadores_de_su_programa()
    {
        var programaId = await api.NuevoProgramaAsync();
        var otroPrograma = await api.NuevoProgramaAsync();
        var coordinador = await api.UsuarioAsync("coordvin", "Coordinador");
        await api.VincularAsync(coordinador.Id, programaId, "Coordinador");
        var investigador = await api.UsuarioAsync("invvin", "Investigador");
        var sesionInvestigador = await api.IniciarSesionAsync(investigador.Email);
        var cliente = await api.ClienteDeAsync(coordinador);

        // CA-4: fecha de fin anterior a la de inicio.
        var fechas = await cliente.PostAsJsonAsync($"/api/v1/programas/{programaId}/integrantes",
            new { usuarioId = investigador.Id, rolEnPrograma = "Investigador", fechaInicio = Hoy(), fechaFin = Hoy(-1) });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, fechas.StatusCode);
        Assert.Contains("fecha de fin", (await fechas.Content.ReadFromJsonAsync<ErrorDto>())!.Error);

        var r = await cliente.PostAsJsonAsync($"/api/v1/programas/{programaId}/integrantes",
            new { usuarioId = investigador.Id, rolEnPrograma = "Investigador", fechaInicio = Hoy() });
        Assert.Equal(HttpStatusCode.Created, r.StatusCode);
        var vinculacion = (await r.Content.ReadFromJsonAsync<CreadoDto>())!.Id;

        // Otro programa: no es su programa.
        Assert.Equal(HttpStatusCode.Forbidden, (await cliente.PostAsJsonAsync($"/api/v1/programas/{otroPrograma}/integrantes",
            new { usuarioId = investigador.Id, rolEnPrograma = "Investigador", fechaInicio = Hoy() })).StatusCode);

        // CA-5: notificación y el programa entra en los permisos al renovar la sesión.
        Assert.True(await api.EnBaseDatosAsync(db => db.Notificaciones.AnyAsync(n => n.UsuarioId == investigador.Id && n.Tipo == "VinculacionPrograma")));
        var renovada = await (await api.RefrescarAsync(sesionInvestigador.CookieRefresco)).Content.ReadFromJsonAsync<RespuestaSesionDto>();
        Assert.Contains(programaId, renovada!.Usuario.Programas);

        // CA-3: retiro con FechaFin; pasa a históricos sin borrarse.
        Assert.Equal(HttpStatusCode.NoContent, (await cliente.PutAsJsonAsync($"/api/v1/programas/{programaId}/integrantes/{vinculacion}",
            new { fechaInicio = Hoy(), fechaFin = Hoy() })).StatusCode);
        var integrantes = await cliente.GetFromJsonAsync<IntegranteProgramaDto[]>($"/api/v1/programas/{programaId}/integrantes");
        var retirado = Assert.Single(integrantes!, i => i.UsuarioId == investigador.Id);
        Assert.False(retirado.Vigente);
        Assert.NotNull(retirado.FechaFin);
        Assert.Contains(integrantes!, i => i.UsuarioId == coordinador.Id && i.Vigente);

        Assert.Equal(HttpStatusCode.Forbidden, (await cliente.GetAsync($"/api/v1/programas/{otroPrograma}/integrantes")).StatusCode);
    }

    [Fact]
    public async Task HU07_CA1_un_nit_registrado_impide_crear_y_muestra_el_aliado_existente()
    {
        var director = await api.ClienteComoAsync("Director");
        var nit = NitValido();

        var creado = await director.PostAsJsonAsync("/api/v1/aliados", new
        {
            tipoAliadoId = 2,
            nombre = "Empresa Uno",
            nit,
            pais = "Colombia",
            ciudad = "Manizales",
            email = "uno@pruebas.local",
        });
        Assert.Equal(HttpStatusCode.Created, creado.StatusCode);
        var aliado = await creado.Content.ReadFromJsonAsync<AliadoDto>();

        var duplicado = await director.PostAsJsonAsync("/api/v1/aliados", new
        {
            tipoAliadoId = 2,
            nombre = "Empresa Dos",
            nit,
            pais = "Colombia",
            email = "dos@pruebas.local",
        });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, duplicado.StatusCode);
        Assert.Contains("Empresa Uno", (await duplicado.Content.ReadFromJsonAsync<ErrorDto>())!.Error);

        // NIT colombiano con dígito de verificación incorrecto, y extranjero sin NIT.
        var digitoMalo = nit[..^1] + ((nit[^1] - '0' + 1) % 10);
        Assert.Equal(HttpStatusCode.BadRequest, (await director.PostAsJsonAsync("/api/v1/aliados", new
        {
            tipoAliadoId = 2,
            nombre = "Empresa Tres",
            nit = digitoMalo,
            pais = "Colombia",
            email = "tres@pruebas.local",
        })).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await director.PostAsJsonAsync("/api/v1/aliados", new
        {
            tipoAliadoId = 5,
            nombre = "Fundación Extranjera",
            pais = "Suiza",
            email = "fundacion@pruebas.local",
        })).StatusCode);

        // CA-2: autocompletado por parte del nombre; el borrado lógico lo saca del catálogo.
        var busqueda = await director.GetFromJsonAsync<PaginaAliadosDto>("/api/v1/aliados?busqueda=Empresa Un");
        Assert.Contains(busqueda!.Items, a => a.Id == aliado!.Id);
        Assert.Equal(HttpStatusCode.NoContent, (await director.DeleteAsync($"/api/v1/aliados/{aliado!.Id}")).StatusCode);
        busqueda = await director.GetFromJsonAsync<PaginaAliadosDto>("/api/v1/aliados?busqueda=Empresa Un");
        Assert.DoesNotContain(busqueda!.Items, a => a.Id == aliado.Id);
    }

    [Fact]
    public async Task HU07_el_coordinador_asocia_aliados_solo_a_su_programa()
    {
        var programaId = await api.NuevoProgramaAsync();
        var otroPrograma = await api.NuevoProgramaAsync();
        var coordinador = await api.UsuarioAsync("coordali", "Coordinador");
        await api.VincularAsync(coordinador.Id, programaId, "Coordinador");
        var cliente = await api.ClienteDeAsync(coordinador);
        var aliado = await (await cliente.PostAsJsonAsync("/api/v1/aliados", new
        {
            tipoAliadoId = 1,
            nombre = "Universidad Aliada",
            pais = "Chile",
            email = "u@pruebas.local",
        })).Content.ReadFromJsonAsync<AliadoDto>();

        Assert.Equal(HttpStatusCode.NoContent, (await cliente.PutAsJsonAsync($"/api/v1/programas/{programaId}/aliados",
            new { aliadoIds = new[] { aliado!.Id } })).StatusCode);
        var detalle = await cliente.GetFromJsonAsync<ProgramaDetalleDto>($"/api/v1/programas/{programaId}");
        Assert.Equal([aliado.Id], detalle!.Aliados.Select(a => a.Id));

        Assert.Equal(HttpStatusCode.Forbidden, (await cliente.PutAsJsonAsync($"/api/v1/programas/{otroPrograma}/aliados",
            new { aliadoIds = new[] { aliado.Id } })).StatusCode);

        // Quitar todos los aliados.
        Assert.Equal(HttpStatusCode.NoContent, (await cliente.PutAsJsonAsync($"/api/v1/programas/{programaId}/aliados",
            new { aliadoIds = Array.Empty<int>() })).StatusCode);
        detalle = await cliente.GetFromJsonAsync<ProgramaDetalleDto>($"/api/v1/programas/{programaId}");
        Assert.Empty(detalle!.Aliados);
    }

    /// <summary>NIT aleatorio de 9 dígitos con su dígito de verificación (algoritmo DIAN).</summary>
    private static string NitValido()
    {
        var numero = Random.Shared.Next(800_000_000, 999_999_999).ToString();
        return $"{numero}-{Application.Aliados.ReglasAliado.DigitoVerificacion(numero)}";
    }
}
