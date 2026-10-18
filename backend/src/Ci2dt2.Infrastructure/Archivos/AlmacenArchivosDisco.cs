using Ci2dt2.Application.Common;
using Microsoft.Extensions.Options;

namespace Ci2dt2.Infrastructure.Archivos;

public sealed class OpcionesArchivos
{
    public const string Seccion = "Archivos";

    /// <summary>
    /// Carpeta donde se guardan los documentos, fuera de wwwroot (HU-21). Si está vacía se usa
    /// App_Data/documentos dentro de la aplicación. D-7 define el almacenamiento definitivo.
    /// </summary>
    public string? Ruta { get; set; }
}

/// <summary>Repositorio de archivos en disco. Los nombres físicos son aleatorios y nunca se exponen.</summary>
internal sealed class AlmacenArchivosDisco(IOptions<OpcionesArchivos> opciones) : IAlmacenArchivos
{
    private string Raiz => string.IsNullOrWhiteSpace(opciones.Value.Ruta)
        ? Path.Combine(AppContext.BaseDirectory, "App_Data", "documentos")
        : opciones.Value.Ruta!;

    public async Task<string> GuardarAsync(Stream contenido, string extension, CancellationToken ct = default)
    {
        var ahora = DateTime.UtcNow;
        var clave = $"{ahora:yyyy}/{ahora:MM}/{Guid.NewGuid():N}{extension}";
        var ruta = RutaFisica(clave);
        Directory.CreateDirectory(Path.GetDirectoryName(ruta)!);
        await using var archivo = new FileStream(ruta, FileMode.CreateNew, FileAccess.Write);
        await contenido.CopyToAsync(archivo, ct);
        return clave;
    }

    public Stream Abrir(string clave) => new FileStream(RutaFisica(clave), FileMode.Open, FileAccess.Read, FileShare.Read);

    private string RutaFisica(string clave)
    {
        var raiz = Path.GetFullPath(Raiz);
        var ruta = Path.GetFullPath(Path.Combine(raiz, clave));
        if (!ruta.StartsWith(raiz, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Ruta de archivo no válida.");
        return ruta;
    }
}
