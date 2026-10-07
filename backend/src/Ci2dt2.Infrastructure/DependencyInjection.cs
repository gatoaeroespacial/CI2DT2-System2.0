using Ci2dt2.Application.Common;
using Ci2dt2.Application.Seguridad;
using Ci2dt2.Infrastructure.Email;
using Ci2dt2.Infrastructure.Persistence;
using Ci2dt2.Infrastructure.Seguridad;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ci2dt2.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuracion)
    {
        // La configuración se lee al resolver los servicios (permite sobrescribirla en pruebas).
        services.AddDbContext<AppDbContext>((sp, o) => o.UseSqlServer(CadenaConexion(sp.GetRequiredService<IConfiguration>())));
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();
        services.AddScoped<InicializadorBaseDatos>();

        services.Configure<OpcionesJwt>(configuracion.GetSection(OpcionesJwt.Seccion));
        services.Configure<OpcionesSmtp>(configuracion.GetSection(OpcionesSmtp.Seccion));
        services.Configure<OpcionesBaseDatos>(configuracion.GetSection(OpcionesBaseDatos.Seccion));
        services.Configure<OpcionesAdminInicial>(configuracion.GetSection(OpcionesAdminInicial.Seccion));
        services.Configure<OpcionesFrontend>(configuracion.GetSection(OpcionesFrontend.Seccion));

        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddSingleton<IPasswordHasher, IdentityPasswordHasher>();
        services.AddSingleton<IEnlaceContrasenaService, EnlaceContrasenaService>();

        // Llaves persistentes para que los enlaces de contraseña sobrevivan a los reinicios.
        var dataProtection = services.AddDataProtection().SetApplicationName("Ci2dt2");
        var rutaLlaves = configuracion["DataProtection:RutaLlaves"];
        if (!string.IsNullOrWhiteSpace(rutaLlaves))
            dataProtection.PersistKeysToFileSystem(new DirectoryInfo(rutaLlaves));

        services.AddSingleton<SmtpEmailSender>();
        services.AddSingleton<LogEmailSender>();
        services.AddSingleton<IEmailSender>(sp =>
            string.IsNullOrWhiteSpace(sp.GetRequiredService<IConfiguration>()[$"{OpcionesSmtp.Seccion}:Host"])
                ? sp.GetRequiredService<LogEmailSender>()
                : sp.GetRequiredService<SmtpEmailSender>());

        return services;
    }

    public static string CadenaConexion(IConfiguration configuracion) =>
        configuracion.GetConnectionString("Default")
        ?? throw new InvalidOperationException("Falta la cadena de conexión ConnectionStrings:Default.");
}
