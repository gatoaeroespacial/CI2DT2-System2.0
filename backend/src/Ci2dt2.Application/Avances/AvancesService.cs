using Ci2dt2.Application.Common;
using Ci2dt2.Application.Proyectos;
using Ci2dt2.Application.Seguridad;
using Ci2dt2.Domain;
using Ci2dt2.Domain.Entities;
using Dapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Ci2dt2.Application.Avances;

public static class EstadosAvance
{
    public const string Pendiente = "Pendiente";
    public const string Validado = "Validado";
    public const string Rechazado = "Rechazado";

    public const string RolIp = "IP";
    public const string RolCoordinador = "Coordinador";
    public const string RolDirector = "Director";
}

public sealed record AvanceRequest(string Titulo, string? Descripcion, decimal? PorcentajeAvance, int? HitoId, Guid? IdCliente);

public sealed record ValidacionRequest(string Resultado, string? Observaciones);

public sealed record AvanceGuardado(int Id, bool Existente);

public sealed record AdjuntoItem(int Id, string Nombre, string? TipoMime);

public sealed record ValidacionItem(int Id, string Validador, string RolValidador, string? Resultado, string? Observaciones, DateTime Fecha);

public sealed class AvanceDetalle
{
    public int Id { get; init; }
    public int ProyectoId { get; init; }
    public int? HitoId { get; init; }
    public string? Hito { get; init; }
    public int AutorId { get; init; }
    public string Autor { get; init; } = "";
    public string Titulo { get; init; } = "";
    public string? Descripcion { get; init; }
    public decimal? PorcentajeAvance { get; init; }
    public DateTime? FechaCaptura { get; init; }
    public string? EstadoValidacion { get; init; }
    public IReadOnlyList<AdjuntoItem> Adjuntos { get; set; } = [];
    public IReadOnlyList<ValidacionItem> Validaciones { get; set; } = [];
}

public sealed class AvancePendiente
{
    public int Id { get; init; }
    public int ProyectoId { get; init; }
    public string? Codigo { get; init; }
    public string Proyecto { get; init; } = "";
    public string Programa { get; init; } = "";
    public string Titulo { get; init; } = "";
    public string Autor { get; init; } = "";
    public decimal? PorcentajeAvance { get; init; }
    public DateTime? FechaCaptura { get; init; }
    public string? EstadoValidacion { get; init; }
    public string RolValidador { get; init; } = "";
}

public sealed record BandejaAvances(int Total, IReadOnlyList<AvancePendiente> Items);

public sealed record ArchivoAdjunto(Stream Contenido, string Nombre, string TipoContenido);

public sealed class AvanceRequestValidator : AbstractValidator<AvanceRequest>
{
    public AvanceRequestValidator()
    {
        RuleFor(x => x.Titulo).NotEmpty().WithMessage("El título es obligatorio.").MaximumLength(200);
        RuleFor(x => x.Descripcion).MaximumLength(4000);
        RuleFor(x => x.PorcentajeAvance).NotNull().WithMessage("El porcentaje de avance es obligatorio.")
            .InclusiveBetween(0, 100).WithMessage("El porcentaje de avance debe estar entre 0 y 100.");
    }
}

public sealed class ValidacionRequestValidator : AbstractValidator<ValidacionRequest>
{
    public ValidacionRequestValidator()
    {
        RuleFor(x => x.Resultado).Must(r => r is EstadosAvance.Validado or EstadosAvance.Rechazado)
            .WithMessage("El resultado debe ser Validado o Rechazado.");
        // CA-3: rechazar exige observaciones.
        RuleFor(x => x.Observaciones).NotEmpty().WithMessage("Para rechazar el avance escriba las observaciones.")
            .When(x => x.Resultado == EstadosAvance.Rechazado);
        RuleFor(x => x.Observaciones).MaximumLength(4000);
    }
}

