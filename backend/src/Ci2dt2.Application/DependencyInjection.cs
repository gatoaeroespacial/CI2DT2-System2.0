using Ci2dt2.Application.Aliados;
using Ci2dt2.Application.Common;
using Ci2dt2.Application.Seguridad;
using Ci2dt2.Application.Usuarios;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Ci2dt2.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>(includeInternalTypes: true);
        services.AddMemoryCache();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<ControlIntentosLogin>();

        services.AddScoped<Auditoria>();
        services.AddScoped<AccesoRecursos>();
        services.AddScoped<AuthService>();
        services.AddScoped<UsuariosService>();
        services.AddScoped<UsuariosQueries>();
        services.AddScoped<AliadosQueries>();
        return services;
    }
}
