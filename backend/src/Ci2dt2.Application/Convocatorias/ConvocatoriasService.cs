using Ci2dt2.Application.Common;
using Ci2dt2.Application.Proyectos;
using Ci2dt2.Application.Seguridad;
using Ci2dt2.Domain;
using Ci2dt2.Domain.Entities;
using Dapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Ci2dt2.Application.Convocatorias;

public static class EstadosConvocatoria
{
    public const string Proxima = "Próxima";
    public const string Abierta = "Abierta";
    public const string Cerrada = "Cerrada";

    /// <summary>HU-15 CA-1: estado calculado por fechas.</summary>
    public static string Calcular(DateOnly? apertura, DateOnly? cierre, DateOnly hoy) =>
        apertura is not null && hoy < apertura ? Proxima
        : cierre is not null && hoy > cierre ? Cerrada
        : Abierta;
}

public sealed class ConvocatoriaItem
{
    public int Id { get; init; }
    public string Nombre { get; init; } = "";
    public string? Numero { get; init; }
    public int TipoConvocatoriaId { get; init; }
    public string TipoConvocatoria { get; init; } = "";
    public int? EntidadConvocanteId { get; init; }
    public string? EntidadConvocante { get; init; }
    public string Plataforma { get; init; } = "";
    public string? Url { get; init; }
    public DateTime? FechaApertura { get; init; }
    public DateTime? FechaCierre { get; init; }
    public string? Moneda { get; init; }
    public decimal? MontoMaximo { get; init; }
    public string Estado { get; set; } = "";
}

public sealed record FiltroConvocatorias(int? TipoConvocatoriaId, int? EntidadConvocanteId, string? Estado, string? Busqueda, int Pagina = 1);

public sealed record ConvocatoriaRequest(
    string Nombre, string? Numero, int TipoConvocatoriaId, int? EntidadConvocanteId, string Plataforma, string? Url,
    DateOnly? FechaApertura, DateOnly? FechaCierre, string? Moneda, decimal? MontoMaximo);

public sealed class ConvocatoriaRequestValidator : AbstractValidator<ConvocatoriaRequest>
{
    public ConvocatoriaRequestValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().WithMessage("El nombre es obligatorio.").MaximumLength(250);
        RuleFor(x => x.Numero).MaximumLength(30);
        RuleFor(x => x.TipoConvocatoriaId).GreaterThan(0).WithMessage("Debe seleccionar el tipo de convocatoria.");
        RuleFor(x => x.Plataforma).NotEmpty().WithMessage("La plataforma de radicación es obligatoria.").MaximumLength(50);
        RuleFor(x => x.Url).MaximumLength(400)
            .Must(u => Uri.TryCreate(u, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https")
            .WithMessage("La URL de los términos de referencia no es válida.")
            .When(x => !string.IsNullOrWhiteSpace(x.Url));
        RuleFor(x => x.FechaCierre).GreaterThanOrEqualTo(x => x.FechaApertura)
            .WithMessage("La fecha de cierre no puede ser anterior a la fecha de apertura.")
            .When(x => x.FechaApertura is not null && x.FechaCierre is not null);
        RuleFor(x => x.Moneda!).Must(m => ValoresProyecto.Monedas.Contains(m))
            .WithMessage("La moneda debe ser COP, USD o EUR.").When(x => x.Moneda is not null);
        RuleFor(x => x.Moneda).NotEmpty().WithMessage("Indique la moneda del monto máximo.").When(x => x.MontoMaximo is not null);
        RuleFor(x => x.MontoMaximo).GreaterThan(0).WithMessage("El monto máximo debe ser mayor que 0.");
    }
}

/// <summary>HU-15: catálogo de convocatorias.</summary>
public sealed class ConvocatoriasService(IApplicationDbContext db, IDbConnectionFactory conexiones, TimeProvider reloj)
{
    public const int TamanoPagina = 20;
    private DateOnly Hoy => DateOnly.FromDateTime(reloj.GetUtcNow().UtcDateTime);

