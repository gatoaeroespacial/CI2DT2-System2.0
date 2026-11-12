using Ci2dt2.Application.Importacion;

namespace Ci2dt2.Api.Infraestructura;

/// <summary>Interfaz de línea de comandos de la carga inicial (HU-14).</summary>
public static class ConsolaImportacion
{
    public static async Task<int> EjecutarAsync(IServiceProvider servicios, string[] args)
    {
        if (args[0] == "plantilla")
        {
            Console.Write(ImportadorProyectos.Plantilla());
            return 0;
        }

        if (args.Length < 2 || !File.Exists(args[1]))
        {
            Console.Error.WriteLine("Uso: importar <archivo.csv> [--confirmar]");
            return 2;
        }

        var confirmar = args.Contains("--confirmar");
        using var scope = servicios.CreateScope();
        var importador = scope.ServiceProvider.GetRequiredService<ImportadorProyectos>();
        var resultado = await importador.ImportarAsync(await File.ReadAllTextAsync(args[1]), confirmar);

        if (!resultado.Valido)
        {
            Console.Error.WriteLine("El archivo tiene errores; no se importó ningún registro:");
            foreach (var error in resultado.Errores)
                Console.Error.WriteLine("  " + error);
            return 1;
        }

        Console.WriteLine(resultado.Importado ? "Importación completada:" : "Archivo válido (use --confirmar para importar):");
        Console.WriteLine($"  Proyectos: {resultado.Proyectos}");
        Console.WriteLine($"  Integrantes: {resultado.Integrantes}");
        Console.WriteLine($"  Fuentes de financiamiento: {resultado.Fuentes}");
        Console.WriteLine($"  Postulaciones: {resultado.Postulaciones}");
        return 0;
    }
}
