using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Ci2dt2.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ci2dt2.IntegrationTests;

/// <summary>HU-01 Iniciar sesión y HU-02 Mantener y cerrar la sesión.</summary>
[Collection(ColeccionApi.Nombre)]
public class AuthTests(ApiFactory api)
{
    [Fact]
    public async Task HU01_CA1_login_correcto_devuelve_tokens_cookie_segura_y_claims()
    {
        var email = ApiFactory.Unico("login");
        var usuario = await api.CrearUsuarioAsync(email, roles: ["Coordinador", "Investigador"]);
        await api.EnBaseDatosAsync(db =>
        {
            db.ProgramaIntegrantes.Add(new ProgramaIntegrante
            {
                ProgramaId = 3,
                UsuarioId = usuario.Id,
                RolEnPrograma = "Coordinador",
                FechaInicio = new DateOnly(2026, 1, 1),
            });
            return db.SaveChangesAsync();
        });

        var respuesta = await api.LoginAsync(email, ApiFactory.PasswordPrueba);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<RespuestaSesionDto>();
        Assert.Equal(["Coordinador", "Investigador"], cuerpo!.Usuario.Roles);
        Assert.Equal([3], cuerpo.Usuario.Programas);
        Assert.InRange(cuerpo.ExpiraEn - DateTime.UtcNow, TimeSpan.FromMinutes(14), TimeSpan.FromMinutes(15.1));

        var setCookie = string.Join(";", respuesta.Headers.GetValues("Set-Cookie")).ToLowerInvariant();
        Assert.Contains("httponly", setCookie);
        Assert.Contains("secure", setCookie);
        Assert.Contains("samesite=strict", setCookie);

        var claims = LeerClaims(cuerpo.AccessToken);
        Assert.Equal(usuario.Id.ToString(), claims.GetProperty("sub").GetString());
        Assert.Equal(email, claims.GetProperty("email").GetString());
        Assert.Equal(2, claims.GetProperty("role").GetArrayLength());
        Assert.Equal("3", claims.GetProperty("programa").GetString());

        // El token de refresco se guarda solo como hash, con la IP de origen y vigencia de 7 días.
        var cookie = ApiFactory.CookieDe(respuesta)!;
        var token = await api.EnBaseDatosAsync(db => db.RefreshTokens.SingleAsync(t => t.UsuarioId == usuario.Id));
        Assert.NotEqual(cookie, token.TokenHash);
        Assert.Equal(64, token.TokenHash.Length);
        Assert.Equal(ApiFactory.IpCliente, token.IpOrigen);
        Assert.InRange(token.ExpiraEn - DateTime.UtcNow, TimeSpan.FromDays(6.99), TimeSpan.FromDays(7));
    }

    [Fact]
    public async Task HU01_CA2_credenciales_incorrectas_responden_mensaje_generico()
    {
        var email = ApiFactory.Unico("generico");
        await api.CrearUsuarioAsync(email, roles: ["Investigador"]);

        var claveMala = await api.LoginAsync(email, "OtraClave123");
        var correoMalo = await api.LoginAsync(ApiFactory.Unico("noexiste"), ApiFactory.PasswordPrueba);

        foreach (var respuesta in new[] { claveMala, correoMalo })
        {
            Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
            Assert.Equal("Correo o contraseña incorrectos", (await respuesta.Content.ReadFromJsonAsync<ErrorDto>())!.Error);
            Assert.Null(ApiFactory.CookieDe(respuesta));
        }
    }

    [Fact]
    public async Task HU01_CA3_usuario_inactivo_no_puede_iniciar_sesion()
    {
        var email = ApiFactory.Unico("inactivo");
        await api.CrearUsuarioAsync(email, tieneAcceso: false, roles: ["Investigador"]);

        var respuesta = await api.LoginAsync(email, ApiFactory.PasswordPrueba);

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
        Assert.Equal("Su cuenta está inactiva. Contacte al administrador del sistema",
            (await respuesta.Content.ReadFromJsonAsync<ErrorDto>())!.Error);
    }

    [Fact]
    public async Task HU01_CA4_cinco_intentos_fallidos_bloquean_la_cuenta_15_minutos()
    {
        var email = ApiFactory.Unico("bloqueo");
        await api.CrearUsuarioAsync(email, roles: ["Investigador"]);

        for (var i = 0; i < 5; i++)
            Assert.Equal(HttpStatusCode.Unauthorized, (await api.LoginAsync(email, "Incorrecta123")).StatusCode);

        // El sexto intento se rechaza aunque la contraseña sea correcta, informando el tiempo restante.
        var sexto = await api.LoginAsync(email, ApiFactory.PasswordPrueba);
        Assert.Equal(HttpStatusCode.TooManyRequests, sexto.StatusCode);
        var error = await sexto.Content.ReadFromJsonAsync<ErrorDto>();
        Assert.InRange(error!.SegundosRestantes!.Value, 890, 900);
        Assert.Contains("15 minuto(s)", error.Error);
    }

