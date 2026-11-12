using Ci2dt2.Application.Common;
using Ci2dt2.Application.Seguridad;
using Ci2dt2.Domain;
using Ci2dt2.Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Ci2dt2.Application.Proyectos;

/// <summary>
/// Máquina de estados del ciclo de vida (P1 y P2): solo se permiten las transiciones definidas;
/// cualquier otra responde 422 "Transición de estado no permitida" (HU-12 CA-5).
/// </summary>
public static class MaquinaEstadosProyecto
{
    private static readonly HashSet<(int, int)> Permitidas =
    [
        (ValoresProyecto.EnElaboracion, ValoresProyecto.EnRevision),
        (ValoresProyecto.Devuelto, ValoresProyecto.EnRevision),
        (ValoresProyecto.EnRevision, ValoresProyecto.Devuelto),
        (ValoresProyecto.EnRevision, ValoresProyecto.Presentado),
        (ValoresProyecto.Presentado, ValoresProyecto.Aprobado),
        (ValoresProyecto.Presentado, ValoresProyecto.NoAprobado),
        (ValoresProyecto.Aprobado, ValoresProyecto.EnEjecucion),
    ];

    public const string MensajeNoPermitida = "Transición de estado no permitida.";

    public static bool EsPermitida(int desde, int hacia) => Permitidas.Contains((desde, hacia));

    public static void Validar(int desde, int hacia)
    {
        if (!EsPermitida(desde, hacia))
            throw new ReglaNegocioException(MensajeNoPermitida);
    }
}

public static class DecisionesRevision
{
    public const string Devuelve = "Devuelve";
    public const string Aprueba = "Aprueba";
    public const string Avala = "Avala";
    public const string Aprobado = "Aprobado";
    public const string NoAprobado = "No aprobado";
    public const string IniciaEjecucion = "Inicia ejecución";

    public const string EtapaCoordinador = "Coordinador";
    public const string EtapaDirector = "Director";

    public const int MinimoObservaciones = 20;
}

public sealed record RevisionRequest(string Decision, string? Observaciones);

public sealed record AprobacionRequest(bool Aprobado, decimal? PresupuestoAprobado, string? Observaciones);

public sealed record IniciarEjecucionRequest(DateOnly? FechaInicio);

public sealed record RequisitoEnvio(string Descripcion, bool Cumple);

/// <summary>Acciones del flujo disponibles para el usuario en la ficha del proyecto.</summary>
public sealed record FlujoProyecto(
    string? EtapaRevision,
    IReadOnlyList<RequisitoEnvio> Requisitos,
    bool PuedeEnviarRevision,
    IReadOnlyList<string> DecisionesDisponibles,
    bool PuedeRegistrarAprobacion,
    bool PuedeIniciarEjecucion,
    bool PuedeGestionarHitos,
    bool PuedeRegistrarAvances,
    bool PuedeGestionarPostulaciones);

public sealed class RevisionRequestValidator : AbstractValidator<RevisionRequest>
{
    public RevisionRequestValidator()
    {
        RuleFor(x => x.Decision)
            .Must(d => d is DecisionesRevision.Devuelve or DecisionesRevision.Aprueba or DecisionesRevision.Avala)
            .WithMessage("La decisión debe ser Devuelve, Aprueba o Avala.");
        // HU-11 CA-5: devolver exige observaciones de al menos 20 caracteres.
        RuleFor(x => x.Observaciones)
            .Must(o => o is not null && o.Trim().Length >= DecisionesRevision.MinimoObservaciones)
            .WithMessage("Para devolver la propuesta escriba observaciones de al menos 20 caracteres.")
            .When(x => x.Decision == DecisionesRevision.Devuelve);
        RuleFor(x => x.Observaciones).MaximumLength(4000);
    }
}

public sealed class AprobacionRequestValidator : AbstractValidator<AprobacionRequest>
{
    public AprobacionRequestValidator()
    {
        RuleFor(x => x.PresupuestoAprobado).GreaterThanOrEqualTo(0).WithMessage("El presupuesto aprobado no puede ser negativo.");
        RuleFor(x => x.Observaciones).MaximumLength(4000);
    }
}

public sealed class IniciarEjecucionRequestValidator : AbstractValidator<IniciarEjecucionRequest>
{
    public IniciarEjecucionRequestValidator()
    {
        RuleFor(x => x.FechaInicio).NotNull().WithMessage("La fecha de inicio real es obligatoria.");
    }
}

