using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Ci2dt2.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ci2dt2.IntegrationTests;

public sealed record RequisitoDto(string Descripcion, bool Cumple);
public sealed record FlujoDto(
    string? EtapaRevision, RequisitoDto[] Requisitos, bool PuedeEnviarRevision, string[] DecisionesDisponibles,
    bool PuedeRegistrarAprobacion, bool PuedeIniciarEjecucion, bool PuedeGestionarHitos, bool PuedeRegistrarAvances,
    bool PuedeGestionarPostulaciones);
public sealed record EstadoFichaDto(int Id, int EstadoId, string Estado, decimal? PorcentajeAvance, decimal? PresupuestoTotal, RevisionDto[] Revisiones);
public sealed record FichaFlujoDto(EstadoFichaDto Proyecto, FlujoDto Flujo);

/// <summary>Escenario común de los Sprints 3 y 4: programa con Coordinador, IP adscrito y Director.</summary>
public sealed record Escenario(int ProgramaId, Usuario Coordinador, Usuario Ip, Usuario Director, HttpClient CliCoordinador, HttpClient CliIp, HttpClient CliDirector);

public static class Sprint3Datos
{
    public static async Task<Escenario> EscenarioAsync(this ApiFactory api)
    {
        var programaId = await api.NuevoProgramaAsync();
        var coordinador = await api.UsuarioAsync("s3coord", "Coordinador");
        var ip = await api.UsuarioAsync("s3ip", "Investigador");
        var director = await api.UsuarioAsync("s3dir", "Director");
        await api.VincularAsync(coordinador.Id, programaId, "Coordinador");
        await api.VincularAsync(ip.Id, programaId, "Investigador");
        return new Escenario(programaId, coordinador, ip, director,
            await api.ClienteDeAsync(coordinador), await api.ClienteDeAsync(ip), await api.ClienteDeAsync(director));
    }

    /// <summary>Propuesta con todos los requisitos de envío (fechas, presupuesto y una fuente).</summary>
    public static async Task<int> PropuestaCompletaAsync(this Escenario e)
    {
        var id = await e.CliIp.CrearPropuestaAsync(e.ProgramaId);
        var actualizar = await e.CliIp.PutAsJsonAsync($"/api/v1/proyectos/{id}", new
        {
            titulo = "Propuesta de investigación completa",
            resumen = "Resumen completo de la propuesta",
            tipoProyectoId = 1,
            fechaInicio = "2027-01-01",
            fechaFin = "2027-12-31",
            moneda = "COP",
            presupuestoTotal = 100_000_000m,
        });
        Assert.Equal(HttpStatusCode.NoContent, actualizar.StatusCode);
        var fuente = await e.CliIp.PostAsJsonAsync($"/api/v1/proyectos/{id}/fuentes", new
        {
            aliadoId = (int?)null,
            tipoFuente = "Convocatoria",
            tipoAporte = "Efectivo",
            moneda = "COP",
            monto = 100_000_000m,
            estado = "Estimado",
        });
        Assert.Equal(HttpStatusCode.Created, fuente.StatusCode);
        return id;
    }

    public static async Task<FichaFlujoDto> FichaAsync(this HttpClient cliente, int proyectoId) =>
        (await cliente.GetFromJsonAsync<FichaFlujoDto>($"/api/v1/proyectos/{proyectoId}"))!;

    public static Task<HttpResponseMessage> RevisarAsync(this HttpClient cliente, int proyectoId, string decision, string? observaciones = null) =>
        cliente.PostAsJsonAsync($"/api/v1/proyectos/{proyectoId}/revisiones", new { decision, observaciones });

    public static async Task<HttpResponseMessage> SubirDocumentoAsync(this HttpClient cliente, string entidadTipo, int entidadId, int tipoDocumentoId, string nombre = "documento.pdf")
    {
        var archivo = new ByteArrayContent("%PDF-1.4 documento de prueba"u8.ToArray());
        archivo.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        return await cliente.PostAsync("/api/v1/documentos", new MultipartFormDataContent
        {
            { archivo, "archivo", nombre },
            { new StringContent(tipoDocumentoId.ToString()), "tipoDocumentoId" },
            { new StringContent(entidadTipo), "entidadTipo" },
            { new StringContent(entidadId.ToString()), "entidadId" },
        });
    }

    /// <summary>Lleva el proyecto a un estado con cronograma (Aprobado o En ejecución) directamente en la base.</summary>
    public static Task<int> ConCronogramaAsync(this ApiFactory api, int proyectoId, int estadoId) =>
        api.EnBaseDatosAsync(async db =>
        {
            await db.Proyectos.Where(p => p.Id == proyectoId).ExecuteUpdateAsync(s => s.SetProperty(p => p.EstadoId, estadoId));
            var cronograma = new Cronograma { ProyectoId = proyectoId, Nombre = "Cronograma de prueba" };
            db.Cronogramas.Add(cronograma);
            await db.SaveChangesAsync();
            return cronograma.Id;
        });

    public static Task<int> HitoAsync(this ApiFactory api, int cronogramaId, int responsableId, DateOnly fechaLimite, int alertarDias = 7, int tipoHitoId = 1) =>
        api.EnBaseDatosAsync(async db =>
        {
            var hito = new Hito
            {
                CronogramaId = cronogramaId,
                TipoHitoId = tipoHitoId,
                Titulo = "Hito de prueba",
                FechaLimite = fechaLimite,
                ResponsableId = responsableId,
                AlertarDias = alertarDias,
                Estado = "Pendiente",
            };
            db.Hitos.Add(hito);
            await db.SaveChangesAsync();
            return hito.Id;
        });

    public static Task<bool> TieneNotificacionAsync(this ApiFactory api, int usuarioId, string tipo, int? referenciaId = null) =>
        api.EnBaseDatosAsync(db => db.Notificaciones.AnyAsync(n => n.UsuarioId == usuarioId && n.Tipo == tipo
            && (referenciaId == null || n.ReferenciaId == referenciaId)));
}
