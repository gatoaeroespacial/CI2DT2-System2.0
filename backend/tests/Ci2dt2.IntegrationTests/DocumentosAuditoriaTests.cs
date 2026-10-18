using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using Ci2dt2.Application.Common;
using Ci2dt2.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Ci2dt2.IntegrationTests;

public sealed record DocumentoDto(int Id, string TipoDocumento, string NombreArchivo, string HashSha256, string SubidoPor, DateTime FechaSubida);
public sealed record EnlaceDto(string Url, DateTime ExpiraEn);

/// <summary>HU-21 Repositorio documental · HU-28 Registro automático de auditoría.</summary>
[Collection(ColeccionApi.Nombre)]
public class DocumentosAuditoriaTests(ApiFactory api)
{
    private const int PlanDeTrabajo = 2;

    private static MultipartFormDataContent Formulario(byte[] contenido, string nombre, int entidadId, string entidadTipo = "Proyecto", int tipo = PlanDeTrabajo)
    {
        var archivo = new ByteArrayContent(contenido);
        archivo.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        return new MultipartFormDataContent
        {
            { archivo, "archivo", nombre },
            { new StringContent(tipo.ToString()), "tipoDocumentoId" },
            { new StringContent(entidadTipo), "entidadTipo" },
            { new StringContent(entidadId.ToString()), "entidadId" },
        };
    }

    private async Task<(int ProyectoId, HttpClient Ip)> ProyectoAsync()
    {
        var programaId = await api.NuevoProgramaAsync();
        var investigador = await api.UsuarioAsync("docip", "Investigador");
        await api.VincularAsync(investigador.Id, programaId, "Investigador");
        var ip = await api.ClienteDeAsync(investigador);
        return (await ip.CrearPropuestaAsync(programaId), ip);
    }

    [Fact]
    public async Task HU21_CA1_CA3_carga_un_pdf_de_3_mb_y_la_descarga_conserva_el_hash()
    {
        var (proyectoId, ip) = await ProyectoAsync();
        var contenido = RandomNumberGenerator.GetBytes(3 * 1024 * 1024);
        var hashEsperado = Convert.ToHexString(SHA256.HashData(contenido));

        var carga = await ip.PostAsync("/api/v1/documentos", Formulario(contenido, "plan-de-trabajo.pdf", proyectoId));
        Assert.Equal(HttpStatusCode.Created, carga.StatusCode);
        var id = (await carga.Content.ReadFromJsonAsync<CreadoDto>())!.Id;

        var documentos = await ip.GetFromJsonAsync<DocumentoDto[]>($"/api/v1/documentos?entidadTipo=Proyecto&entidadId={proyectoId}");
        var documento = Assert.Single(documentos!);
        Assert.Equal(("Plan de trabajo", "plan-de-trabajo.pdf", hashEsperado), (documento.TipoDocumento, documento.NombreArchivo, documento.HashSha256));
        Assert.False(string.IsNullOrEmpty(documento.SubidoPor));

        // CA-3: la descarga por URL firmada devuelve el mismo contenido (mismo SHA-256).
        var enlace = await ip.GetFromJsonAsync<EnlaceDto>($"/api/v1/documentos/{id}/descarga");
        Assert.StartsWith("/api/v1/documentos/archivo?token=", enlace!.Url);
        var descarga = await api.CreateClient().GetAsync(enlace.Url);
        Assert.Equal(HttpStatusCode.OK, descarga.StatusCode);
        Assert.Equal(hashEsperado, Convert.ToHexString(SHA256.HashData(await descarga.Content.ReadAsByteArrayAsync())));

        // Un token alterado no da acceso al archivo.
        Assert.Equal(HttpStatusCode.NotFound, (await api.CreateClient().GetAsync("/api/v1/documentos/archivo?token=alterado")).StatusCode);
    }

