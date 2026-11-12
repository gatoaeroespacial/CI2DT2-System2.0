using Ci2dt2.Application.Aliados;
using Ci2dt2.Application.Asistente;
using Ci2dt2.Application.Avances;
using Ci2dt2.Application.Catalogos;
using Ci2dt2.Application.Common;
using Ci2dt2.Application.Convocatorias;
using Ci2dt2.Application.Cronogramas;
using Ci2dt2.Application.Documentos;
using Ci2dt2.Application.Importacion;
using Ci2dt2.Application.Notificaciones;
using Ci2dt2.Application.Programas;
using Ci2dt2.Application.Proyectos;
using Ci2dt2.Application.Publico;
using Ci2dt2.Application.Seguridad;
using Ci2dt2.Application.Tablero;
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
        services.AddScoped<AsistenteConsultas>();
        services.AddScoped<AuthService>();
        services.AddScoped<UsuariosService>();
        services.AddScoped<UsuariosQueries>();
        services.AddScoped<Notificador>();
        services.AddScoped<AliadosQueries>();
        services.AddScoped<AliadosService>();
        services.AddScoped<CatalogosQueries>();
        services.AddScoped<ProgramasQueries>();
        services.AddScoped<ProgramasService>();
        services.AddScoped<ProyectosQueries>();
        services.AddScoped<ProyectosService>();
        services.AddScoped<BorradorProyectosService>();
        services.AddScoped<DocumentosService>();
        services.AddScoped<RestablecimientoContrasenaService>();
        services.AddScoped<ConsentimientosService>();
        services.AddScoped<NotificacionesService>();
        services.AddScoped<DespachadorCorreos>();
        services.AddScoped<FlujoProyectoService>();
        services.AddScoped<ConvocatoriasService>();
        services.AddScoped<PostulacionesService>();
        services.AddScoped<HitosService>();
        services.AddScoped<AlertasService>();
        services.AddScoped<AvancesService>();
        services.AddScoped<TableroQueries>();
        services.AddScoped<PortalQueries>();
        services.AddScoped<ImportadorProyectos>();
        return services;
    }
}
