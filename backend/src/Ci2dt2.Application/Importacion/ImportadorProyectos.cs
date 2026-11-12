using System.Globalization;
using System.Text;
using System.Text.Json;
using Ci2dt2.Application.Common;
using Ci2dt2.Application.Convocatorias;
using Ci2dt2.Application.Proyectos;
using Ci2dt2.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ci2dt2.Application.Importacion;

public sealed record ResultadoImportacion(bool Valido, bool Importado, IReadOnlyList<string> Errores, int Proyectos, int Integrantes, int Fuentes, int Postulaciones);

/// <summary>
/// HU-14: carga inicial de proyectos y propuestas desde la plantilla CSV (separador ";", UTF-8).
/// Valida todo el archivo y reporta los errores por fila; si hay errores no importa nada.
/// Columnas: Codigo; Titulo; Resumen; Programa; TipoProyecto; Estado; CorreoIP; FechaInicio; FechaFin;
/// Moneda; PresupuestoTotal; TasaCambio; PorcentajeAvance; Integrantes; Fuentes; Convocatoria;
/// FechaPresentacion; MontoSolicitado; ResultadoPostulacion.
/// Integrantes: "correo:Rol|correo:Rol". Fuentes: "TipoFuente:TipoAporte:Moneda:Monto:Tasa:Estado:NIT|…".
/// </summary>
public sealed class ImportadorProyectos(IApplicationDbContext db, TimeProvider reloj)
{
    public static readonly string[] Columnas =
    [
        "Codigo", "Titulo", "Resumen", "Programa", "TipoProyecto", "Estado", "CorreoIP", "FechaInicio", "FechaFin",
        "Moneda", "PresupuestoTotal", "TasaCambio", "PorcentajeAvance", "Integrantes", "Fuentes", "Convocatoria",
        "FechaPresentacion", "MontoSolicitado", "ResultadoPostulacion",
    ];

    private static readonly CultureInfo Cultura = CultureInfo.InvariantCulture;