    private const string Seleccion = """
        SELECT c.Id, c.Nombre, c.Numero, c.TipoConvocatoriaId, t.Nombre AS TipoConvocatoria,
               c.EntidadConvocanteId, a.Nombre AS EntidadConvocante, c.Plataforma, c.Url,
               CAST(c.FechaApertura AS datetime2) AS FechaApertura, CAST(c.FechaCierre AS datetime2) AS FechaCierre,
               c.Moneda, c.MontoMaximo
        FROM Convocatorias c
        JOIN TiposConvocatoria t ON t.Id = c.TipoConvocatoriaId
        LEFT JOIN Aliados a ON a.Id = c.EntidadConvocanteId
        """;

    // El estado se calcula por fechas también en SQL para poder filtrar y paginar en el servidor.
    private const string EstadoSql = """
        CASE WHEN c.FechaApertura IS NOT NULL AND @hoy < c.FechaApertura THEN N'Próxima'
             WHEN c.FechaCierre IS NOT NULL AND @hoy > c.FechaCierre THEN N'Cerrada'
             ELSE N'Abierta' END
        """;

    public async Task<Pagina<ConvocatoriaItem>> ListarAsync(FiltroConvocatorias f, CancellationToken ct = default)
    {
        var pagina = Math.Max(1, f.Pagina);
        var busqueda = string.IsNullOrWhiteSpace(f.Busqueda) ? null : $"%{f.Busqueda.Trim().Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]")}%";
        var filtro = $"""
            WHERE c.Activo = 1
              AND (@tipo IS NULL OR c.TipoConvocatoriaId = @tipo)
              AND (@entidad IS NULL OR c.EntidadConvocanteId = @entidad)
              AND (@estado IS NULL OR {EstadoSql} = @estado)
              AND (@busqueda IS NULL OR c.Nombre LIKE @busqueda OR c.Numero LIKE @busqueda)
            """;
        var sql = $"""
            SELECT COUNT(*) FROM Convocatorias c {filtro};
            {Seleccion} {filtro}
            ORDER BY CASE {EstadoSql} WHEN N'Abierta' THEN 0 WHEN N'Próxima' THEN 1 ELSE 2 END, c.FechaCierre, c.Id DESC
            OFFSET @salto ROWS FETCH NEXT @tamano ROWS ONLY;
            """;
        var parametros = new
        {
            tipo = f.TipoConvocatoriaId,
            entidad = f.EntidadConvocanteId,
            estado = string.IsNullOrWhiteSpace(f.Estado) ? null : f.Estado,
            busqueda,
            hoy = Hoy.ToDateTime(TimeOnly.MinValue),
            salto = (pagina - 1) * TamanoPagina,
            tamano = TamanoPagina,
        };
        using var conexion = conexiones.Crear();
        using var r = await conexion.QueryMultipleAsync(new CommandDefinition(sql, parametros, cancellationToken: ct));
        var total = await r.ReadSingleAsync<int>();
        var items = (await r.ReadAsync<ConvocatoriaItem>()).ToList();
        ConEstado(items);
        return new Pagina<ConvocatoriaItem>(items, total, pagina, TamanoPagina);
    }

    public async Task<ConvocatoriaItem?> ObtenerAsync(int id, CancellationToken ct = default)
    {
        using var conexion = conexiones.Crear();
        var item = await conexion.QuerySingleOrDefaultAsync<ConvocatoriaItem>(
            new CommandDefinition($"{Seleccion} WHERE c.Id = @id AND c.Activo = 1", new { id }, cancellationToken: ct));
        if (item is not null)
            ConEstado([item]);
        return item;
    }

    /// <summary>Convocatorias abiertas (FechaCierre ≥ hoy) para el portal público (HU-25).</summary>
    public async Task<IReadOnlyList<ConvocatoriaItem>> AbiertasAsync(CancellationToken ct = default)
    {
        using var conexion = conexiones.Crear();
        var items = (await conexion.QueryAsync<ConvocatoriaItem>(new CommandDefinition($"""
            {Seleccion}
            WHERE c.Activo = 1 AND (c.FechaCierre IS NULL OR c.FechaCierre >= @hoy) AND (c.FechaApertura IS NULL OR c.FechaApertura <= @hoy)
            ORDER BY c.FechaCierre
            """, new { hoy = Hoy.ToDateTime(TimeOnly.MinValue) }, cancellationToken: ct))).ToList();
        ConEstado(items);
        return items;
    }

    public async Task<int> CrearAsync(ConvocatoriaRequest r, CancellationToken ct = default)
    {
        await ValidarReferenciasAsync(r, ct);
        var c = new Convocatoria();
        Aplicar(c, r);
        db.Convocatorias.Add(c);
        await db.SaveChangesAsync(ct);
        return c.Id;
    }

    public async Task ActualizarAsync(int id, ConvocatoriaRequest r, CancellationToken ct = default)
    {
        var c = await CargarAsync(id, ct);
        await ValidarReferenciasAsync(r, ct);
        Aplicar(c, r);
        await db.SaveChangesAsync(ct);
    }

    public async Task EliminarAsync(int id, CancellationToken ct = default)
    {
        var c = await CargarAsync(id, ct);
        if (await db.PropuestasConvocatoria.AnyAsync(p => p.ConvocatoriaId == id && p.Activo, ct))
            throw new ReglaNegocioException("La convocatoria tiene postulaciones registradas y no se puede eliminar.");
        c.Activo = false;
        await db.SaveChangesAsync(ct);
    }

    private void ConEstado(IEnumerable<ConvocatoriaItem> items)
    {
        foreach (var i in items)
            i.Estado = EstadosConvocatoria.Calcular(
                i.FechaApertura is { } a ? DateOnly.FromDateTime(a) : null,
                i.FechaCierre is { } c ? DateOnly.FromDateTime(c) : null, Hoy);
    }

    private async Task<Convocatoria> CargarAsync(int id, CancellationToken ct) =>
        await db.Convocatorias.FirstOrDefaultAsync(c => c.Id == id && c.Activo, ct)
        ?? throw new NoEncontradoException("La convocatoria no existe.");

    private async Task ValidarReferenciasAsync(ConvocatoriaRequest r, CancellationToken ct)
    {
        if (!await db.TiposConvocatoria.AnyAsync(t => t.Id == r.TipoConvocatoriaId && t.Activo, ct))
            throw new ReglaNegocioException("El tipo de convocatoria no existe.");
        // La entidad convocante se selecciona del catálogo de aliados.
        if (r.EntidadConvocanteId is { } e && !await db.Aliados.AnyAsync(a => a.Id == e && a.Activo, ct))
            throw new ReglaNegocioException("La entidad convocante no existe en el catálogo de aliados.");
    }

    private static void Aplicar(Convocatoria c, ConvocatoriaRequest r)
    {
        c.Nombre = r.Nombre.Trim();
        c.Numero = string.IsNullOrWhiteSpace(r.Numero) ? null : r.Numero.Trim();
        c.TipoConvocatoriaId = r.TipoConvocatoriaId;
        c.EntidadConvocanteId = r.EntidadConvocanteId;
        c.Plataforma = r.Plataforma.Trim();
        c.Url = string.IsNullOrWhiteSpace(r.Url) ? null : r.Url.Trim();
        c.FechaApertura = r.FechaApertura;
        c.FechaCierre = r.FechaCierre;
        c.Moneda = r.Moneda;
        c.MontoMaximo = r.MontoMaximo;
    }
}