    [Fact]
    public async Task HU21_CA2_un_usuario_sin_permiso_sobre_el_proyecto_recibe_403()
    {
        var (proyectoId, ip) = await ProyectoAsync();
        var carga = await ip.PostAsync("/api/v1/documentos", Formulario([1, 2, 3], "acta.pdf", proyectoId));
        var id = (await carga.Content.ReadFromJsonAsync<CreadoDto>())!.Id;
        var ajeno = await api.ClienteComoAsync("Investigador");

        Assert.Equal(HttpStatusCode.Forbidden, (await ajeno.GetAsync($"/api/v1/documentos/{id}/descarga")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await ajeno.GetAsync($"/api/v1/documentos?entidadTipo=Proyecto&entidadId={proyectoId}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await ajeno.PostAsync("/api/v1/documentos", Formulario([1], "x.pdf", proyectoId))).StatusCode);

        var externo = await api.ClienteComoAsync("Externo");
        Assert.Equal(HttpStatusCode.Forbidden, (await externo.GetAsync($"/api/v1/documentos/{id}/descarga")).StatusCode);

        // El Director sí descarga.
        var director = await api.ClienteComoAsync("Director");
        Assert.Equal(HttpStatusCode.OK, (await director.GetAsync($"/api/v1/documentos/{id}/descarga")).StatusCode);
    }

    [Fact]
    public async Task HU21_formato_y_tamano_maximo()
    {
        var (proyectoId, ip) = await ProyectoAsync();

        var exe = await ip.PostAsync("/api/v1/documentos", Formulario([1, 2], "programa.exe", proyectoId));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, exe.StatusCode);

        var grande = await ip.PostAsync("/api/v1/documentos", Formulario(new byte[25 * 1024 * 1024 + 1], "grande.pdf", proyectoId));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, grande.StatusCode);
        Assert.Contains("25 MB", (await grande.Content.ReadFromJsonAsync<ErrorDto>())!.Error);
    }

    [Fact]
    public async Task HU28_CA1_la_edicion_de_un_proyecto_registra_valores_anteriores_y_nuevos()
    {
        var (proyectoId, ip) = await ProyectoAsync();

        await ip.PutAsJsonAsync($"/api/v1/proyectos/{proyectoId}", new { titulo = "Título nuevo para auditoría", tipoProyectoId = 1 });

        var registro = Assert.Single(await api.EsperarLogsAsync(l => l.Recurso == "proyectos" && l.Accion == "PUT" && l.RecursoId == proyectoId.ToString()));
        Assert.Equal(204, registro.EstadoHttp);
        Assert.Contains("\"titulo\":\"Propuesta de investigación de prueba\"", registro.ValoresAnteriores);
        Assert.Contains("\"titulo\":\"Título nuevo para auditoría\"", registro.ValoresNuevos);
        Assert.NotNull(registro.UsuarioId);
        Assert.Equal(ApiFactory.IpCliente, registro.IpOrigen);
        Assert.Null(registro.HashEncadenado);
    }

    [Fact]
    public async Task HU28_CA2_un_fallo_al_escribir_la_auditoria_no_detiene_el_registro()
    {
        var cola = api.Services.GetRequiredService<IColaAuditoria>();
        var marca = "prueba-" + Guid.NewGuid().ToString("N")[..8];

        // Registro inválido (Recurso obligatorio) seguido de uno válido: el worker sigue funcionando.
        cola.Encolar(new LogActividad { Recurso = null!, Accion = "POST", FechaUtc = DateTime.UtcNow });
        cola.Encolar(new LogActividad { Recurso = marca, Accion = "POST", FechaUtc = DateTime.UtcNow });

        Assert.Single(await api.EsperarLogsAsync(l => l.Recurso == marca));
    }

    [Fact]
    public async Task HU28_los_eventos_de_autenticacion_quedan_registrados()
    {
        var email = ApiFactory.Unico("authev");
        await api.CrearUsuarioAsync(email, roles: ["Investigador"]);
        var sesion = await api.IniciarSesionAsync(email);
        var desde = DateTime.UtcNow.AddSeconds(-5);

        await api.RefrescarAsync(sesion.CookieRefresco);

        var registros = await api.EsperarLogsAsync(l => l.Ruta == "POST /api/v1/auth/refresh" && l.FechaUtc >= desde);
        Assert.Contains(registros, r => r.Accion == "POST" && r.EstadoHttp == 200 && r.IpOrigen == ApiFactory.IpCliente);
    }
}