/// <summary>
/// HU-11 (envío a revisión, revisión del Coordinador y aval del Director) y HU-12 (aprobación e
/// inicio de ejecución). Cada decisión queda en ProyectoRevisiones y se notifica a los involucrados.
/// </summary>
public sealed class FlujoProyectoService(IApplicationDbContext db, AccesoRecursos acceso, Notificador notificador, TimeProvider reloj)
{
    private DateTime Ahora => reloj.GetUtcNow().UtcDateTime;
    private DateOnly Hoy => DateOnly.FromDateTime(Ahora);

    // ----- HU-11 -----

    /// <summary>Lista de verificación previa al envío (HU-11 CA-6).</summary>
    public async Task<IReadOnlyList<RequisitoEnvio>> RequisitosAsync(int proyectoId, CancellationToken ct = default)
    {
        var p = await db.Proyectos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == proyectoId && x.Activo, ct)
            ?? throw new NoEncontradoException("El proyecto no existe.");
        var fuentes = await db.FuentesFinanciamiento.AsNoTracking()
            .Where(f => f.ProyectoId == proyectoId && f.Activo).ToListAsync(ct);
        var postulaciones = await db.PropuestasConvocatoria.AsNoTracking()
            .Where(pc => pc.ProyectoId == proyectoId && pc.Activo).Select(pc => new { pc.Id, pc.Convocatoria!.TipoConvocatoriaId })
            .ToListAsync(ct);
        var idsPostulaciones = postulaciones.Select(x => x.Id).ToList();
        var tiposDocumento = await db.Documentos.AsNoTracking()
            .Where(d => d.Activo && ((d.EntidadTipo == "Proyecto" && d.EntidadId == proyectoId)
                || (d.EntidadTipo == "PropuestaConvocatoria" && d.EntidadId != null && idsPostulaciones.Contains(d.EntidadId.Value))))
            .Select(d => d.TipoDocumentoId).Distinct().ToListAsync(ct);

        var requisitos = new List<RequisitoEnvio>
        {
            new("Título, resumen, tipo, fechas de inicio y fin, moneda y presupuesto diligenciados",
                !string.IsNullOrWhiteSpace(p.Titulo) && !string.IsNullOrWhiteSpace(p.Resumen) && p.FechaInicio is not null
                && p.FechaFin is not null && p.Moneda is not null && p.PresupuestoTotal is not null),
            new("Al menos una fuente de financiamiento registrada", fuentes.Count > 0),
        };

        // Cartas de compromiso cuando hay aportes de aliados.
        if (fuentes.Any(f => f.AliadoId is not null))
            requisitos.Add(new("Carta de compromiso adjunta (hay aportes de aliados)", tiposDocumento.Contains(Catalogo.DocCartaCompromiso)));

        // Convocatoria SGR: Documento Conceptual MGA adjunto.
        if (postulaciones.Any(x => x.TipoConvocatoriaId == Catalogo.ConvocatoriaSgr))
            requisitos.Add(new("Documento Conceptual MGA adjunto (convocatoria SGR)", tiposDocumento.Contains(Catalogo.DocConceptualMga)));

        // P1.2: convocatorias externas nacionales exigen el desglose de contrapartidas de la Universidad.
        if (postulaciones.Any(x => x.TipoConvocatoriaId == Catalogo.ConvocatoriaNacional))
            requisitos.Add(new("Contrapartidas de la Universidad de Caldas registradas (convocatoria nacional)",
                fuentes.Any(f => f.TipoFuente == "Contrapartida" && f.AliadoId is null)));