// ----- HU-16 -----

public static class ValoresPostulacion
{
    public static readonly string[] Resultados = ["Pendiente", "Seleccionada", "No seleccionada"];
    public static readonly string[] FasesSgr = ["Banco de elegibles", "Viabilización OCAD", "Designación de ejecutor"];
    public const string Seleccionada = "Seleccionada";
}

public sealed record PostulacionRequest(
    int ConvocatoriaId, DateOnly? FechaPresentacion, string? IdPostulacionExterna, string? CodigoMga,
    decimal? MontoSolicitado, string? Moneda, decimal? TasaCambioEstimada, string? Resultado, decimal? Puntaje,
    string? FaseExterna, DateOnly? FechaResultado);

public sealed class PostulacionDetalle
{
    public int Id { get; init; }
    public int ConvocatoriaId { get; init; }
    public string Convocatoria { get; init; } = "";
    public int TipoConvocatoriaId { get; init; }
    public string TipoConvocatoria { get; init; } = "";
    public DateTime? FechaPresentacion { get; init; }
    public string? IdPostulacionExterna { get; init; }
    public string? CodigoMga { get; init; }
    public decimal? MontoSolicitado { get; init; }
    public string? Moneda { get; init; }
    public decimal? TasaCambioEstimada { get; init; }
    public decimal? MontoSolicitadoCop { get; init; }
    public string? Resultado { get; init; }
    public decimal? Puntaje { get; init; }
    public string? FaseExterna { get; init; }
    public DateTime? FechaResultado { get; init; }
    public int? ComprobanteDocId { get; init; }
}

