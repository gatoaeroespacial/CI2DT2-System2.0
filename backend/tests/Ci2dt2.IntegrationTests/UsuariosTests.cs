using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Ci2dt2.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ci2dt2.IntegrationTests;

public sealed record UsuarioDetalleDto(int Id, string Email, string Nombres, bool TieneAcceso, string[] Roles, int? AliadoId);

public sealed record UsuarioItemDto(int Id, string Email, bool TieneAcceso, string[] Roles);

public sealed record PaginaDto(UsuarioItemDto[] Items, int Total, int PaginaActual, int TamanoPagina);

/// <summary>HU-04 Administrar usuarios y roles.</summary>
[Collection(ColeccionApi.Nombre)]
public class UsuariosTests(ApiFactory api)
{
    private async Task<HttpClient> AdminAsync() =>
        api.ClienteCon(await api.IniciarSesionAsync(ApiFactory.AdminEmail, ApiFactory.AdminPassword));

    private static object NuevoUsuario(string email, string[]? roles = null, string? orcid = null, string? cvlac = null, int? aliadoId = null) => new
    {
        nombres = "María",
        apellidos = "Gómez",
        tipoDocumento = "CC",
        numeroDocumento = "10203040",
        email,
        telefono = "3001234567",
        cargo = "Docente",
        dependencia = "Facultad de Ingeniería",
        cvLacUrl = cvlac,
        orcid,
        aliadoId,
        roles = roles ?? ["Investigador"],
        contrasena = "ClaveInicial123",
    };

    [Fact]
    public async Task CA1_crear_usuario_con_contrasena_inicial_e_iniciar_sesion()
    {
        var admin = await AdminAsync();
        var email = ApiFactory.Unico("nuevo");

        var respuesta = await admin.PostAsJsonAsync("/api/v1/usuarios",
            NuevoUsuario(email.ToUpperInvariant(), ["Investigador", "Coordinador"], "0000-0002-1825-0097",
                "https://scienti.minciencias.gov.co/cvlac/visualizador/generarCurriculoCv.do?cod_rh=1"));

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        var creado = await respuesta.Content.ReadFromJsonAsync<UsuarioDetalleDto>();
        Assert.Equal(email, creado!.Email);
        Assert.True(creado.TieneAcceso);
        Assert.Equal(["Coordinador", "Investigador"], creado.Roles);
        Assert.Equal(HttpStatusCode.OK, (await api.LoginAsync(email, "ClaveInicial123")).StatusCode);

        // La contraseña nunca se envía por correo.
        Assert.Null(api.Correos.UltimoPara(email));
    }

    [Fact]
    public async Task CA1_la_contrasena_inicial_debe_cumplir_la_politica()
    {
        var admin = await AdminAsync();
        var datos = new Dictionary<string, object?>
        {
            ["nombres"] = "Ana",
            ["apellidos"] = "Pérez",
            ["email"] = ApiFactory.Unico("debil"),
            ["roles"] = new[] { "Investigador" },
            ["contrasena"] = "debil",
        };

        var respuesta = await admin.PostAsJsonAsync("/api/v1/usuarios", datos);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.True((await respuesta.Content.ReadFromJsonAsync<ErrorDto>())!.Errores!.ContainsKey("contrasena"));
    }

    [Fact]
    public async Task CA6_asignar_nueva_contrasena_revoca_las_sesiones()
    {
        var admin = await AdminAsync();
        var email = ApiFactory.Unico("nuevaclave");
        var usuario = await api.CrearUsuarioAsync(email, roles: ["Investigador"]);
        var sesion = await api.IniciarSesionAsync(email);

        var debil = await admin.PutAsJsonAsync($"/api/v1/usuarios/{usuario.Id}/contrasena", new { contrasena = "corta" });
        Assert.Equal(HttpStatusCode.BadRequest, debil.StatusCode);

        var respuesta = await admin.PutAsJsonAsync($"/api/v1/usuarios/{usuario.Id}/contrasena", new { contrasena = "Renovada12345" });

        Assert.Equal(HttpStatusCode.NoContent, respuesta.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await api.RefrescarAsync(sesion.CookieRefresco)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await api.LoginAsync(email, ApiFactory.PasswordPrueba)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await api.LoginAsync(email, "Renovada12345")).StatusCode);