    public async Task<ResultadoImportacion> ImportarAsync(string contenidoCsv, bool confirmar, CancellationToken ct = default)
    {
        var filas = LeerCsv(contenidoCsv);
        var errores = new List<string>();
        if (filas.Count == 0 || !filas[0].Select(c => c.Trim()).SequenceEqual(Columnas, StringComparer.OrdinalIgnoreCase))
            return new ResultadoImportacion(false, false, ["Fila 1: el encabezado no coincide con la plantilla: " + string.Join(";", Columnas)], 0, 0, 0, 0);

        var programas = await db.Programas.Where(p => p.Activo).ToDictionaryAsync(p => p.Nombre.ToLowerInvariant(), p => p.Id, ct);
        var tipos = await db.TiposProyecto.Where(t => t.Activo).ToDictionaryAsync(t => t.Nombre.ToLowerInvariant(), t => t.Id, ct);
        var estados = await db.EstadosProyecto.Where(e => e.Activo).ToDictionaryAsync(e => e.Nombre.ToLowerInvariant(), e => e.Id, ct);
        var usuarios = await db.Usuarios.Where(u => u.Activo).ToDictionaryAsync(u => u.Email.ToLowerInvariant(), u => u.Id, ct);
        var aliados = await db.Aliados.Where(a => a.Activo && a.Nit != null).ToDictionaryAsync(a => a.Nit!, a => a.Id, ct);
        var convocatorias = await db.Convocatorias.Where(c => c.Activo).Select(c => new { c.Id, c.Nombre, c.Numero }).ToListAsync(ct);
        var codigosExistentes = (await db.Proyectos.Where(p => p.Codigo != null).Select(p => p.Codigo!).ToListAsync(ct)).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var proyectos = new List<(Proyecto Proyecto, List<ProyectoIntegrante> Integrantes, List<FuenteFinanciamiento> Fuentes, PropuestaConvocatoria? Postulacion)>();
        for (var i = 1; i < filas.Count; i++)
        {
            var n = i + 1;
            var c = filas[i];
            if (c.All(string.IsNullOrWhiteSpace))
                continue;
            if (c.Length != Columnas.Length)
            {
                errores.Add($"Fila {n}: tiene {c.Length} columnas y la plantilla tiene {Columnas.Length}.");
                continue;
            }
            string V(string columna) => c[Array.IndexOf(Columnas, columna)].Trim();
            var erroresFila = new List<string>();

            var titulo = V("Titulo");
            if (titulo.Length is < 10 or > 300)
                erroresFila.Add("el título debe tener entre 10 y 300 caracteres");
            var programaId = Buscar(programas, V("Programa"), "el programa", erroresFila);
            var tipoId = Buscar(tipos, V("TipoProyecto"), "el tipo de proyecto", erroresFila);
            var estadoId = Buscar(estados, V("Estado"), "el estado", erroresFila);
            var correoIp = V("CorreoIP").ToLowerInvariant();
            if (!usuarios.TryGetValue(correoIp, out var ipId))
                erroresFila.Add($"el usuario con correo {correoIp} no existe");
            var inicio = Fecha(V("FechaInicio"), "FechaInicio", erroresFila);
            var fin = Fecha(V("FechaFin"), "FechaFin", erroresFila);
            if (inicio is not null && fin is not null && fin <= inicio)
                erroresFila.Add("la fecha de fin debe ser posterior a la de inicio");
            var moneda = Vacio(V("Moneda"))?.ToUpperInvariant();
            if (moneda is not null && !ValoresProyecto.Monedas.Contains(moneda))
                erroresFila.Add("la moneda debe ser COP, USD o EUR");
            var presupuesto = Numero(V("PresupuestoTotal"), "PresupuestoTotal", erroresFila);
            var tasa = Numero(V("TasaCambio"), "TasaCambio", erroresFila);
            if (moneda is not null and not "COP" && tasa is not > 0)
                erroresFila.Add("la tasa de cambio es obligatoria cuando la moneda no es COP");
            var avance = Numero(V("PorcentajeAvance"), "PorcentajeAvance", erroresFila);
            if (avance is < 0 or > 100)
                erroresFila.Add("el porcentaje de avance debe estar entre 0 y 100");

            var codigo = Vacio(V("Codigo"))?.ToUpperInvariant();
            if (codigo is not null && !codigosExistentes.Add(codigo))
                erroresFila.Add($"el código {codigo} ya existe");

            var proyecto = new Proyecto
            {
                Codigo = codigo,
                Titulo = titulo,
                Resumen = Vacio(V("Resumen")),
                ProgramaId = programaId,
                TipoProyectoId = tipoId,
                EstadoId = estadoId,
                InvestigadorPrincipalId = ipId,
                FechaInicio = inicio,
                FechaFin = fin,
                Moneda = moneda,
                PresupuestoTotal = presupuesto,
                TasaCambio = moneda is null or "COP" ? null : tasa,
                PresupuestoTotalCop = ValoresProyecto.EnCop(presupuesto, moneda, tasa),
                PorcentajeAvance = avance ?? 0,
            };

            // Integrantes: el IP siempre queda como integrante con rol "IP".
            var integrantes = new List<ProyectoIntegrante> { new() { Proyecto = proyecto, UsuarioId = ipId, RolEnProyecto = ValoresProyecto.RolIp, FechaInicio = inicio } };
            foreach (var parte in Lista(V("Integrantes")))
            {
                var datos = parte.Split(':');
                var correo = datos[0].Trim().ToLowerInvariant();
                var rol = datos.Length > 1 ? datos[1].Trim() : "";
                if (!usuarios.TryGetValue(correo, out var uid))
                    erroresFila.Add($"el usuario con correo {correo} no existe");
                else if (!ValoresProyecto.RolesEnProyecto.Contains(rol) || rol == ValoresProyecto.RolIp)
                    erroresFila.Add($"el rol \"{rol}\" del integrante {correo} no es válido (Co-investigador, Auxiliar de campo o Estudiante)");
                else if (uid != ipId)
                    integrantes.Add(new ProyectoIntegrante { Proyecto = proyecto, UsuarioId = uid, RolEnProyecto = rol, FechaInicio = inicio });
            }

            var fuentes = new List<FuenteFinanciamiento>();
            foreach (var parte in Lista(V("Fuentes")))
            {
                var d = parte.Split(':').Select(x => x.Trim()).ToArray();
                if (d.Length < 6)
                {
                    erroresFila.Add($"la fuente \"{parte}\" debe tener TipoFuente:TipoAporte:Moneda:Monto:Tasa:Estado[:NIT]");
                    continue;
                }
                var monto = Numero(d[3], "Monto de la fuente", erroresFila);
                var tasaFuente = Numero(d[4], "Tasa de la fuente", erroresFila);
                int? aliadoId = null;
                if (d.Length > 6 && !string.IsNullOrWhiteSpace(d[6]))
                {
                    if (aliados.TryGetValue(d[6], out var a))
                        aliadoId = a;
                    else
                        erroresFila.Add($"el aliado con NIT {d[6]} no existe");
                }
                if (!ValoresProyecto.TiposFuente.Contains(d[0]) || !ValoresProyecto.TiposAporte.Contains(d[1])
                    || !ValoresProyecto.Monedas.Contains(d[2]) || !ValoresProyecto.EstadosFuente.Contains(d[5]) || monto is not > 0)
                    erroresFila.Add($"la fuente \"{parte}\" tiene valores no válidos");
                else if (d[0] == ValoresProyecto.Cofinanciacion && aliadoId is null)
                    erroresFila.Add("una fuente de Cofinanciación exige el NIT del aliado");
                else if (d[2] != "COP" && tasaFuente is not > 0)
                    erroresFila.Add($"la fuente \"{parte}\" en {d[2]} exige la tasa de cambio");
                else
                    fuentes.Add(new FuenteFinanciamiento
                    {
                        Proyecto = proyecto,
                        AliadoId = aliadoId,
                        TipoFuente = d[0],
                        TipoAporte = d[1],
                        Moneda = d[2],
                        Monto = monto,
                        TasaCambio = d[2] == "COP" ? null : tasaFuente,
                        MontoCop = ValoresProyecto.EnCop(monto, d[2], tasaFuente),
                        Estado = d[5],
                    });
            }

            PropuestaConvocatoria? postulacion = null;
            if (Vacio(V("Convocatoria")) is { } nombreConvocatoria)
            {
                var conv = convocatorias.FirstOrDefault(x => string.Equals(x.Nombre, nombreConvocatoria, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(x.Numero, nombreConvocatoria, StringComparison.OrdinalIgnoreCase));
                var resultado = Vacio(V("ResultadoPostulacion")) ?? "Pendiente";
                if (conv is null)
                    erroresFila.Add($"la convocatoria \"{nombreConvocatoria}\" no existe");
                else if (!ValoresPostulacion.Resultados.Contains(resultado))
                    erroresFila.Add("el resultado de la postulación debe ser Pendiente, Seleccionada o No seleccionada");
                else
                {
                    var montoSolicitado = Numero(V("MontoSolicitado"), "MontoSolicitado", erroresFila);
                    postulacion = new PropuestaConvocatoria
                    {
                        Proyecto = proyecto,
                        ConvocatoriaId = conv.Id,
                        FechaPresentacion = Fecha(V("FechaPresentacion"), "FechaPresentacion", erroresFila),
                        MontoSolicitado = montoSolicitado,
                        Moneda = montoSolicitado is null ? null : "COP",
                        MontoSolicitadoCop = montoSolicitado,
                        Resultado = resultado,
                    };
                }
            }

            if (erroresFila.Count > 0)
                errores.Add($"Fila {n}: {string.Join("; ", erroresFila)}.");
            else
                proyectos.Add((proyecto, integrantes, fuentes, postulacion));
        }

        var totalIntegrantes = proyectos.Sum(p => p.Integrantes.Count);
        var totalFuentes = proyectos.Sum(p => p.Fuentes.Count);
        var totalPostulaciones = proyectos.Count(p => p.Postulacion is not null);
        if (errores.Count > 0)
            return new ResultadoImportacion(false, false, errores, 0, 0, 0, 0);
        if (!confirmar)
            return new ResultadoImportacion(true, false, [], proyectos.Count, totalIntegrantes, totalFuentes, totalPostulaciones);

        // Códigos faltantes: consecutivo CI2DT2-{AAAA}-{NNN} por año de inicio.
        foreach (var (p, _, _, _) in proyectos.Where(x => x.Proyecto.Codigo is null))
        {
            var prefijo = $"CI2DT2-{(p.FechaInicio?.Year ?? reloj.GetUtcNow().Year)}-";
            var siguiente = codigosExistentes.Where(x => x.StartsWith(prefijo, StringComparison.OrdinalIgnoreCase))
                .Select(x => int.TryParse(x[prefijo.Length..], out var k) ? k : 0).DefaultIfEmpty(0).Max() + 1;
            p.Codigo = $"{prefijo}{siguiente:D3}";
            codigosExistentes.Add(p.Codigo);
        }

        foreach (var (p, integrantes, fuentes, postulacion) in proyectos)
        {
            db.Proyectos.Add(p);
            foreach (var i in integrantes)
                db.ProyectoIntegrantes.Add(i);
            foreach (var f in fuentes)
                db.FuentesFinanciamiento.Add(f);
            if (postulacion is not null)
                db.PropuestasConvocatoria.Add(postulacion);
            // Los proyectos importados quedan marcados en el log con origen "Importación".
            db.LogActividades.Add(new LogActividad
            {
                Recurso = "proyectos",
                RecursoId = p.Codigo,
                Accion = "IMPORT",
                Ruta = "importar-csv",
                EstadoHttp = 201,
                ValoresNuevos = JsonSerializer.Serialize(new { origen = "Importación", codigo = p.Codigo, titulo = p.Titulo }, Auditoria.Json),
                FechaUtc = reloj.GetUtcNow().UtcDateTime,
            });
        }
        // Un solo SaveChanges: todo o nada.
        await db.SaveChangesAsync(ct);
        return new ResultadoImportacion(true, true, [], proyectos.Count, totalIntegrantes, totalFuentes, totalPostulaciones);
    }

    /// <summary>Plantilla vacía con el encabezado y una fila de ejemplo.</summary>
    public static string Plantilla() =>
        string.Join(";", Columnas) + "\n" +
        ";Bioinsumos a partir de pulpa de café;Resumen del proyecto;Biotecnología y Agroindustria;I+D+i;En ejecución;ip@ucaldas.edu.co;" +
        "2026-02-01;2027-01-31;COP;120000000;;35;coinvestigador@ucaldas.edu.co:Co-investigador;Convocatoria:Efectivo:COP:100000000::Comprometido|" +
        "Contrapartida:Especie:COP:20000000::Comprometido;;;;\n";

    // ----- Utilidades -----

    private static int Buscar(Dictionary<string, int> catalogo, string valor, string nombre, List<string> errores)
    {
        if (catalogo.TryGetValue(valor.ToLowerInvariant(), out var id))
            return id;
        errores.Add($"{nombre} \"{valor}\" no existe");
        return 0;
    }

    private static string? Vacio(string valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

    private static IEnumerable<string> Lista(string valor) =>
        valor.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static DateOnly? Fecha(string valor, string columna, List<string> errores)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return null;
        if (DateOnly.TryParseExact(valor.Trim(), "yyyy-MM-dd", Cultura, DateTimeStyles.None, out var f))
            return f;
        errores.Add($"{columna} debe tener el formato AAAA-MM-DD");
        return null;
    }

    private static decimal? Numero(string valor, string columna, List<string> errores)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return null;
        if (decimal.TryParse(valor.Trim().Replace(',', '.'), NumberStyles.Number, Cultura, out var d))
            return d;
        errores.Add($"{columna} no es un número válido");
        return null;
    }