public sealed class PostulacionRequestValidator : AbstractValidator<PostulacionRequest>
{
    public PostulacionRequestValidator()
    {
        RuleFor(x => x.ConvocatoriaId).GreaterThan(0).WithMessage("Debe seleccionar la convocatoria.");
        RuleFor(x => x.IdPostulacionExterna).MaximumLength(60);
        RuleFor(x => x.CodigoMga).MaximumLength(30);
        RuleFor(x => x.MontoSolicitado).GreaterThan(0).WithMessage("El monto solicitado debe ser mayor que 0.");
        RuleFor(x => x.Moneda!).Must(m => ValoresProyecto.Monedas.Contains(m))
            .WithMessage("La moneda debe ser COP, USD o EUR.").When(x => x.Moneda is not null);
        RuleFor(x => x.Moneda).NotEmpty().WithMessage("Indique la moneda del monto solicitado.").When(x => x.MontoSolicitado is not null);
        // P1.3: con moneda distinta de COP la tasa estimada es obligatoria.
        RuleFor(x => x.TasaCambioEstimada).NotNull().GreaterThan(0)
            .WithMessage("La tasa de cambio estimada es obligatoria y mayor que 0 cuando la moneda no es COP.")
            .When(x => x.Moneda is not null && x.Moneda != "COP");
        RuleFor(x => x.Resultado!).Must(r => ValoresPostulacion.Resultados.Contains(r))
            .WithMessage("El resultado debe ser Pendiente, Seleccionada o No seleccionada.").When(x => x.Resultado is not null);
        RuleFor(x => x.FaseExterna!).Must(f => ValoresPostulacion.FasesSgr.Contains(f))
            .WithMessage("La fase externa debe ser Banco de elegibles, Viabilización OCAD o Designación de ejecutor.")
            .When(x => x.FaseExterna is not null);
        RuleFor(x => x.Puntaje).InclusiveBetween(0, 9999.99m);
    }
}

