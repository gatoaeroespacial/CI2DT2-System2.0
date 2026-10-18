using System.Net.Http.Json;
using Ci2dt2.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ci2dt2.IntegrationTests;

public sealed record CreadoDto(int Id);

public sealed record PaginaProyectosDto(ProyectoItemDto[] Items, int Total, int PaginaActual, int TamanoPagina);

public sealed record ProyectoItemDto(
    int Id, string? Codigo, string Titulo, int ProgramaId, string Programa, int InvestigadorPrincipalId,
    string Estado, decimal? PorcentajeAvance, DateTime? ProximoHitoFecha, string? ProximoHitoTitulo);

/// <summary>Utilidades de datos para las pruebas del Sprint 2.</summary>
public static class Sprint2Datos
{
    /// <summary>Programa propio de la prueba para no compartir Coordinadores vigentes entre pruebas.</summary>
    public static Task<int> NuevoProgramaAsync(this ApiFactory api) =>
        api.EnBaseDatosAsync(async db =>
        {
            var programa = new Programa { Nombre = "Programa de prueba " + Guid.NewGuid().ToString("N")[..8] };
            db.Programas.Add(programa);
            await db.SaveChangesAsync();
            return programa.Id;
        });

    public static Task<Usuario> UsuarioAsync(this ApiFactory api, string prefijo, params string[] roles) =>
        api.CrearUsuarioAsync(ApiFactory.Unico(prefijo), roles: roles);

    public static Task VincularAsync(this ApiFactory api, int usuarioId, int programaId, string rol, DateOnly? fin = null) =>
        api.EnBaseDatosAsync(db =>
        {
            db.ProgramaIntegrantes.Add(new ProgramaIntegrante
            {
                UsuarioId = usuarioId,
                ProgramaId = programaId,
                RolEnPrograma = rol,
                FechaInicio = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-1),
                FechaFin = fin,
            });
            return db.SaveChangesAsync();
        });

    public static async Task<HttpClient> ClienteDeAsync(this ApiFactory api, Usuario usuario) =>
        api.ClienteCon(await api.IniciarSesionAsync(usuario.Email));

    /// <summary>Crea una propuesta por la API y devuelve su Id.</summary>
    public static async Task<int> CrearPropuestaAsync(this HttpClient cliente, int programaId, int? ipId = null)
    {
        var respuesta = await cliente.PostAsJsonAsync("/api/v1/proyectos", new
        {
            titulo = "Propuesta de investigación de prueba",
            resumen = "Resumen de la propuesta",
            programaId,
            tipoProyectoId = 1,
            investigadorPrincipalId = ipId,
        });
        Assert.Equal(System.Net.HttpStatusCode.Created, respuesta.StatusCode);
        return (await respuesta.Content.ReadFromJsonAsync<CreadoDto>())!.Id;
    }

    public static Task CambiarEstadoAsync(this ApiFactory api, int proyectoId, int estadoId) =>
        api.EnBaseDatosAsync(db => db.Proyectos.Where(p => p.Id == proyectoId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.EstadoId, estadoId)));
}