    [Fact]
    public async Task HU01_CA5_todo_intento_de_login_queda_en_la_auditoria()
    {
        var email = ApiFactory.Unico("auditlogin");
        var usuario = await api.CrearUsuarioAsync(email, roles: ["Investigador"]);

        await api.LoginAsync(email, "Incorrecta123");
        await api.LoginAsync(email, ApiFactory.PasswordPrueba);

        var registros = await api.EsperarLogsAsync(l => l.Recurso == "Auth" && l.Accion == "LOGIN" && l.UsuarioId == usuario.Id, 2);
        Assert.Equal([401, 200], registros.Select(r => r.EstadoHttp));
        Assert.All(registros, r =>
        {
            Assert.Equal(ApiFactory.IpCliente, r.IpOrigen);
            Assert.InRange(r.FechaUtc, DateTime.UtcNow.AddMinutes(-2), DateTime.UtcNow.AddMinutes(1));
            Assert.DoesNotContain("Incorrecta123", r.ValoresNuevos);
        });
        Assert.Contains("CredencialesInvalidas", registros[0].ValoresNuevos);
        Assert.Contains("Exitoso", registros[1].ValoresNuevos);
    }

    [Fact]
    public async Task HU02_CA1_refresh_rota_el_token_y_emite_uno_nuevo()
    {
        var email = ApiFactory.Unico("rotacion");
        await api.CrearUsuarioAsync(email, roles: ["Investigador"]);
        var sesion = await api.IniciarSesionAsync(email);

        var respuesta = await api.RefrescarAsync(sesion.CookieRefresco);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var nuevaCookie = ApiFactory.CookieDe(respuesta);
        Assert.NotNull(nuevaCookie);
        Assert.NotEqual(sesion.CookieRefresco, nuevaCookie);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<RespuestaSesionDto>();
        Assert.False(string.IsNullOrEmpty(cuerpo!.AccessToken));

        var tokens = await api.EnBaseDatosAsync(db => db.RefreshTokens
            .Where(t => t.Usuario!.Email == email).OrderBy(t => t.Id).ToListAsync());
        Assert.Equal(2, tokens.Count);
        Assert.NotNull(tokens[0].RevocadoEn);
        Assert.Null(tokens[1].RevocadoEn);
    }

    [Fact]
    public async Task HU02_CA2_refresh_expirado_o_invalido_pide_iniciar_sesion_de_nuevo()
    {
        var email = ApiFactory.Unico("expirado");
        var usuario = await api.CrearUsuarioAsync(email, roles: ["Investigador"]);
        const string tokenExpirado = "token-expirado-de-prueba";
        await api.EnBaseDatosAsync(db =>
        {
            db.RefreshTokens.Add(new RefreshToken
            {
                UsuarioId = usuario.Id,
                TokenHash = Application.Seguridad.Criptografia.Sha256Hex(tokenExpirado),
                ExpiraEn = DateTime.UtcNow.AddMinutes(-1),
            });
            return db.SaveChangesAsync();
        });

        foreach (var cookie in new[] { tokenExpirado, "token-inexistente", null })
        {
            var respuesta = await api.RefrescarAsync(cookie);
            Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
            Assert.Equal("Su sesión expiró. Inicie sesión nuevamente",
                (await respuesta.Content.ReadFromJsonAsync<ErrorDto>())!.Error);
        }
    }

    [Fact]
    public async Task HU02_CA3_reutilizar_un_token_rotado_revoca_todos_los_tokens_del_usuario()
    {
        var email = ApiFactory.Unico("robo");
        await api.CrearUsuarioAsync(email, roles: ["Investigador"]);
        var sesion = await api.IniciarSesionAsync(email);
        var otraSesion = await api.IniciarSesionAsync(email);
        var rotada = ApiFactory.CookieDe(await api.RefrescarAsync(sesion.CookieRefresco));

        var reutilizado = await api.RefrescarAsync(sesion.CookieRefresco);

        Assert.Equal(HttpStatusCode.Unauthorized, reutilizado.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await api.RefrescarAsync(rotada)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await api.RefrescarAsync(otraSesion.CookieRefresco)).StatusCode);
        var activos = await api.EnBaseDatosAsync(db => db.RefreshTokens.CountAsync(t => t.Usuario!.Email == email && t.RevocadoEn == null));
        Assert.Equal(0, activos);
    }

    [Fact]
    public async Task HU02_CA4_cerrar_sesion_revoca_el_token_y_borra_la_cookie()
    {
        var email = ApiFactory.Unico("logout");
        await api.CrearUsuarioAsync(email, roles: ["Investigador"]);
        var sesion = await api.IniciarSesionAsync(email);

        var peticion = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/logout");
        peticion.Headers.Add("Cookie", $"ci2dt2_rt={sesion.CookieRefresco}");
        var respuesta = await api.CreateClient().SendAsync(peticion);

        Assert.Equal(HttpStatusCode.NoContent, respuesta.StatusCode);
        Assert.Contains(respuesta.Headers.GetValues("Set-Cookie"), c => c.StartsWith("ci2dt2_rt=;") && c.Contains("expires="));
        Assert.Equal(HttpStatusCode.Unauthorized, (await api.RefrescarAsync(sesion.CookieRefresco)).StatusCode);
    }

    [Fact]
    public async Task Las_respuestas_de_la_api_no_se_guardan_en_cache()
    {
        var respuesta = await api.LoginAsync(ApiFactory.AdminEmail, ApiFactory.AdminPassword);
        Assert.Contains("no-store", respuesta.Headers.CacheControl!.ToString());
    }

    [Fact]
    public async Task Health_check_responde_sin_autenticacion()
    {
        var respuesta = await api.CreateClient().GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
    }

    private static JsonElement LeerClaims(string jwt)
    {
        var payload = jwt.Split('.')[1].Replace('-', '+').Replace('_', '/');
        payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
        return JsonDocument.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(payload))).RootElement;
    }
}