        return requisitos;
    }

    public async Task EnviarRevisionAsync(int proyectoId, UsuarioAutenticado actor, CancellationToken ct = default)
    {
        var proyecto = await CargarAsync(proyectoId, ct);
        if (!await PuedeEnviarAsync(proyecto, actor, ct))
            throw new AccesoDenegadoException();
        MaquinaEstadosProyecto.Validar(proyecto.EstadoId, ValoresProyecto.EnRevision);

        var pendientes = (await RequisitosAsync(proyectoId, ct)).Where(r => !r.Cumple).Select(r => r.Descripcion).ToList();
        if (pendientes.Count > 0)
            throw new ReglaNegocioException("La propuesta tiene requisitos pendientes: " + string.Join("; ", pendientes) + ".");

        var reenvio = proyecto.EstadoId == ValoresProyecto.Devuelto;
        proyecto.EstadoId = ValoresProyecto.EnRevision;

        // CA-1: notificación al Coordinador del programa; si no hay Coordinador vigente, a los Directores.
        var coordinador = await notificador.CoordinadorVigenteAsync(proyecto.ProgramaId, ct);
        var destinatarios = coordinador is { } c ? [c] : await DirectoresAsync(ct);
        foreach (var d in destinatarios)
            notificador.Notificar(d, "PropuestaEnRevision", $"Propuesta {proyecto.Codigo} enviada a revisión",
                $"La propuesta \"{proyecto.Titulo}\" {(reenvio ? "fue corregida y reenviada" : "fue enviada")} a revisión (etapa Coordinador).",
                "Proyecto", proyecto.Id);
        await db.SaveChangesAsync(ct);
    }

    public async Task RevisarAsync(int proyectoId, RevisionRequest r, UsuarioAutenticado actor, CancellationToken ct = default)
    {
        var proyecto = await CargarAsync(proyectoId, ct);
        if (proyecto.EstadoId != ValoresProyecto.EnRevision)
            throw new ReglaNegocioException("La propuesta no está en revisión.");

        var etapa = await EtapaActualAsync(proyectoId, ct) ?? DecisionesRevision.EtapaCoordinador;
        var disponibles = await DecisionesDisponiblesAsync(proyecto, etapa, actor, ct);
        if (disponibles.Count == 0)
            throw new AccesoDenegadoException();
        if (!disponibles.Contains(r.Decision))
            throw new ReglaNegocioException($"La decisión \"{r.Decision}\" no corresponde a la etapa {etapa}.");

        var estadoNuevo = r.Decision switch
        {
            DecisionesRevision.Devuelve => ValoresProyecto.Devuelto,
            DecisionesRevision.Avala => ValoresProyecto.Presentado,
            _ => ValoresProyecto.EnRevision, // Aprueba: pasa a la etapa Director sin cambiar de estado.
        };
        if (estadoNuevo != proyecto.EstadoId)
            MaquinaEstadosProyecto.Validar(proyecto.EstadoId, estadoNuevo);

        var observaciones = string.IsNullOrWhiteSpace(r.Observaciones) ? null : r.Observaciones.Trim();
        Registrar(proyecto, actor.Id, etapa, r.Decision, estadoNuevo, observaciones);
        proyecto.EstadoId = estadoNuevo;

        var coordinador = await notificador.CoordinadorVigenteAsync(proyecto.ProgramaId, ct);
        switch (r.Decision)
        {
            case DecisionesRevision.Devuelve:
                // CA-2: el IP recibe las observaciones.
                notificador.Notificar(proyecto.InvestigadorPrincipalId, "PropuestaDevuelta", $"Propuesta {proyecto.Codigo} devuelta",
                    $"La propuesta \"{proyecto.Titulo}\" fue devuelta en la etapa {etapa}. Observaciones: {observaciones}", "Proyecto", proyecto.Id);
                break;
            case DecisionesRevision.Aprueba:
                // CA-3: pasa a la etapa Director y se notifica al Director.
                foreach (var d in await DirectoresAsync(ct))
                    notificador.Notificar(d, "PropuestaAprobadaCoordinador", $"Propuesta {proyecto.Codigo} pendiente de aval",
                        $"El Coordinador aprobó la propuesta \"{proyecto.Titulo}\". Está pendiente de su aval.", "Proyecto", proyecto.Id);
                break;
            case DecisionesRevision.Avala:
                // CA-4: se notifica al IP y al Coordinador.
                foreach (var d in new[] { (int?)proyecto.InvestigadorPrincipalId, coordinador }.OfType<int>().Distinct())
                    notificador.Notificar(d, "PropuestaAvalada", $"Propuesta {proyecto.Codigo} avalada",
                        $"El Director otorgó el aval institucional a la propuesta \"{proyecto.Titulo}\". Su estado es \"Presentado\".", "Proyecto", proyecto.Id);
                break;
        }
        await db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// La etapa se registra en ProyectoRevisiones.Etapa: tras la aprobación del Coordinador la
    /// propuesta queda en etapa Director; en cualquier otro caso (envío o reenvío) en etapa Coordinador.
    /// </summary>
    public async Task<string?> EtapaActualAsync(int proyectoId, CancellationToken ct = default)
    {
        var estado = await db.Proyectos.Where(p => p.Id == proyectoId).Select(p => p.EstadoId).FirstOrDefaultAsync(ct);
        if (estado != ValoresProyecto.EnRevision)
            return null;
        var ultima = await db.ProyectoRevisiones.Where(r => r.ProyectoId == proyectoId && r.Activo)
            .OrderByDescending(r => r.Fecha).ThenByDescending(r => r.Id).Select(r => r.Decision).FirstOrDefaultAsync(ct);
        return ultima == DecisionesRevision.Aprueba ? DecisionesRevision.EtapaDirector : DecisionesRevision.EtapaCoordinador;
    }

    // ----- HU-12 -----

    public async Task RegistrarAprobacionAsync(int proyectoId, AprobacionRequest r, UsuarioAutenticado actor, CancellationToken ct = default)
    {
        EsDirectivoAval(actor);
        var proyecto = await CargarAsync(proyectoId, ct);
        var estadoNuevo = r.Aprobado ? ValoresProyecto.Aprobado : ValoresProyecto.NoAprobado;
        MaquinaEstadosProyecto.Validar(proyecto.EstadoId, estadoNuevo);

        if (r.Aprobado)
        {
            // CA-2: la aprobación exige el acto administrativo en PDF adjunto al proyecto.
            var tieneActo = await db.Documentos.AnyAsync(d => d.Activo && d.EntidadTipo == "Proyecto" && d.EntidadId == proyectoId
                && d.TipoDocumentoId == Catalogo.DocActoAdministrativo && d.NombreArchivo.ToLower().EndsWith(".pdf"), ct);
            if (!tieneActo)
                throw new ReglaNegocioException("Para aprobar el proyecto adjunte primero el acto administrativo de aprobación en PDF (pestaña Documentos).");

            // Si el presupuesto aprobado difiere del solicitado se actualiza (el cambio queda en el log).
            if (r.PresupuestoAprobado is { } aprobado && aprobado != proyecto.PresupuestoTotal)
            {
                proyecto.PresupuestoTotal = aprobado;
                proyecto.PresupuestoTotalCop = ValoresProyecto.EnCop(aprobado, proyecto.Moneda, proyecto.TasaCambio);
            }

            // Al pasar a "Aprobado" se crea el cronograma del proyecto.
            if (!await db.Cronogramas.AnyAsync(c => c.ProyectoId == proyectoId && c.Activo, ct))
                db.Cronogramas.Add(new Cronograma { ProyectoId = proyectoId, Nombre = $"Cronograma {proyecto.Codigo}" });
        }

        var observaciones = string.IsNullOrWhiteSpace(r.Observaciones) ? null : r.Observaciones.Trim();
        Registrar(proyecto, actor.Id, DecisionesRevision.EtapaDirector, r.Aprobado ? DecisionesRevision.Aprobado : DecisionesRevision.NoAprobado,
            estadoNuevo, observaciones);
        proyecto.EstadoId = estadoNuevo;

        foreach (var u in await EquipoVigenteAsync(proyectoId, ct))
        {
            var mensaje = !r.Aprobado
                ? $"La postulación del proyecto \"{proyecto.Titulo}\" quedó registrada como no aprobada."
                : u == proyecto.InvestigadorPrincipalId
                    ? $"El proyecto \"{proyecto.Titulo}\" fue aprobado. Revise la información registrada del proyecto aprobado y defina sus hitos."
                    : $"El proyecto \"{proyecto.Titulo}\" fue aprobado.";
            notificador.Notificar(u, r.Aprobado ? "ProyectoAprobado" : "ProyectoNoAprobado",
                $"Proyecto {proyecto.Codigo} {(r.Aprobado ? "aprobado" : "no aprobado")}", mensaje, "Proyecto", proyecto.Id);
        }
        await db.SaveChangesAsync(ct);
    }

    public async Task IniciarEjecucionAsync(int proyectoId, IniciarEjecucionRequest r, UsuarioAutenticado actor, CancellationToken ct = default)
    {
        EsDirectivoAval(actor);
        var proyecto = await CargarAsync(proyectoId, ct);
        MaquinaEstadosProyecto.Validar(proyecto.EstadoId, ValoresProyecto.EnEjecucion);

        var tienePlan = await db.Documentos.AnyAsync(d => d.Activo && d.EntidadTipo == "Proyecto" && d.EntidadId == proyectoId
            && d.TipoDocumentoId == Catalogo.DocPlanTrabajo, ct);
        if (!tienePlan)
            throw new ReglaNegocioException("Para iniciar la ejecución adjunte primero el plan de trabajo aprobado (pestaña Documentos).");

        // P11 / HU-17 CA-3: un Reto empresarial exige 3 hitos tipo Sprint y 1 Entrega de MVP.
        var tipo = await db.TiposProyecto.AsNoTracking().FirstAsync(t => t.Id == proyecto.TipoProyectoId, ct);
        if (tipo.RequiereHitosFijos)
        {
            var hitos = await db.Hitos.AsNoTracking()
                .Where(h => h.Activo && h.Cronograma!.ProyectoId == proyectoId && h.Cronograma.Activo)
                .Select(h => h.TipoHitoId).ToListAsync(ct);
            var faltantes = new List<string>();
            var sprints = hitos.Count(t => t == Catalogo.HitoSprint);
            if (sprints < 3)
                faltantes.Add($"{3 - sprints} hito(s) de tipo Sprint");
            if (!hitos.Contains(Catalogo.HitoMvp))
                faltantes.Add("1 hito de Entrega de MVP");
            if (faltantes.Count > 0)
                throw new ReglaNegocioException($"Un reto empresarial necesita al menos 3 hitos tipo Sprint y 1 de Entrega de MVP antes de iniciar la ejecución. Faltan: {string.Join(" y ", faltantes)}.");
        }

        proyecto.FechaInicio = r.FechaInicio;
        Registrar(proyecto, actor.Id, DecisionesRevision.EtapaDirector, DecisionesRevision.IniciaEjecucion, ValoresProyecto.EnEjecucion, null);
        proyecto.EstadoId = ValoresProyecto.EnEjecucion;
        foreach (var u in await EquipoVigenteAsync(proyectoId, ct))
            notificador.Notificar(u, "ProyectoEnEjecucion", $"Proyecto {proyecto.Codigo} en ejecución",
                $"El proyecto \"{proyecto.Titulo}\" inició su ejecución el {r.FechaInicio:dd/MM/yyyy}. Ya puede registrar avances.", "Proyecto", proyecto.Id);
        await db.SaveChangesAsync(ct);
    }

    // ----- Permisos de la ficha -----

    public async Task<FlujoProyecto> FlujoAsync(int proyectoId, UsuarioAutenticado actor, CancellationToken ct = default)
    {
        var proyecto = await db.Proyectos.AsNoTracking().Include(p => p.Estado)
            .FirstOrDefaultAsync(p => p.Id == proyectoId && p.Activo, ct)
            ?? throw new NoEncontradoException("El proyecto no existe.");
        var etapa = await EtapaActualAsync(proyectoId, ct);
        var esAvalador = actor.Tiene(Roles.Administrador) || actor.Tiene(Roles.Director);
        var requisitos = proyecto.EstadoId is ValoresProyecto.EnElaboracion or ValoresProyecto.Devuelto
            ? await RequisitosAsync(proyectoId, ct)
            : [];
        var gestor = await acceso.PuedeModificarProyectoComoIpAsync(actor, proyectoId, ct)
            && actor.Roles.Any(r => Politicas.Matriz[Politicas.GestionarHitos].Contains(r));
        var tieneCronograma = await db.Cronogramas.AnyAsync(c => c.ProyectoId == proyectoId && c.Activo, ct);

        return new FlujoProyecto(
            etapa,
            requisitos,
            requisitos.Count > 0 && await PuedeEnviarAsync(proyecto, actor, ct),
            etapa is { } e ? await DecisionesDisponiblesAsync(proyecto, e, actor, ct) : [],
            esAvalador && proyecto.EstadoId == ValoresProyecto.Presentado,
            esAvalador && proyecto.EstadoId == ValoresProyecto.Aprobado,
            gestor && tieneCronograma && !proyecto.Estado!.EsFinal,
            proyecto.EstadoId == ValoresProyecto.EnEjecucion && await PuedeRegistrarAvancesAsync(proyectoId, actor, ct),
            gestor && !proyecto.Estado!.EsFinal);
    }

    /// <summary>"Registrar avances": Admin y Director; Coordinador de su programa; integrantes vigentes (HU-09 CA-3).</summary>
    public async Task<bool> PuedeRegistrarAvancesAsync(int proyectoId, UsuarioAutenticado actor, CancellationToken ct = default)
    {
        if (!actor.Roles.Any(r => Politicas.Matriz[Politicas.RegistrarAvances].Contains(r)))
            return false;
        if (actor.Tiene(Roles.Administrador) || actor.Tiene(Roles.Director))
            return true;
        if (actor.Tiene(Roles.Coordinador) && await acceso.EsCoordinadorDelProyectoAsync(actor.Id, proyectoId, ct))
            return true;
        return await acceso.EsIntegranteProyectoAsync(actor.Id, proyectoId, ct);
    }

    // ----- Apoyo -----

    private async Task<bool> PuedeEnviarAsync(Proyecto proyecto, UsuarioAutenticado actor, CancellationToken ct)
    {
        if (proyecto.EstadoId is not (ValoresProyecto.EnElaboracion or ValoresProyecto.Devuelto))
            return false;
        if (actor.Tiene(Roles.Investigador) && proyecto.InvestigadorPrincipalId == actor.Id)
            return true;
        return await acceso.PuedeGestionarProgramaAsync(actor, proyecto.ProgramaId, ct)
            && actor.Roles.Any(r => Politicas.Matriz[Politicas.EditarPropuestas].Contains(r));
    }

    /// <summary>
    /// Anexo A: "Revisar / devolver propuestas" Director y Coordinador de su programa; "Otorgar aval"
    /// Administrador y Director. La etapa Coordinador la atiende el Coordinador vigente del programa
    /// (o el Director); la etapa Director, el Director.
    /// </summary>
    private async Task<IReadOnlyList<string>> DecisionesDisponiblesAsync(Proyecto proyecto, string etapa, UsuarioAutenticado actor, CancellationToken ct)
    {
        if (etapa == DecisionesRevision.EtapaCoordinador)
        {
            var puede = actor.Tiene(Roles.Director)
                || (actor.Tiene(Roles.Coordinador) && await acceso.EsCoordinadorVigenteAsync(actor.Id, proyecto.ProgramaId, ct));
            return puede ? [DecisionesRevision.Aprueba, DecisionesRevision.Devuelve] : [];
        }

        var decisiones = new List<string>();
        if (actor.Tiene(Roles.Director) || actor.Tiene(Roles.Administrador))
            decisiones.Add(DecisionesRevision.Avala);
        if (actor.Tiene(Roles.Director))
            decisiones.Add(DecisionesRevision.Devuelve);
        return decisiones;
    }

    private static void EsDirectivoAval(UsuarioAutenticado actor)
    {
        if (!actor.Roles.Any(r => Politicas.Matriz[Politicas.AvalarAprobarProyectos].Contains(r)))
            throw new AccesoDenegadoException();
    }

    private async Task<Proyecto> CargarAsync(int proyectoId, CancellationToken ct) =>
        await db.Proyectos.FirstOrDefaultAsync(p => p.Id == proyectoId && p.Activo, ct)
        ?? throw new NoEncontradoException("El proyecto no existe.");

    private void Registrar(Proyecto proyecto, int revisorId, string etapa, string decision, int estadoNuevo, string? observaciones) =>
        db.ProyectoRevisiones.Add(new ProyectoRevision
        {
            ProyectoId = proyecto.Id,
            RevisorId = revisorId,
            Etapa = etapa,
            Decision = decision,
            EstadoAnteriorId = proyecto.EstadoId,
            EstadoNuevoId = estadoNuevo,
            Observaciones = observaciones,
            Fecha = Ahora,
        });

    private async Task<List<int>> DirectoresAsync(CancellationToken ct) =>
        await db.Usuarios.Where(u => u.Activo && u.TieneAcceso && u.Roles.Any(r => r.Rol!.Nombre == Roles.Director))
            .Select(u => u.Id).ToListAsync(ct);

    private async Task<List<int>> EquipoVigenteAsync(int proyectoId, CancellationToken ct)
    {
        var hoy = Hoy;
        return await db.ProyectoIntegrantes
            .Where(pi => pi.ProyectoId == proyectoId && pi.Activo && (pi.FechaFin == null || pi.FechaFin > hoy))
            .Select(pi => pi.UsuarioId).Distinct().ToListAsync(ct);
    }
}