        var registro = Assert.Single(await api.EsperarLogsAsync(l => l.Recurso == "usuarios" && l.Accion == "PUT" && l.RecursoId == usuario.Id.ToString() && l.EstadoHttp == 204));
        Assert.DoesNotContain("Renovada12345", registro.ValoresNuevos ?? "");
        Assert.DoesNotContain("passwordHash", registro.ValoresNuevos ?? "", StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Un_usuario_sin_contrasena_no_puede_activarse()
    {
        var admin = await AdminAsync();
        var jurado = await api.EnBaseDatosAsync(async db =>
        {
            var u = new Usuario { Email = ApiFactory.Unico("jurado"), Nombres = "Jurado", Apellidos = "Externo", TieneAcceso = false };
            db.Usuarios.Add(u);
            await db.SaveChangesAsync();
            return u;
        });

        var respuesta = await admin.PatchAsJsonAsync($"/api/v1/usuarios/{jurado.Id}/estado", new { tieneAcceso = true });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, respuesta.StatusCode);
    }

    [Fact]
    public async Task CA2_correo_duplicado_responde_422()
    {
        var admin = await AdminAsync();
        var email = ApiFactory.Unico("duplicado");
        Assert.Equal(HttpStatusCode.Created, (await admin.PostAsJsonAsync("/api/v1/usuarios", NuevoUsuario(email))).StatusCode);

        var respuesta = await admin.PostAsJsonAsync("/api/v1/usuarios", NuevoUsuario(" " + email.ToUpperInvariant()));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, respuesta.StatusCode);
        Assert.Equal("Ya existe un usuario con este correo", (await respuesta.Content.ReadFromJsonAsync<ErrorDto>())!.Error);
    }

    [Fact]
    public async Task CA2_editar_con_el_correo_de_otro_usuario_responde_422()
    {
        var admin = await AdminAsync();
        var otro = await api.CrearUsuarioAsync(ApiFactory.Unico("otro"), roles: ["Investigador"]);
        var usuario = await api.CrearUsuarioAsync(ApiFactory.Unico("editar"), roles: ["Investigador"]);

        var respuesta = await admin.PutAsJsonAsync($"/api/v1/usuarios/{usuario.Id}", NuevoUsuario(otro.Email));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, respuesta.StatusCode);
    }

    [Fact]
    public async Task CA3_desactivar_impide_el_login_y_revoca_los_tokens_de_inmediato()
    {
        var admin = await AdminAsync();
        var email = ApiFactory.Unico("desactivar");
        var usuario = await api.CrearUsuarioAsync(email, roles: ["Investigador"]);
        var sesion = await api.IniciarSesionAsync(email);

        var respuesta = await admin.PatchAsJsonAsync($"/api/v1/usuarios/{usuario.Id}/estado", new { tieneAcceso = false });

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await api.LoginAsync(email, ApiFactory.PasswordPrueba)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await api.RefrescarAsync(sesion.CookieRefresco)).StatusCode);
        Assert.Equal(0, await api.EnBaseDatosAsync(db => db.RefreshTokens.CountAsync(t => t.UsuarioId == usuario.Id && t.RevocadoEn == null)));

        // Reactivar permite volver a iniciar sesión.
        await admin.PatchAsJsonAsync($"/api/v1/usuarios/{usuario.Id}/estado", new { tieneAcceso = true });
        Assert.Equal(HttpStatusCode.OK, (await api.LoginAsync(email, ApiFactory.PasswordPrueba)).StatusCode);
    }

    [Fact]
    public async Task Cambiar_estado_exige_indicar_el_nuevo_estado()
    {
        var admin = await AdminAsync();
        var usuario = await api.CrearUsuarioAsync(ApiFactory.Unico("estado"), roles: ["Investigador"]);

        var respuesta = await admin.PatchAsJsonAsync($"/api/v1/usuarios/{usuario.Id}/estado", new { });

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.True(await api.EnBaseDatosAsync(db => db.Usuarios.Where(u => u.Id == usuario.Id).Select(u => u.TieneAcceso).SingleAsync()));
    }

    [Fact]
    public async Task El_administrador_no_puede_desactivarse_a_si_mismo()
    {
        var email = ApiFactory.Unico("adminself");
        var admin = await api.CrearUsuarioAsync(email, roles: ["Admin"]);
        var cliente = api.ClienteCon(await api.IniciarSesionAsync(email));

        var respuesta = await cliente.PatchAsJsonAsync($"/api/v1/usuarios/{admin.Id}/estado", new { tieneAcceso = false });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, respuesta.StatusCode);
        Assert.Equal("No puede desactivar su propia cuenta.", (await respuesta.Content.ReadFromJsonAsync<ErrorDto>())!.Error);
    }

    [Fact]
    public async Task No_se_puede_quitar_el_rol_al_ultimo_administrador_activo()
    {
        var admin = await AdminAsync();
        var adminInicialId = await api.EnBaseDatosAsync(db => db.Usuarios.Where(u => u.Email == ApiFactory.AdminEmail).Select(u => u.Id).SingleAsync());

        // Se desactivan temporalmente los demás administradores creados por otras pruebas.
        var otrosAdmins = await api.EnBaseDatosAsync(db => db.Usuarios
            .Where(u => u.Id != adminInicialId && u.TieneAcceso && u.Roles.Any(r => r.Rol!.Nombre == "Admin"))
            .Select(u => u.Id).ToListAsync());
        await CambiarAccesoAsync(otrosAdmins, false);
        try
        {
            var respuesta = await admin.PutAsJsonAsync($"/api/v1/usuarios/{adminInicialId}/roles", new { roles = new[] { "Director" } });

            Assert.Equal(HttpStatusCode.UnprocessableEntity, respuesta.StatusCode);
            Assert.Equal("No se puede quitar el rol al último Administrador activo.",
                (await respuesta.Content.ReadFromJsonAsync<ErrorDto>())!.Error);
        }
        finally
        {
            await CambiarAccesoAsync(otrosAdmins, true);
        }
    }

    [Fact]
    public async Task Asignar_roles_reemplaza_los_roles_del_usuario()
    {
        var admin = await AdminAsync();
        var usuario = await api.CrearUsuarioAsync(ApiFactory.Unico("roles"), roles: ["Investigador"]);

        var respuesta = await admin.PutAsJsonAsync($"/api/v1/usuarios/{usuario.Id}/roles", new { roles = new[] { "Coordinador", "Director" } });

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Equal(["Coordinador", "Director"], (await respuesta.Content.ReadFromJsonAsync<UsuarioDetalleDto>())!.Roles);
        var roles = await api.EnBaseDatosAsync(db => db.UsuarioRoles.Where(ur => ur.UsuarioId == usuario.Id).Select(ur => ur.Rol!.Nombre).OrderBy(n => n).ToListAsync());
        Assert.Equal(["Coordinador", "Director"], roles);
    }

    [Fact]
    public async Task CA4_listado_paginado_de_20_con_busqueda_y_filtros()
    {
        var admin = await AdminAsync();
        var marca = "lst" + Guid.NewGuid().ToString("N")[..8];
        for (var i = 0; i < 23; i++)
            await api.CrearUsuarioAsync($"{marca}{i:00}@pruebas.local", tieneAcceso: i % 2 == 0, roles: i < 3 ? ["Director"] : ["Investigador"]);

        var pagina1 = await admin.GetFromJsonAsync<PaginaDto>($"/api/v1/usuarios?busqueda={marca}");
        var pagina2 = await admin.GetFromJsonAsync<PaginaDto>($"/api/v1/usuarios?busqueda={marca}&pagina=2");
        Assert.Equal(23, pagina1!.Total);
        Assert.Equal(20, pagina1.Items.Length);
        Assert.Equal(20, pagina1.TamanoPagina);
        Assert.Equal(3, pagina2!.Items.Length);
        Assert.Empty(pagina1.Items.Select(u => u.Id).Intersect(pagina2.Items.Select(u => u.Id)));

        var directores = await admin.GetFromJsonAsync<PaginaDto>($"/api/v1/usuarios?busqueda={marca}&rol=Director");
        Assert.Equal(3, directores!.Total);
        Assert.All(directores.Items, u => Assert.Contains("Director", u.Roles));

        var inactivos = await admin.GetFromJsonAsync<PaginaDto>($"/api/v1/usuarios?busqueda={marca}&estado=inactivo");
        Assert.Equal(11, inactivos!.Total);
        Assert.All(inactivos.Items, u => Assert.False(u.TieneAcceso));

        // Búsqueda por documento y escape de comodines.
        await api.EnBaseDatosAsync(db => db.Usuarios.Where(u => u.Email == $"{marca}05@pruebas.local")
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.NumeroDocumento, $"{marca}9988")));
        Assert.Equal(1, (await admin.GetFromJsonAsync<PaginaDto>($"/api/v1/usuarios?busqueda={marca}9988"))!.Total);
        Assert.Equal(0, (await admin.GetFromJsonAsync<PaginaDto>("/api/v1/usuarios?busqueda=%25%25_"))!.Total);
    }

    [Theory]
    [InlineData("0000-0002-1825-009", null)]
    [InlineData(null, "https://otro-sitio.com/cvlac")]
    public async Task CA5_orcid_y_cvlac_invalidos_responden_400(string? orcid, string? cvlac)
    {
        var admin = await AdminAsync();

        var respuesta = await admin.PostAsJsonAsync("/api/v1/usuarios", NuevoUsuario(ApiFactory.Unico("orcid"), orcid: orcid, cvlac: cvlac));

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        var error = await respuesta.Content.ReadFromJsonAsync<ErrorDto>();
        Assert.True(error!.Errores!.ContainsKey(orcid is null ? "cvLacUrl" : "orcid"));
    }

    [Fact]
    public async Task Un_usuario_externo_debe_quedar_asociado_a_un_aliado()
    {
        var admin = await AdminAsync();
        var aliadoId = await api.EnBaseDatosAsync(async db =>
        {
            var aliado = new Aliado { Nombre = "Aliado de prueba", TipoAliadoId = 2, Pais = "Suiza", Email = "aliado@pruebas.local" };
            db.Aliados.Add(aliado);
            await db.SaveChangesAsync();
            return aliado.Id;
        });

        var sinAliado = await admin.PostAsJsonAsync("/api/v1/usuarios", NuevoUsuario(ApiFactory.Unico("ext"), ["Externo"]));
        var aliadoInexistente = await admin.PostAsJsonAsync("/api/v1/usuarios", NuevoUsuario(ApiFactory.Unico("ext"), ["Externo"], aliadoId: 999999));
        var conAliado = await admin.PostAsJsonAsync("/api/v1/usuarios", NuevoUsuario(ApiFactory.Unico("ext"), ["Externo"], aliadoId: aliadoId));

        Assert.Equal(HttpStatusCode.BadRequest, sinAliado.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, aliadoInexistente.StatusCode);
        Assert.Equal(HttpStatusCode.Created, conAliado.StatusCode);

        // Tampoco se puede asignar el rol Externo a un usuario sin aliado.
        var usuario = await api.CrearUsuarioAsync(ApiFactory.Unico("ext"), roles: ["Investigador"]);
        var asignar = await admin.PutAsJsonAsync($"/api/v1/usuarios/{usuario.Id}/roles", new { roles = new[] { "Externo" } });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, asignar.StatusCode);
    }

    [Fact]
    public async Task CA6_crear_y_editar_quedan_auditados_sin_el_hash_de_la_contrasena()
    {
        var admin = await AdminAsync();
        var email = ApiFactory.Unico("audit");
        var creado = await (await admin.PostAsJsonAsync("/api/v1/usuarios", NuevoUsuario(email))).Content.ReadFromJsonAsync<UsuarioDetalleDto>();

        var editado = await admin.PutAsJsonAsync($"/api/v1/usuarios/{creado!.Id}", new
        {
            nombres = "María José",
            apellidos = "Gómez",
            email,
            cargo = "Coordinadora",
        });
        Assert.Equal(HttpStatusCode.OK, editado.StatusCode);

        var registros = await api.EsperarLogsAsync(l => l.Recurso == "usuarios" && l.RecursoId == creado.Id.ToString(), 2);
        Assert.Equal(["POST", "PUT"], registros.Select(r => r.Accion));
        Assert.Null(registros[0].ValoresAnteriores);
        Assert.Contains("\"cargo\":\"Docente\"", registros[1].ValoresAnteriores);
        Assert.Contains("\"cargo\":\"Coordinadora\"", registros[1].ValoresNuevos);
        Assert.Contains("María José", registros[1].ValoresNuevos);
        Assert.All(registros, r =>
        {
            Assert.NotNull(r.UsuarioId);
            Assert.DoesNotContain("passwordHash", r.ValoresNuevos, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("passwordHash", r.ValoresAnteriores ?? "", StringComparison.OrdinalIgnoreCase);
        });
    }

    [Fact]
    public async Task Obtener_un_usuario_inexistente_responde_404()
    {
        var admin = await AdminAsync();
        Assert.Equal(HttpStatusCode.NotFound, (await admin.GetAsync("/api/v1/usuarios/999999")).StatusCode);
    }

    private Task CambiarAccesoAsync(List<int> ids, bool tieneAcceso) =>
        api.EnBaseDatosAsync(db => db.Usuarios.Where(u => ids.Contains(u.Id))
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.TieneAcceso, tieneAcceso)));
}