/// <summary>HU-16: postulación de la propuesta a una convocatoria (radicación externa y resultado).</summary>
public sealed class PostulacionesService(
    IApplicationDbContext db, IDbConnectionFactory conexiones, AccesoRecursos acceso, Notificador notificador)
{
    public async Task<IReadOnlyList<PostulacionDetalle>> ListarAsync(int proyectoId, CancellationToken ct = default)
    {
        using var conexion = conexiones.Crear();
        return (await conexion.QueryAsync<PostulacionDetalle>(new CommandDefinition("""
            SELECT pc.Id, pc.ConvocatoriaId, c.Nombre AS Convocatoria, c.TipoConvocatoriaId, t.Nombre AS TipoConvocatoria,
                   CAST(pc.FechaPresentacion AS datetime2) AS FechaPresentacion, pc.IdPostulacionExterna, pc.CodigoMga,
                   pc.MontoSolicitado, pc.Moneda, pc.TasaCambioEstimada, pc.MontoSolicitadoCop, pc.Resultado, pc.Puntaje,
                   pc.FaseExterna, CAST(pc.FechaResultado AS datetime2) AS FechaResultado, pc.ComprobanteDocId
            FROM PropuestasConvocatoria pc
            JOIN Convocatorias c ON c.Id = pc.ConvocatoriaId
            JOIN TiposConvocatoria t ON t.Id = c.TipoConvocatoriaId
            WHERE pc.ProyectoId = @proyectoId AND pc.Activo = 1
            ORDER BY pc.Id DESC
            """, new { proyectoId }, cancellationToken: ct))).ToList();
    }

    public async Task<int> CrearAsync(int proyectoId, PostulacionRequest r, UsuarioAutenticado actor, CancellationToken ct = default)
    {
        var proyecto = await ValidarProyectoAsync(proyectoId, actor, ct);
        var p = new PropuestaConvocatoria { ProyectoId = proyectoId, Resultado = "Pendiente" };
        db.PropuestasConvocatoria.Add(p);
        await AplicarAsync(p, r, null, proyecto, ct);
        await db.SaveChangesAsync(ct);
        return p.Id;
    }

    public async Task ActualizarAsync(int proyectoId, int id, PostulacionRequest r, UsuarioAutenticado actor, CancellationToken ct = default)
    {
        var proyecto = await ValidarProyectoAsync(proyectoId, actor, ct);
        var p = await db.PropuestasConvocatoria.FirstOrDefaultAsync(x => x.Id == id && x.ProyectoId == proyectoId && x.Activo, ct)
            ?? throw new NoEncontradoException("La postulación no existe.");
        await AplicarAsync(p, r, p.Resultado, proyecto, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task EliminarAsync(int proyectoId, int id, UsuarioAutenticado actor, CancellationToken ct = default)
    {
        await ValidarProyectoAsync(proyectoId, actor, ct);
        var p = await db.PropuestasConvocatoria.FirstOrDefaultAsync(x => x.Id == id && x.ProyectoId == proyectoId && x.Activo, ct)
            ?? throw new NoEncontradoException("La postulación no existe.");
        p.Activo = false;
        await db.SaveChangesAsync(ct);
    }

    private async Task AplicarAsync(PropuestaConvocatoria p, PostulacionRequest r, string? resultadoAnterior, Proyecto proyecto, CancellationToken ct)
    {
        var convocatoria = await db.Convocatorias.AsNoTracking().FirstOrDefaultAsync(c => c.Id == r.ConvocatoriaId && c.Activo, ct)
            ?? throw new ReglaNegocioException("La convocatoria no existe.");

        // CA-2: el monto solicitado no puede superar el máximo de la convocatoria.
        if (r.MontoSolicitado is { } monto && convocatoria.MontoMaximo is { } maximo)
        {
            var comparable = r.Moneda == convocatoria.Moneda ? monto
                : convocatoria.Moneda == "COP" ? ValoresProyecto.EnCop(monto, r.Moneda, r.TasaCambioEstimada)
                : (decimal?)null;
            if (comparable > maximo)
                throw new ReglaNegocioException($"El monto solicitado supera el monto máximo de la convocatoria ({maximo:N0} {convocatoria.Moneda}).");
        }

        // CA-1 y P1.5: en convocatorias SGR el código MGA es obligatorio; la fase externa solo aplica a SGR.
        var esSgr = convocatoria.TipoConvocatoriaId == Catalogo.ConvocatoriaSgr;
        if (esSgr && string.IsNullOrWhiteSpace(r.CodigoMga))
            throw new ReglaNegocioException("Las convocatorias SGR exigen el código MGA.");
        if (!esSgr && r.FaseExterna is not null)
            throw new ReglaNegocioException("La fase externa solo aplica a convocatorias SGR.");

        p.ConvocatoriaId = r.ConvocatoriaId;
        p.FechaPresentacion = r.FechaPresentacion;
        p.IdPostulacionExterna = string.IsNullOrWhiteSpace(r.IdPostulacionExterna) ? null : r.IdPostulacionExterna.Trim();
        p.CodigoMga = string.IsNullOrWhiteSpace(r.CodigoMga) ? null : r.CodigoMga.Trim();
        p.MontoSolicitado = r.MontoSolicitado;
        p.Moneda = r.Moneda;
        p.TasaCambioEstimada = r.Moneda is null or "COP" ? null : r.TasaCambioEstimada;
        p.MontoSolicitadoCop = ValoresProyecto.EnCop(r.MontoSolicitado, r.Moneda, r.TasaCambioEstimada);
        p.Resultado = r.Resultado ?? p.Resultado ?? "Pendiente";
        p.Puntaje = r.Puntaje;
        p.FaseExterna = r.FaseExterna;
        p.FechaResultado = r.FechaResultado;

        // Convocatorias externas: para marcarla como radicada (con fecha de presentación) se exigen
        // el id de postulación externa y el comprobante de radicación adjunto a la postulación.
        if (convocatoria.TipoConvocatoriaId != Catalogo.ConvocatoriaInterna && p.FechaPresentacion is not null)
        {
            if (p.IdPostulacionExterna is null)
                throw new ReglaNegocioException("Para marcar la postulación como radicada indique el id de postulación externa.");
            var comprobante = p.Id == 0 ? null : await db.Documentos.Where(d => d.Activo && d.EntidadTipo == "PropuestaConvocatoria"
                    && d.EntidadId == p.Id && d.TipoDocumentoId == Catalogo.DocComprobanteRadicacion)
                .OrderByDescending(d => d.Id).Select(d => (int?)d.Id).FirstOrDefaultAsync(ct);
            p.ComprobanteDocId = comprobante
                ?? throw new ReglaNegocioException("Para marcar la postulación como radicada adjunte primero el comprobante de radicación a la postulación.");
        }

        // CA-3: con resultado "Seleccionada" se notifica al Director para registrar la aprobación.
        if (p.Resultado == ValoresPostulacion.Seleccionada && resultadoAnterior != ValoresPostulacion.Seleccionada)
        {
            var directores = await db.Usuarios.Where(u => u.Activo && u.TieneAcceso && u.Roles.Any(x => x.Rol!.Nombre == Roles.Director))
                .Select(u => u.Id).ToListAsync(ct);
            foreach (var d in directores)
                notificador.Notificar(d, "PostulacionSeleccionada", $"Postulación seleccionada: {proyecto.Codigo}",
                    $"La propuesta \"{proyecto.Titulo}\" fue seleccionada en la convocatoria \"{convocatoria.Nombre}\". Registre la aprobación del proyecto.",
                    "Proyecto", proyecto.Id);
        }
    }

    /// <summary>Postulaciones: el IP del proyecto, el Coordinador de su programa, el Director y el Administrador.</summary>
    private async Task<Proyecto> ValidarProyectoAsync(int proyectoId, UsuarioAutenticado actor, CancellationToken ct)
    {
        var proyecto = await db.Proyectos.Include(p => p.Estado).FirstOrDefaultAsync(p => p.Id == proyectoId && p.Activo, ct)
            ?? throw new NoEncontradoException("El proyecto no existe.");
        if (!actor.Roles.Any(r => Politicas.Matriz[Politicas.EditarPropuestas].Contains(r))
            || !await acceso.PuedeModificarProyectoComoIpAsync(actor, proyectoId, ct))
            throw new AccesoDenegadoException();
        if (proyecto.Estado!.EsFinal)
            throw new ReglaNegocioException($"El proyecto está en estado \"{proyecto.Estado.Nombre}\" y no admite cambios.");
        return proyecto;
    }
}
