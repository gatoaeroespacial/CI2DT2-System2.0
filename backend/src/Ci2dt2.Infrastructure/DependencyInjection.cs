using Ci2dt2.Application.Asistente;
using Ci2dt2.Application.Common;
using Ci2dt2.Application.Notificaciones;
using Ci2dt2.Application.Seguridad;
using Ci2dt2.Infrastructure.Archivos;
using Ci2dt2.Infrastructure.Asistente;
using Ci2dt2.Infrastructure.Auditoria;
using Ci2dt2.Infrastructure.Email;
using Ci2dt2.Infrastructure.Persistence;
using Ci2dt2.Infrastructure.Seguridad;
using Ci2dt2.Infrastructure.Trabajos;
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
        services.AddScoped<CapturaCambios>();
        services.AddScoped<ICambiosPeticion>(sp => sp.GetRequiredService<CapturaCambios>());
        services.AddDbContext<AppDbContext>((sp, o) => o
            .UseSqlServer(CadenaConexion(sp.GetRequiredService<IConfiguration>()))
            .AddInterceptors(
                new ColumnasAuditoriaInterceptor(sp.GetService<IContextoPeticion>(), sp.GetRequiredService<TimeProvider>()),
                sp.GetRequiredService<CapturaCambios>()));
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();
        services.AddScoped<IRecuperadorConocimientoAsistente, RecuperadorConocimientoAsistente>();
        services.AddSingleton<IDocumentParserService, DocumentParserService>();
        services.AddScoped<IExportadorBorradorService, ExportadorBorradorDocxService>();
        services.AddHttpClient<IGeneradorRespuestaAsistente, GeminiAnswerGenerator>(client =>
            client.Timeout = TimeSpan.FromSeconds(20));
        services.AddHttpClient<IAgenteFormulacionService, GeminiAgenteFormulacionService>(client =>
            client.Timeout = TimeSpan.FromMinutes(2));
        services.AddScoped<InicializadorBaseDatos>();

        services.Configure<OpcionesJwt>(configuracion.GetSection(OpcionesJwt.Seccion));
        services.Configure<OpcionesSmtp>(configuracion.GetSection(OpcionesSmtp.Seccion));
        services.Configure<OpcionesBaseDatos>(configuracion.GetSection(OpcionesBaseDatos.Seccion));
        services.Configure<OpcionesAdminInicial>(configuracion.GetSection(OpcionesAdminInicial.Seccion));
        services.Configure<OpcionesArchivos>(configuracion.GetSection(OpcionesArchivos.Seccion));
        services.Configure<OpcionesFrontend>(configuracion.GetSection(OpcionesFrontend.Seccion));

        // Correo (HU-23, HU-03) y alertas de hitos (HU-18) en segundo plano.
        services.AddSingleton<ColaCorreos>();
        services.AddSingleton<IColaCorreos>(sp => sp.GetRequiredService<ColaCorreos>());
        services.AddHostedService<CorreosDirectosWorker>();
        services.AddHostedService<NotificacionesCorreoWorker>();
        services.AddHostedService<AlertasProgramadasWorker>();

        // Auditoría asíncrona (HU-28): cola en memoria y worker en segundo plano.
        services.AddSingleton<ColaAuditoria>();
        services.AddSingleton<IColaAuditoria>(sp => sp.GetRequiredService<ColaAuditoria>());
        services.AddHostedService<AuditoriaWorker>();

        services.AddSingleton<IAlmacenArchivos, AlmacenArchivosDisco>();
        services.AddSingleton<IEnlaceDescarga, EnlaceDescargaService>();
        services.AddSingleton<ITokenRestablecimiento, TokenRestablecimientoService>();

        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddSingleton<IPasswordHasher, IdentityPasswordHasher>();

        // Llaves persistentes para que las URL firmadas de descarga sobrevivan a los reinicios.
        var dataProtection = services.AddDataProtection().SetApplicationName("Ci2dt2");
        // Sin ruta configurada se usa App_Data/llaves: en hosting sin perfil de usuario las llaves en
        // memoria se perderían en cada reciclaje e invalidarían los enlaces vigentes.
        var rutaLlaves = configuracion["DataProtection:RutaLlaves"];
        dataProtection.PersistKeysToFileSystem(new DirectoryInfo(string.IsNullOrWhiteSpace(rutaLlaves)
            ? Path.Combine(AppContext.BaseDirectory, "App_Data", "llaves")
            : rutaLlaves));

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
