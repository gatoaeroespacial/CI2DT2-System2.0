using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Ci2dt2.Application.Seguridad;
using Ci2dt2.Domain.Entities;
using Ci2dt2.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Ci2dt2.IntegrationTests;

public sealed record CorreoEnviado(string Para, string Asunto, string Cuerpo);

public sealed class EmailSenderFalso : IEmailSender
{
    private readonly List<CorreoEnviado> _correos = [];

    public Task EnviarAsync(string para, string asunto, string cuerpoHtml, CancellationToken ct = default)
    {
        lock (_correos) _correos.Add(new CorreoEnviado(para, asunto, cuerpoHtml));
        return Task.CompletedTask;
    }

    public CorreoEnviado? UltimoPara(string para)
    {
        lock (_correos) return _correos.LastOrDefault(c => c.Para == para);
    }
}

public sealed record Sesion(string AccessToken, string CookieRefresco);

/// <summary>
/// API real sobre una base de datos SQL Server temporal (LocalDB por defecto; en CI se usa
/// la variable CI2DT2_TEST_SQL con la cadena de conexión del servidor).
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string AdminEmail = "admin@pruebas.local";
    public const string AdminPassword = "AdminPrueba123";
    public const string PasswordPrueba = "ClavePrueba123";

    private readonly string _conexion;
    public EmailSenderFalso Correos { get; } = new();

    public ApiFactory()
    {
        var servidor = Environment.GetEnvironmentVariable("CI2DT2_TEST_SQL")
            ?? @"Server=(localdb)\MSSQLLocalDB;Trusted_Connection=True;TrustServerCertificate=True";
        _conexion = new SqlConnectionStringBuilder(servidor) { InitialCatalog = $"Ci2dt2_Pruebas_{Guid.NewGuid():N}" }.ConnectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Default", _conexion);
        builder.UseSetting("Jwt:Clave", "clave-de-pruebas-de-integracion-0123456789abcdef");
        builder.UseSetting("AdminInicial:Email", AdminEmail);
        builder.UseSetting("AdminInicial:Password", AdminPassword);
        builder.UseSetting("Archivos:Ruta", Path.Combine(Path.GetTempPath(), $"ci2dt2-pruebas-{Guid.NewGuid():N}"));
        builder.ConfigureTestServices(s =>
        {
            s.RemoveAll<IEmailSender>();
            s.AddSingleton<IEmailSender>(Correos);
        });
    }

    public Task InitializeAsync()
    {
        _ = Server; // arranca la API: aplica migraciones y crea el Administrador inicial
        return Task.CompletedTask;
    }

    public new async Task DisposeAsync()
    {
        using (var scope = Services.CreateScope())
            await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureDeletedAsync();
        await base.DisposeAsync();
    }

    public async Task<T> EnBaseDatosAsync<T>(Func<AppDbContext, Task<T>> accion)
    {
        using var scope = Services.CreateScope();
        return await accion(scope.ServiceProvider.GetRequiredService<AppDbContext>());
    }

    public T Servicio<T>(IServiceScope scope) where T : notnull => scope.ServiceProvider.GetRequiredService<T>();

    /// <summary>
    /// La auditoría se escribe de forma asíncrona (HU-28): espera hasta que existan al menos
    /// <paramref name="cantidad"/> registros que cumplan el filtro.
    /// </summary>
    public async Task<List<LogActividad>> EsperarLogsAsync(Expression<Func<LogActividad, bool>> filtro, int cantidad = 1)
    {
        for (var intento = 0; ; intento++)
        {
            var registros = await EnBaseDatosAsync(db => db.LogActividades.Where(filtro).OrderBy(l => l.Id).ToListAsync());
            if (registros.Count >= cantidad || intento >= 50)
                return registros;
            await Task.Delay(100);
        }
    }

    /// <summary>Crea directamente en la base de datos un usuario con contraseña y roles.</summary>
    public Task<Usuario> CrearUsuarioAsync(string email, bool tieneAcceso = true, int? aliadoId = null, params string[] roles) =>
        EnBaseDatosAsync(async db =>
        {
            var hasher = Services.GetRequiredService<IPasswordHasher>();
            var rolesBd = await db.Roles.Where(r => roles.Contains(r.Nombre)).ToListAsync();
            var usuario = new Usuario
            {
                Email = email,
                Nombres = "Prueba",
                Apellidos = email,
                PasswordHash = hasher.Hash(PasswordPrueba),
                TieneAcceso = tieneAcceso,
                AliadoId = aliadoId,
                Roles = rolesBd.Select(r => new UsuarioRol { Rol = r }).ToList(),
            };
            db.Usuarios.Add(usuario);
            await db.SaveChangesAsync();
            return usuario;
        });

    public static string Unico(string prefijo) => $"{prefijo}-{Guid.NewGuid():N}"[..20] + "@pruebas.local";

    /// <summary>IP simulada del cliente: TestServer no tiene conexión real y la API toma la IP de X-Forwarded-For.</summary>
    public const string IpCliente = "203.0.113.10";

    protected override void ConfigureClient(HttpClient client)
    {
        base.ConfigureClient(client);
        client.DefaultRequestHeaders.Add("X-Forwarded-For", IpCliente);
    }

    public async Task<HttpResponseMessage> LoginAsync(string email, string password) =>
        await CreateClient().PostAsJsonAsync("/api/v1/auth/login", new { email, password });

    public async Task<Sesion> IniciarSesionAsync(string email, string password = PasswordPrueba)
    {
        var respuesta = await LoginAsync(email, password);
        respuesta.EnsureSuccessStatusCode();
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<RespuestaSesionDto>();
        return new Sesion(cuerpo!.AccessToken, CookieDe(respuesta)!);
    }

    public HttpClient ClienteCon(Sesion sesion)
    {
        var cliente = CreateClient();
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sesion.AccessToken);
        return cliente;
    }

    public async Task<HttpClient> ClienteComoAsync(params string[] roles)
    {
        var email = Unico(string.Join("", roles.Select(r => r[..3])));
        await CrearUsuarioAsync(email, roles: roles);
        return ClienteCon(await IniciarSesionAsync(email));
    }

    public Task<HttpResponseMessage> RefrescarAsync(string? cookie)
    {
        var peticion = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/refresh");
        if (cookie is not null)
            peticion.Headers.Add("Cookie", $"ci2dt2_rt={cookie}");
        return CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = false }).SendAsync(peticion);
    }

    public static string? CookieDe(HttpResponseMessage respuesta)
    {
        if (!respuesta.Headers.TryGetValues("Set-Cookie", out var cookies))
            return null;
        var match = cookies.Select(c => Regex.Match(c, "^ci2dt2_rt=([^;]*)")).FirstOrDefault(m => m.Success);
        return match?.Groups[1].Value;
    }
}

public sealed record UsuarioSesionDto(int Id, string Email, string[] Roles, int[] Programas);

public sealed record RespuestaSesionDto(string AccessToken, DateTime ExpiraEn, UsuarioSesionDto Usuario);

public sealed record ErrorDto(string Error, Dictionary<string, string[]>? Errores, int? SegundosRestantes);

[CollectionDefinition(Nombre)]
public sealed class ColeccionApi : ICollectionFixture<ApiFactory>
{
    public const string Nombre = "api";
}