    /// <summary>Lector CSV con separador ";" y comillas dobles para valores con separadores o saltos de línea.</summary>
    public static List<string[]> LeerCsv(string contenido)
    {
        var filas = new List<string[]>();
        var fila = new List<string>();
        var valor = new StringBuilder();
        var entreComillas = false;
        contenido = contenido.TrimStart('﻿');
        for (var i = 0; i < contenido.Length; i++)
        {
            var ch = contenido[i];
            if (entreComillas)
            {
                if (ch == '"' && i + 1 < contenido.Length && contenido[i + 1] == '"')
                {
                    valor.Append('"');
                    i++;
                }
                else if (ch == '"')
                    entreComillas = false;
                else
                    valor.Append(ch);
            }
            else if (ch == '"')
                entreComillas = true;
            else if (ch == ';')
            {
                fila.Add(valor.ToString());
                valor.Clear();
            }
            else if (ch is '\n' or '\r')
            {
                if (ch == '\r' && i + 1 < contenido.Length && contenido[i + 1] == '\n')
                    i++;
                fila.Add(valor.ToString());
                valor.Clear();
                filas.Add([.. fila]);
                fila.Clear();
            }
            else
                valor.Append(ch);
        }
        if (valor.Length > 0 || fila.Count > 0)
        {
            fila.Add(valor.ToString());
            filas.Add([.. fila]);
        }
        return filas;
    }
}
