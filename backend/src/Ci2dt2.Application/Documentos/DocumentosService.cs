using System.Security.Cryptography;
using Ci2dt2.Application.Common;
using Ci2dt2.Application.Seguridad;
using Ci2dt2.Domain.Entities;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace Ci2dt2.Application.Documentos;

public sealed record DocumentoItem(
    int Id, int TipoDocumentoId, string TipoDocumento, string NombreArchivo, string HashSha256,
    int Version, bool EsPublico, string SubidoPor, DateTime FechaSubida);

public sealed record EnlaceDescarga(string Url, DateTime ExpiraEn);

public sealed record ArchivoDescarga(Stream Contenido, string NombreArchivo, string TipoContenido);

/// <summary>Repositorio documental de proyectos (HU-21).</summary>
public sealed class DocumentosService(
    IApplicationDbContext db,
    IDbConnectionFactory conexiones,
    IAlmacenArchivos almacen,
    IEnlaceDescarga enlaces,
    AccesoRecursos acceso,
    TimeProvider reloj)
{
    public const long TamanoMaximo = 25L * 1024 * 1024;
    public static readonly TimeSpan VigenciaDescarga = TimeSpan.FromMinutes(5);

    // Modelo genérico por EntidadTipo / EntidadId.
    public static readonly string[] EntidadesPermitidas = ["Proyecto", "Convocatoria", "PropuestaConvocatoria", "Avance"];

    private static readonly Dictionary<string, string> Formatos = new(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = "application/pdf",
        [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        [".xlsx"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        [".pptx"] = "application/vnd.openxmlformats-officedocument.presentationml.presentation",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".png"] = "image/png",
        [".zip"] = "application/zip",
    };

    public async Task<int> CargarAsync(
        string entidadTipo, int entidadId, int tipoDocumentoId, string nombreArchivo, long tamano, Stream contenido,
        UsuarioAutenticado actor, CancellationToken ct = default)
    {
        await ValidarAccesoAsync(entidadTipo, entidadId, actor, ct);

        var extension = Path.GetExtension(nombreArchivo);
        if (!Formatos.ContainsKey(extension))
            throw new ReglaNegocioException("Formato no permitido. Use PDF, DOCX, XLSX, PPTX, JPG, PNG o ZIP.");
        if (tamano <= 0)
            throw new ReglaNegocioException("El archivo está vacío.");
        if (tamano > TamanoMaximo)
            throw new ReglaNegocioException("El archivo supera el tamaño máximo de 25 MB.");
        if (!await db.TiposDocumento.AnyAsync(t => t.Id == tipoDocumentoId && t.Activo, ct))
            throw new ReglaNegocioException("El tipo de documento no existe.");

        // Se calcula el SHA-256 del contenido para verificar la integridad.
        using var memoria = new MemoryStream();
        await contenido.CopyToAsync(memoria, ct);
        if (memoria.Length > TamanoMaximo)
            throw new ReglaNegocioException("El archivo supera el tamaño máximo de 25 MB.");
        memoria.Position = 0;
        var hash = Convert.ToHexString(await SHA256.HashDataAsync(memoria, ct));
        memoria.Position = 0;
        var clave = await almacen.GuardarAsync(memoria, extension.ToLowerInvariant(), ct);

        var nombre = Path.GetFileName(nombreArchivo);
        var documento = new Documento
        {
            TipoDocumentoId = tipoDocumentoId,
            EntidadTipo = entidadTipo,
            EntidadId = entidadId,
            NombreArchivo = nombre.Length > 200 ? nombre[^200..] : nombre,
            UrlArchivo = clave,
            HashSha256 = hash,
            Version = 1, // Sin control de versiones en el MVP: cada carga crea un documento nuevo.
            EsPublico = false,
            SubidoPorId = actor.Id,
            FechaSubida = reloj.GetUtcNow().UtcDateTime,
        };
        db.Documentos.Add(documento);
        await db.SaveChangesAsync(ct);
        return documento.Id;
    }

    public async Task<IReadOnlyList<DocumentoItem>> ListarAsync(string entidadTipo, int entidadId, UsuarioAutenticado actor, CancellationToken ct = default)
    {
        await ValidarAccesoAsync(entidadTipo, entidadId, actor, ct);
        const string sql = """
            SELECT d.Id, d.TipoDocumentoId, t.Nombre AS TipoDocumento, d.NombreArchivo, d.HashSha256, d.Version, d.EsPublico,
                   u.Nombres + ' ' + u.Apellidos AS SubidoPor, d.FechaSubida
            FROM Documentos d JOIN TiposDocumento t ON t.Id = d.TipoDocumentoId JOIN Usuarios u ON u.Id = d.SubidoPorId
            WHERE d.EntidadTipo = @entidadTipo AND d.EntidadId = @entidadId AND d.Activo = 1
            ORDER BY d.FechaSubida DESC, d.Id DESC
            """;
        using var conexion = conexiones.Crear();
        return (await conexion.QueryAsync<DocumentoItem>(new CommandDefinition(sql, new { entidadTipo, entidadId }, cancellationToken: ct))).ToList();
    }

    /// <summary>La descarga exige permiso sobre la entidad y usa una URL firmada temporal.</summary>
    public async Task<EnlaceDescarga> GenerarDescargaAsync(int id, UsuarioAutenticado actor, CancellationToken ct = default)
    {
        var documento = await db.Documentos.FirstOrDefaultAsync(d => d.Id == id && d.Activo, ct)
            ?? throw new NoEncontradoException("El documento no existe.");
        if (documento.EntidadTipo is null || documento.EntidadId is null)
            throw new AccesoDenegadoException();
        await ValidarAccesoAsync(documento.EntidadTipo, documento.EntidadId.Value, actor, ct);

        var token = enlaces.Generar(id, VigenciaDescarga);
        return new EnlaceDescarga($"/api/v1/documentos/archivo?token={Uri.EscapeDataString(token)}",
            reloj.GetUtcNow().UtcDateTime.Add(VigenciaDescarga));
    }

    public async Task<ArchivoDescarga> AbrirAsync(string token, CancellationToken ct = default)
    {
        var id = enlaces.Validar(token) ?? throw new NoEncontradoException("El enlace de descarga no es válido o expiró.");
        var documento = await db.Documentos.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id && d.Activo, ct)
            ?? throw new NoEncontradoException("El documento no existe.");
        var tipo = Formatos.GetValueOrDefault(Path.GetExtension(documento.NombreArchivo), "application/octet-stream");
        return new ArchivoDescarga(almacen.Abrir(documento.UrlArchivo), documento.NombreArchivo, tipo);
    }

    /// <summary>
    /// Anexo A, "Cargar / descargar documentos": Admin y Director sí; Coordinador su programa;
    /// Investigador sus proyectos; Externo no. Los documentos de convocatorias los gestionan
    /// quienes gestionan convocatorias.
    /// </summary>
    private async Task ValidarAccesoAsync(string entidadTipo, int entidadId, UsuarioAutenticado actor, CancellationToken ct)
    {
        if (!EntidadesPermitidas.Contains(entidadTipo))
            throw new ReglaNegocioException("El tipo de entidad no es válido.");
        if (!actor.Roles.Any(r => Politicas.Matriz[Politicas.GestionarDocumentos].Contains(r)))
            throw new AccesoDenegadoException();

        int? proyectoId = entidadTipo switch
        {
            "Proyecto" => await db.Proyectos.Where(p => p.Id == entidadId && p.Activo).Select(p => (int?)p.Id).FirstOrDefaultAsync(ct),
            "PropuestaConvocatoria" => await db.PropuestasConvocatoria.Where(p => p.Id == entidadId && p.Activo).Select(p => (int?)p.ProyectoId).FirstOrDefaultAsync(ct),
            "Avance" => await db.Avances.Where(a => a.Id == entidadId && a.Activo).Select(a => (int?)a.ProyectoId).FirstOrDefaultAsync(ct),
            _ => null,
        };

        if (entidadTipo == "Convocatoria")
        {
            if (!await db.Convocatorias.AnyAsync(c => c.Id == entidadId && c.Activo, ct))
                throw new NoEncontradoException("La convocatoria no existe.");
            if (!actor.Roles.Any(r => Politicas.Matriz[Politicas.GestionarAliadosConvocatorias].Contains(r)))
                throw new AccesoDenegadoException();
            return;
        }

        if (proyectoId is null)
            throw new NoEncontradoException("La entidad del documento no existe.");
        if (!await acceso.PuedeVerProyectoAsync(actor, proyectoId.Value, ct))
            throw new AccesoDenegadoException();
    }
}