/// <summary>HU-19 (registrar avances con adjuntos) y HU-20 (validar avances, sin rúbrica en el MVP).</summary>
public sealed class AvancesService(
    IApplicationDbContext db,
    IDbConnectionFactory conexiones,
    IAlmacenArchivos almacen,
    IEnlaceDescarga enlaces,
    AccesoRecursos acceso,
    FlujoProyectoService flujo,
    Notificador notificador,
    TimeProvider reloj)
{
    public const int MaxAdjuntos = 5;
    public const long TamanoMaximoAdjunto = 20L * 1024 * 1024;

    // PDF, DOCX, XLSX, CSV, JPG y PNG.
    public static readonly Dictionary<string, string> Formatos = new(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = "application/pdf",
        [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        [".xlsx"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        [".csv"] = "text/csv",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".png"] = "image/png",
    };

    private DateTime Ahora => reloj.GetUtcNow().UtcDateTime;

    // ----- HU-19 -----

    public async Task<IReadOnlyList<AvanceDetalle>> ListarAsync(int proyectoId, CancellationToken ct = default)
    {
        const string sql = """
            SELECT a.Id, a.ProyectoId, a.HitoId, h.Titulo AS Hito, a.AutorId, u.Nombres + ' ' + u.Apellidos AS Autor,
                   a.Titulo, a.Descripcion, a.PorcentajeAvance, a.FechaCaptura, a.EstadoValidacion
            FROM Avances a JOIN Usuarios u ON u.Id = a.AutorId LEFT JOIN Hitos h ON h.Id = a.HitoId
            WHERE a.ProyectoId = @proyectoId AND a.Activo = 1
            ORDER BY a.FechaCaptura DESC, a.Id DESC;

            SELECT ad.Id, ad.AvanceId, ad.UrlArchivo, ad.TipoMime
            FROM AvancesAdjuntos ad JOIN Avances a ON a.Id = ad.AvanceId
            WHERE a.ProyectoId = @proyectoId AND a.Activo = 1 AND ad.Activo = 1 ORDER BY ad.Id;

            SELECT v.Id, v.AvanceId, u.Nombres + ' ' + u.Apellidos AS Validador, v.RolValidador, v.Resultado, v.Observaciones, v.Fecha
            FROM AvanceValidaciones v JOIN Avances a ON a.Id = v.AvanceId JOIN Usuarios u ON u.Id = v.ValidadorId
            WHERE a.ProyectoId = @proyectoId AND a.Activo = 1 AND v.Activo = 1 ORDER BY v.Fecha, v.Id;
            """;
        using var conexion = conexiones.Crear();
        using var r = await conexion.QueryMultipleAsync(new CommandDefinition(sql, new { proyectoId }, cancellationToken: ct));
        var avances = (await r.ReadAsync<AvanceDetalle>()).ToList();
        var adjuntos = (await r.ReadAsync<(int Id, int AvanceId, string UrlArchivo, string? TipoMime)>()).ToList();
        var validaciones = (await r.ReadAsync<FilaValidacion>()).ToList();
        foreach (var a in avances)
        {
            a.Adjuntos = adjuntos.Where(x => x.AvanceId == a.Id)
                .Select((x, i) => new AdjuntoItem(x.Id, $"Adjunto {i + 1}{Path.GetExtension(x.UrlArchivo)}", x.TipoMime)).ToList();
            a.Validaciones = validaciones.Where(v => v.AvanceId == a.Id)
                .Select(v => new ValidacionItem(v.Id, v.Validador, v.RolValidador, v.Resultado, v.Observaciones, v.Fecha)).ToList();
        }
        return avances;
    }

    private sealed class FilaValidacion
    {
        public int Id { get; init; }
        public int AvanceId { get; init; }
        public string Validador { get; init; } = "";
        public string RolValidador { get; init; } = "";
        public string? Resultado { get; init; }
        public string? Observaciones { get; init; }
        public DateTime Fecha { get; init; }
    }

    public async Task<AvanceGuardado> CrearAsync(int proyectoId, AvanceRequest r, UsuarioAutenticado actor, CancellationToken ct = default)
    {
        // CA-3: el IdCliente evita duplicados por doble envío.
        if (r.IdCliente is { } idCliente
            && await db.Avances.Where(a => a.IdCliente == idCliente).Select(a => (int?)a.Id).FirstOrDefaultAsync(ct) is { } existente)
            return new AvanceGuardado(existente, true);

        var proyecto = await db.Proyectos.FirstOrDefaultAsync(p => p.Id == proyectoId && p.Activo, ct)
            ?? throw new NoEncontradoException("El proyecto no existe.");
        if (!await flujo.PuedeRegistrarAvancesAsync(proyectoId, actor, ct))
            throw new AccesoDenegadoException();
        if (proyecto.EstadoId != ValoresProyecto.EnEjecucion)
            throw new ReglaNegocioException("Solo se registran avances en proyectos \"En ejecución\".");
        await ValidarAsync(proyectoId, r, ct);

        var avance = new Avance
        {
            ProyectoId = proyectoId,
            AutorId = actor.Id,
            Titulo = r.Titulo.Trim(),
            Descripcion = string.IsNullOrWhiteSpace(r.Descripcion) ? null : r.Descripcion.Trim(),
            PorcentajeAvance = r.PorcentajeAvance,
            HitoId = r.HitoId,
            OrigenRegistro = "Web", // Los campos de GPS quedan nulos para la futura app móvil.
            FechaCaptura = Ahora,
            IdCliente = r.IdCliente,
            EstadoValidacion = EstadosAvance.Pendiente,
        };
        db.Avances.Add(avance);

        // CA-1: se notifica al IP y al Coordinador.
        var coordinador = await notificador.CoordinadorVigenteAsync(proyecto.ProgramaId, ct);
        foreach (var u in new[] { (int?)proyecto.InvestigadorPrincipalId, coordinador }.OfType<int>().Distinct().Where(u => u != actor.Id))
            notificador.Notificar(u, "AvanceRegistrado", $"Nuevo avance en {proyecto.Codigo}",
                $"Se registró el avance \"{avance.Titulo}\" ({r.PorcentajeAvance:0.##} %) del proyecto \"{proyecto.Titulo}\". Está pendiente de validación.",
                "Proyecto", proyecto.Id);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException) when (r.IdCliente is not null)
        {
            // Dos peticiones simultáneas con el mismo IdCliente: se devuelve el avance ya creado.
            var otro = await db.Avances.AsNoTracking().Where(a => a.IdCliente == r.IdCliente).Select(a => (int?)a.Id).FirstOrDefaultAsync(ct);
            if (otro is null)
                throw;
            return new AvanceGuardado(otro.Value, true);
        }
        return new AvanceGuardado(avance.Id, false);
    }

    /// <summary>El autor edita mientras el avance esté "Pendiente"; si estaba "Rechazado" vuelve a "Pendiente" (HU-20 CA-4).</summary>
    public async Task ActualizarAsync(int avanceId, AvanceRequest r, UsuarioAutenticado actor, CancellationToken ct = default)
    {
        var avance = await CargarDelAutorAsync(avanceId, actor, ct);
        await ValidarAsync(avance.ProyectoId, r, ct);
        avance.Titulo = r.Titulo.Trim();
        avance.Descripcion = string.IsNullOrWhiteSpace(r.Descripcion) ? null : r.Descripcion.Trim();
        avance.PorcentajeAvance = r.PorcentajeAvance;
        avance.HitoId = r.HitoId;
        avance.EstadoValidacion = EstadosAvance.Pendiente;
        await db.SaveChangesAsync(ct);
    }

    public async Task<int> AdjuntarAsync(int avanceId, string nombreArchivo, long tamano, Stream contenido, UsuarioAutenticado actor, CancellationToken ct = default)
    {
        var avance = await CargarDelAutorAsync(avanceId, actor, ct);
        var extension = Path.GetExtension(nombreArchivo);
        if (!Formatos.TryGetValue(extension, out var mime))
            throw new ReglaNegocioException("Formato no permitido. Use PDF, DOCX, XLSX, CSV, JPG o PNG.");
        if (tamano <= 0)
            throw new ReglaNegocioException("El archivo está vacío.");
        if (tamano > TamanoMaximoAdjunto)
            throw new ReglaNegocioException("El archivo supera el tamaño máximo de 20 MB.");
        if (await db.AvancesAdjuntos.CountAsync(a => a.AvanceId == avanceId && a.Activo, ct) >= MaxAdjuntos)
            throw new ReglaNegocioException("Un avance admite hasta 5 adjuntos.");

        using var memoria = new MemoryStream();
        await contenido.CopyToAsync(memoria, ct);
        if (memoria.Length > TamanoMaximoAdjunto)
            throw new ReglaNegocioException("El archivo supera el tamaño máximo de 20 MB.");
        memoria.Position = 0;
        var clave = await almacen.GuardarAsync(memoria, extension.ToLowerInvariant(), ct);

        var adjunto = new AvanceAdjunto { AvanceId = avance.Id, UrlArchivo = clave, TipoMime = mime };
        db.AvancesAdjuntos.Add(adjunto);
        await db.SaveChangesAsync(ct);
        return adjunto.Id;
    }

    public async Task<EnlaceDescargaAdjunto> GenerarDescargaAdjuntoAsync(int adjuntoId, UsuarioAutenticado actor, CancellationToken ct = default)
    {
        var proyectoId = await db.AvancesAdjuntos.Where(a => a.Id == adjuntoId && a.Activo && a.Avance!.Activo)
            .Select(a => (int?)a.Avance!.ProyectoId).FirstOrDefaultAsync(ct)
            ?? throw new NoEncontradoException("El adjunto no existe.");
        if (!await acceso.PuedeVerProyectoAsync(actor, proyectoId, ct))
            throw new AccesoDenegadoException();
        var vigencia = Documentos.DocumentosService.VigenciaDescarga;
        var token = enlaces.Generar(adjuntoId, vigencia, RecursosDescarga.AdjuntoAvance);
        return new EnlaceDescargaAdjunto($"/api/v1/avances/adjuntos/archivo?token={Uri.EscapeDataString(token)}", Ahora.Add(vigencia));
    }

    public async Task<ArchivoAdjunto> AbrirAdjuntoAsync(string token, CancellationToken ct = default)
    {
        var id = enlaces.Validar(token, RecursosDescarga.AdjuntoAvance)
            ?? throw new NoEncontradoException("El enlace de descarga no es válido o expiró.");
        var adjunto = await db.AvancesAdjuntos.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id && a.Activo, ct)
            ?? throw new NoEncontradoException("El adjunto no existe.");
        var extension = Path.GetExtension(adjunto.UrlArchivo);
        return new ArchivoAdjunto(almacen.Abrir(adjunto.UrlArchivo), $"adjunto-avance-{adjunto.AvanceId}-{adjunto.Id}{extension}",
            adjunto.TipoMime ?? "application/octet-stream");
    }

    // ----- HU-20 -----

    /// <summary>
    /// Bandeja "Avances por validar". El IP ve los avances pendientes de sus proyectos; el Coordinador
    /// (y el Director) los de su programa que aún no tienen la validación de ese nivel.
    /// </summary>
    public async Task<BandejaAvances> PendientesAsync(UsuarioAutenticado actor, int? proyectoId, string? periodo, CancellationToken ct = default)
    {
        if (!actor.Roles.Any(r => Politicas.Matriz[Politicas.ValidarAvances].Contains(r)))
            throw new AccesoDenegadoException();

        var hoy = DateOnly.FromDateTime(Ahora).ToDateTime(TimeOnly.MinValue);
        // Última validación del avance: si fue del IP, el avance sigue pendiente para el Coordinador.
        const string ultimaRol = "(SELECT TOP 1 v.RolValidador FROM AvanceValidaciones v WHERE v.AvanceId = a.Id AND v.Activo = 1 ORDER BY v.Id DESC)";
        var condiciones = new List<string>();
        if (actor.Tiene(Roles.Director))
            condiciones.Add($"(a.EstadoValidacion = N'Pendiente' OR (a.EstadoValidacion = N'Validado' AND {ultimaRol} = N'IP'))");
        if (actor.Tiene(Roles.Coordinador))
            condiciones.Add($"""
                ((a.EstadoValidacion = N'Pendiente' OR (a.EstadoValidacion = N'Validado' AND {ultimaRol} = N'IP'))
                 AND EXISTS (SELECT 1 FROM ProgramaIntegrantes g WHERE g.ProgramaId = p.ProgramaId AND g.UsuarioId = @usuarioId
                             AND g.Activo = 1 AND g.RolEnPrograma = N'Coordinador'
                             AND (g.FechaInicio IS NULL OR g.FechaInicio <= @hoy) AND (g.FechaFin IS NULL OR g.FechaFin > @hoy)))
                """);
        if (actor.Tiene(Roles.Investigador))
            condiciones.Add("(a.EstadoValidacion = N'Pendiente' AND p.InvestigadorPrincipalId = @usuarioId)");
        if (condiciones.Count == 0)
            return new BandejaAvances(0, []);

        var rol = actor.Tiene(Roles.Director) ? EstadosAvance.RolDirector
            : actor.Tiene(Roles.Coordinador) ? EstadosAvance.RolCoordinador : EstadosAvance.RolIp;
        var sql = $"""
            SELECT a.Id, a.ProyectoId, p.Codigo, p.Titulo AS Proyecto, pr.Nombre AS Programa, a.Titulo,
                   u.Nombres + ' ' + u.Apellidos AS Autor, a.PorcentajeAvance, a.FechaCaptura, a.EstadoValidacion,
                   @rol AS RolValidador
            FROM Avances a
            JOIN Proyectos p ON p.Id = a.ProyectoId AND p.Activo = 1
            JOIN Programas pr ON pr.Id = p.ProgramaId
            JOIN Usuarios u ON u.Id = a.AutorId
            WHERE a.Activo = 1 AND ({string.Join(" OR ", condiciones)})
              AND (@proyectoId IS NULL OR a.ProyectoId = @proyectoId)
              AND (@periodo IS NULL OR FORMAT(a.FechaCaptura, 'yyyy-MM') = @periodo)
            ORDER BY a.FechaCaptura
            """;
        using var conexion = conexiones.Crear();
        var items = (await conexion.QueryAsync<AvancePendiente>(new CommandDefinition(sql,
            new { usuarioId = actor.Id, hoy, proyectoId, periodo = string.IsNullOrWhiteSpace(periodo) ? null : periodo, rol },
            cancellationToken: ct))).ToList();
        return new BandejaAvances(items.Count, items);
    }

    public async Task ValidarAsync(int avanceId, ValidacionRequest r, UsuarioAutenticado actor, CancellationToken ct = default)
    {
        var avance = await db.Avances.Include(a => a.Proyecto)
            .FirstOrDefaultAsync(a => a.Id == avanceId && a.Activo && a.Proyecto!.Activo, ct)
            ?? throw new NoEncontradoException("El avance no existe.");
        var proyecto = avance.Proyecto!;

        var rol = await RolValidadorAsync(proyecto, actor, ct) ?? throw new AccesoDenegadoException();
        if (avance.EstadoValidacion == EstadosAvance.Rechazado)
            throw new ReglaNegocioException("El avance fue rechazado; el autor debe corregirlo antes de una nueva validación.");
        if (avance.EstadoValidacion == EstadosAvance.Validado && rol == EstadosAvance.RolIp)
            throw new ReglaNegocioException("El avance ya fue validado.");
        if (avance.EstadoValidacion == EstadosAvance.Validado && rol != EstadosAvance.RolIp
            && await UltimoRolAsync(avanceId, ct) != EstadosAvance.RolIp)
            throw new ReglaNegocioException("El avance ya fue validado.");

        db.AvanceValidaciones.Add(new AvanceValidacion
        {
            AvanceId = avance.Id,
            ValidadorId = actor.Id,
            RolValidador = rol,
            Periodo = (avance.FechaCaptura ?? Ahora).ToString("yyyy-MM"),
            Resultado = r.Resultado,
            Observaciones = string.IsNullOrWhiteSpace(r.Observaciones) ? null : r.Observaciones.Trim(),
            Fecha = Ahora,
        });
        avance.EstadoValidacion = r.Resultado;

        // CA-2: cuando el Coordinador valida, el porcentaje del proyecto toma el del avance.
        if (r.Resultado == EstadosAvance.Validado && rol != EstadosAvance.RolIp)
            proyecto.PorcentajeAvance = avance.PorcentajeAvance;

        if (avance.AutorId != actor.Id)
            notificador.Notificar(avance.AutorId, r.Resultado == EstadosAvance.Validado ? "AvanceValidado" : "AvanceRechazado",
                $"Avance {(r.Resultado == EstadosAvance.Validado ? "validado" : "rechazado")} en {proyecto.Codigo}",
                r.Resultado == EstadosAvance.Validado
                    ? $"Su avance \"{avance.Titulo}\" fue validado por el {rol}."
                    : $"Su avance \"{avance.Titulo}\" fue rechazado por el {rol}. Observaciones: {r.Observaciones?.Trim()}",
                "Proyecto", proyecto.Id);
        await db.SaveChangesAsync(ct);
    }

    // ----- Apoyo -----

    /// <summary>Anexo A, "Validar avances": Director; Coordinador su programa; Investigador si es IP.</summary>
    private async Task<string?> RolValidadorAsync(Proyecto proyecto, UsuarioAutenticado actor, CancellationToken ct)
    {
        if (actor.Tiene(Roles.Coordinador) && await acceso.EsCoordinadorVigenteAsync(actor.Id, proyecto.ProgramaId, ct))
            return EstadosAvance.RolCoordinador;
        if (actor.Tiene(Roles.Director))
            return EstadosAvance.RolDirector;
        if (actor.Tiene(Roles.Investigador) && proyecto.InvestigadorPrincipalId == actor.Id)
            return EstadosAvance.RolIp;
        return null;
    }

    private Task<string?> UltimoRolAsync(int avanceId, CancellationToken ct) =>
        db.AvanceValidaciones.Where(v => v.AvanceId == avanceId && v.Activo)
            .OrderByDescending(v => v.Id).Select(v => v.RolValidador).FirstOrDefaultAsync(ct);

    private async Task ValidarAsync(int proyectoId, AvanceRequest r, CancellationToken ct)
    {
        // CA-2: el porcentaje acumulado no puede ser menor que el del último avance validado.
        var ultimoValidado = await db.Avances
            .Where(a => a.ProyectoId == proyectoId && a.Activo && a.EstadoValidacion == EstadosAvance.Validado)
            .OrderByDescending(a => a.FechaCaptura).ThenByDescending(a => a.Id)
            .Select(a => a.PorcentajeAvance).FirstOrDefaultAsync(ct);
        if (ultimoValidado is { } minimo && r.PorcentajeAvance < minimo)
            throw new ReglaNegocioException($"El porcentaje acumulado no puede ser menor que el del último avance validado ({minimo:0.##} %).");
        if (r.HitoId is { } hitoId && !await db.Hitos.AnyAsync(h => h.Id == hitoId && h.Activo && h.Cronograma!.ProyectoId == proyectoId, ct))
            throw new ReglaNegocioException("El hito no pertenece al proyecto.");
    }

    private async Task<Avance> CargarDelAutorAsync(int avanceId, UsuarioAutenticado actor, CancellationToken ct)
    {
        var avance = await db.Avances.Include(a => a.Proyecto).FirstOrDefaultAsync(a => a.Id == avanceId && a.Activo, ct)
            ?? throw new NoEncontradoException("El avance no existe.");
        if (avance.AutorId != actor.Id)
            throw new AccesoDenegadoException("Solo el autor puede modificar el avance.");
        if (avance.EstadoValidacion is not (EstadosAvance.Pendiente or EstadosAvance.Rechazado))
            throw new ReglaNegocioException("El avance ya fue validado y no admite cambios.");
        if (avance.Proyecto!.EstadoId != ValoresProyecto.EnEjecucion)
            throw new ReglaNegocioException("Solo se registran avances en proyectos \"En ejecución\".");
        return avance;
    }
}

public sealed record EnlaceDescargaAdjunto(string Url, DateTime ExpiraEn);
