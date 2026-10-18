using Ci2dt2.Application.Common;
using Ci2dt2.Application.Seguridad;
using Ci2dt2.Domain;
using Ci2dt2.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ci2dt2.Application.Proyectos;

/// <summary>Permisos del usuario sobre un proyecto (pestañas y acciones de la ficha).</summary>
public sealed record PermisosProyecto(bool PuedeEditar, bool PuedeCambiarIp, bool PuedeGestionarDocumentos);

/// <summary>Comandos de proyectos (HU-08), equipo (HU-09) y financiación (HU-10).</summary>
public sealed class ProyectosService(IApplicationDbContext db, AccesoRecursos acceso, Notificador notificador, TimeProvider reloj)
{
    private DateOnly Hoy => DateOnly.FromDateTime(reloj.GetUtcNow().UtcDateTime);

    // ----- HU-08 -----

    public async Task<int> CrearAsync(CrearProyectoRequest r, UsuarioAutenticado actor, CancellationToken ct = default)
    {
        if (!await db.Programas.AnyAsync(p => p.Id == r.ProgramaId && p.Activo, ct))
            throw new ReglaNegocioException("El programa no existe.");
        await ValidarTipoAsync(r.TipoProyectoId, ct);

        // Admin, Director o Coordinador del programa designan al IP; un Investigador queda como IP.
        int ipId;
        if (await acceso.PuedeGestionarProgramaAsync(actor, r.ProgramaId, ct))
            ipId = r.InvestigadorPrincipalId ?? actor.Id;
        else if (actor.Tiene(Roles.Investigador))
            ipId = actor.Id;
        else
            throw new AccesoDenegadoException();

        if (!await db.Usuarios.AnyAsync(u => u.Id == ipId && u.Activo, ct))
            throw new ReglaNegocioException("El investigador principal no existe.");
        if (!await acceso.EsAdscritoVigenteAsync(ipId, r.ProgramaId, ct))
            throw new ReglaNegocioException("El investigador principal debe tener adscripción vigente al programa seleccionado.");

        var proyecto = new Proyecto
        {
            ProgramaId = r.ProgramaId,
            InvestigadorPrincipalId = ipId,
            EstadoId = ValoresProyecto.EnElaboracion,
            PorcentajeAvance = 0,
        };
        Aplicar(proyecto, r);
        db.Proyectos.Add(proyecto);
        db.ProyectoIntegrantes.Add(new ProyectoIntegrante
        {
            Proyecto = proyecto,
            UsuarioId = ipId,
            RolEnProyecto = ValoresProyecto.RolIp,
            FechaInicio = Hoy,
        });

        await GuardarConCodigoAsync(proyecto, ct);

        // HU-08 CA-5: se notifica al Coordinador del programa.
        var coordinador = await notificador.CoordinadorVigenteAsync(r.ProgramaId, ct);
        if (coordinador is not null)
        {
            notificador.Notificar(coordinador.Value, "PropuestaRegistrada", $"Nueva propuesta {proyecto.Codigo}",
                $"Se registró la propuesta \"{proyecto.Titulo}\" en su programa.", "Proyecto", proyecto.Id);
            await db.SaveChangesAsync(ct);
        }
        return proyecto.Id;
    }

    public async Task ActualizarAsync(int id, ActualizarProyectoRequest r, UsuarioAutenticado actor, CancellationToken ct = default)
    {
        var proyecto = await CargarParaEditarAsync(id, actor, ct);
        await ValidarTipoAsync(r.TipoProyectoId, ct);
        Aplicar(proyecto, r);
        await db.SaveChangesAsync(ct);
    }

    public async Task<PermisosProyecto> PermisosAsync(int id, UsuarioAutenticado actor, CancellationToken ct = default)
    {
        var proyecto = await db.Proyectos.Include(p => p.Estado).FirstOrDefaultAsync(p => p.Id == id && p.Activo, ct)
            ?? throw new NoEncontradoException("El proyecto no existe.");
        var editar = await EvaluarEdicionAsync(proyecto, actor, ct) is null;
        var cambiarIp = !proyecto.Estado!.EsFinal && await PuedeCambiarIpAsync(proyecto, actor, ct);
        var documentos = actor.Roles.Any(r => Politicas.Matriz[Politicas.GestionarDocumentos].Contains(r))
            && await acceso.PuedeVerProyectoAsync(actor, id, ct);
        return new PermisosProyecto(editar, cambiarIp, documentos);
    }

    // ----- HU-09 -----

    public async Task<int> AgregarIntegranteAsync(int proyectoId, AgregarIntegranteRequest r, UsuarioAutenticado actor, CancellationToken ct = default)
    {
        var proyecto = await CargarParaEditarAsync(proyectoId, actor, ct);
        ValidarFechas(r.FechaInicio, r.FechaFin);
        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == r.UsuarioId && u.Activo, ct)
            ?? throw new ReglaNegocioException("El usuario no existe. Solicite su creación al Coordinador o al Administrador.");

        var integrantes = await db.ProyectoIntegrantes.Where(pi => pi.ProyectoId == proyectoId && pi.Activo).ToListAsync(ct);
        if (integrantes.Any(pi => pi.UsuarioId == r.UsuarioId && pi.RolEnProyecto == r.RolEnProyecto && EsVigente(pi.FechaFin)))
            throw new ReglaNegocioException("El usuario ya tiene una vinculación vigente con ese rol en el proyecto.");

        if (r.RolEnProyecto == ValoresProyecto.RolIp)
        {
            // Cambio de IP: solo Coordinador o Director; el IP anterior se retira (exactamente uno vigente).
            if (!await PuedeCambiarIpAsync(proyecto, actor, ct))
                throw new AccesoDenegadoException("Solo el Coordinador del programa o el Director pueden cambiar el IP.");
            if (!await acceso.EsAdscritoVigenteAsync(r.UsuarioId, proyecto.ProgramaId, ct))
                throw new ReglaNegocioException("El nuevo IP debe tener adscripción vigente al programa del proyecto.");
            foreach (var anterior in integrantes.Where(pi => pi.RolEnProyecto == ValoresProyecto.RolIp && EsVigente(pi.FechaFin)))
                anterior.FechaFin = Hoy;
            proyecto.InvestigadorPrincipalId = r.UsuarioId;
        }

        var integrante = new ProyectoIntegrante
        {
            ProyectoId = proyectoId,
            UsuarioId = usuario.Id,
            RolEnProyecto = r.RolEnProyecto,
            FechaInicio = r.FechaInicio ?? Hoy,
            FechaFin = r.FechaFin,
        };
        db.ProyectoIntegrantes.Add(integrante);

        // HU-09 CA-1: el integrante recibe una notificación y ve el proyecto en "Mis proyectos".
        notificador.Notificar(usuario.Id, "IntegranteProyecto", $"Fue agregado al proyecto {proyecto.Codigo}",
            $"Fue agregado al proyecto \"{proyecto.Titulo}\" como {r.RolEnProyecto}.", "Proyecto", proyectoId);
        await db.SaveChangesAsync(ct);
        return integrante.Id;
    }

    /// <summary>Actualiza fechas; el retiro se registra con FechaFin y se conserva el historial.</summary>
    public async Task ActualizarIntegranteAsync(int proyectoId, int integranteId, ActualizarIntegranteRequest r, UsuarioAutenticado actor, CancellationToken ct = default)
    {
        var proyecto = await CargarParaEditarAsync(proyectoId, actor, ct);
        var integrante = await db.ProyectoIntegrantes
            .FirstOrDefaultAsync(pi => pi.Id == integranteId && pi.ProyectoId == proyectoId && pi.Activo, ct)
            ?? throw new NoEncontradoException("El integrante no existe.");
        ValidarFechas(r.FechaInicio ?? integrante.FechaInicio, r.FechaFin);

        if (integrante.RolEnProyecto == ValoresProyecto.RolIp && EsVigente(integrante.FechaFin))
        {
            if (!await PuedeCambiarIpAsync(proyecto, actor, ct))
                throw new AccesoDenegadoException("Solo el Coordinador del programa o el Director pueden modificar al IP.");
            if (!EsVigente(r.FechaFin))
                throw new ReglaNegocioException("El proyecto debe tener un IP vigente: para cambiarlo agregue al nuevo IP.");
        }

        integrante.FechaInicio = r.FechaInicio ?? integrante.FechaInicio;
        integrante.FechaFin = r.FechaFin;
        await db.SaveChangesAsync(ct);
    }

    // ----- HU-10 -----

    public async Task<int> CrearFuenteAsync(int proyectoId, FuenteRequest r, UsuarioAutenticado actor, CancellationToken ct = default)
    {
        await CargarParaEditarAsync(proyectoId, actor, ct);
        await ValidarFuenteAsync(r, ct);
        var fuente = new FuenteFinanciamiento { ProyectoId = proyectoId };
        AplicarFuente(fuente, r);
        db.FuentesFinanciamiento.Add(fuente);
        await db.SaveChangesAsync(ct);
        return fuente.Id;
    }

    public async Task ActualizarFuenteAsync(int proyectoId, int fuenteId, FuenteRequest r, UsuarioAutenticado actor, CancellationToken ct = default)
    {
        await CargarParaEditarAsync(proyectoId, actor, ct);
        var fuente = await CargarFuenteAsync(proyectoId, fuenteId, ct);
        await ValidarFuenteAsync(r, ct);
        AplicarFuente(fuente, r);
        await db.SaveChangesAsync(ct);
    }

    public async Task EliminarFuenteAsync(int proyectoId, int fuenteId, UsuarioAutenticado actor, CancellationToken ct = default)
    {
        await CargarParaEditarAsync(proyectoId, actor, ct);
        var fuente = await CargarFuenteAsync(proyectoId, fuenteId, ct);
        fuente.Activo = false;
        await db.SaveChangesAsync(ct);
    }

    private async Task ValidarFuenteAsync(FuenteRequest r, CancellationToken ct)
    {
        // HU-10 CA-3: una Cofinanciación exige el aliado.
        if (r.TipoFuente == ValoresProyecto.Cofinanciacion && r.AliadoId is null)
            throw new ReglaNegocioException("Una fuente de tipo Cofinanciación exige seleccionar el aliado.");
        if (r.AliadoId is not null && !await db.Aliados.AnyAsync(a => a.Id == r.AliadoId && a.Activo, ct))
            throw new ReglaNegocioException("El aliado seleccionado no existe.");
    }

    private static void AplicarFuente(FuenteFinanciamiento f, FuenteRequest r)
    {
        f.AliadoId = r.AliadoId;
        f.TipoFuente = r.TipoFuente;
        f.TipoAporte = r.TipoAporte;
        f.Moneda = r.Moneda;
        f.Monto = r.Monto;
        f.TasaCambio = r.Moneda == "COP" ? null : r.TasaCambio;
        f.MontoCop = ValoresProyecto.EnCop(r.Monto, r.Moneda, r.TasaCambio);
        f.Estado = r.Estado;
    }

    private async Task<FuenteFinanciamiento> CargarFuenteAsync(int proyectoId, int fuenteId, CancellationToken ct) =>
        await db.FuentesFinanciamiento.FirstOrDefaultAsync(f => f.Id == fuenteId && f.ProyectoId == proyectoId && f.Activo, ct)
        ?? throw new NoEncontradoException("La fuente de financiamiento no existe.");

    // ----- Reglas comunes -----

    /// <summary>
    /// Admin, Director y Coordinador del programa editan mientras el proyecto no esté en un estado
    /// final; el IP solo en "En elaboración" o "Devuelto" (Anexo A y HU-08).
    /// </summary>
    private async Task<Proyecto> CargarParaEditarAsync(int id, UsuarioAutenticado actor, CancellationToken ct)
    {
        var proyecto = await db.Proyectos.Include(p => p.Estado).FirstOrDefaultAsync(p => p.Id == id && p.Activo, ct)
            ?? throw new NoEncontradoException("El proyecto no existe.");
        var error = await EvaluarEdicionAsync(proyecto, actor, ct);
        if (error is not null)
            throw error;
        return proyecto;
    }

    private async Task<Exception?> EvaluarEdicionAsync(Proyecto proyecto, UsuarioAutenticado actor, CancellationToken ct)
    {
        var puedeEditarRol = actor.Roles.Any(r => Politicas.Matriz[Politicas.EditarPropuestas].Contains(r));
        if (!puedeEditarRol)
            return new AccesoDenegadoException();

        var esDirectivo = actor.Tiene(Roles.Administrador) || actor.Tiene(Roles.Director)
            || (actor.Tiene(Roles.Coordinador) && await acceso.EsCoordinadorVigenteAsync(actor.Id, proyecto.ProgramaId, ct));
        var esIp = actor.Tiene(Roles.Investigador) && proyecto.InvestigadorPrincipalId == actor.Id;
        if (!esDirectivo && !esIp)
            return new AccesoDenegadoException();

        if (proyecto.Estado!.EsFinal)
            return new ReglaNegocioException($"El proyecto está en estado \"{proyecto.Estado.Nombre}\" y no admite cambios.");
        if (!esDirectivo && proyecto.EstadoId is not (ValoresProyecto.EnElaboracion or ValoresProyecto.Devuelto))
            return new ReglaNegocioException("El IP solo puede editar la propuesta en estado \"En elaboración\" o \"Devuelto\".");
        return null;
    }

    private async Task<bool> PuedeCambiarIpAsync(Proyecto proyecto, UsuarioAutenticado actor, CancellationToken ct) =>
        actor.Tiene(Roles.Director)
        || (actor.Tiene(Roles.Coordinador) && await acceso.EsCoordinadorVigenteAsync(actor.Id, proyecto.ProgramaId, ct));

    private async Task ValidarTipoAsync(int tipoProyectoId, CancellationToken ct)
    {
        if (!await db.TiposProyecto.AnyAsync(t => t.Id == tipoProyectoId && t.Activo, ct))
            throw new ReglaNegocioException("El tipo de proyecto no existe.");
    }

    private static void Aplicar(Proyecto p, IDatosProyecto r)
    {
        p.Titulo = r.Titulo.Trim();
        p.Resumen = string.IsNullOrWhiteSpace(r.Resumen) ? null : r.Resumen.Trim();
        p.TipoProyectoId = r.TipoProyectoId;
        p.FechaInicio = r.FechaInicio;
        p.FechaFin = r.FechaFin;
        p.Moneda = r.Moneda;
        p.PresupuestoTotal = r.PresupuestoTotal;
        p.TasaCambio = r.Moneda is null or "COP" ? null : r.TasaCambio;
        p.PresupuestoTotalCop = ValoresProyecto.EnCop(r.PresupuestoTotal, r.Moneda, r.TasaCambio);
    }

    /// <summary>Código CI2DT2-{AAAA}-{consecutivo de 3 dígitos}, único y no editable.</summary>
    private async Task GuardarConCodigoAsync(Proyecto proyecto, CancellationToken ct)
    {
        var prefijo = $"CI2DT2-{reloj.GetUtcNow().Year}-";
        for (var intento = 1; ; intento++)
        {
            var codigos = await db.Proyectos.Where(p => p.Codigo != null && p.Codigo.StartsWith(prefijo))
                .Select(p => p.Codigo!).ToListAsync(ct);
            var siguiente = codigos.Select(c => int.TryParse(c[prefijo.Length..], out var n) ? n : 0).DefaultIfEmpty(0).Max() + 1;
            proyecto.Codigo = $"{prefijo}{siguiente:D3}";
            try
            {
                await db.SaveChangesAsync(ct);
                return;
            }
            catch (DbUpdateException) when (intento < 3)
            {
                // Otro proyecto tomó el mismo consecutivo al mismo tiempo: se recalcula.
            }
        }
    }

    private static void ValidarFechas(DateOnly? inicio, DateOnly? fin)
    {
        if (inicio is not null && fin is not null && fin < inicio)
            throw new ReglaNegocioException("La fecha de fin no puede ser anterior a la fecha de inicio.");
    }

    private bool EsVigente(DateOnly? fechaFin) => fechaFin is null || fechaFin > Hoy;
}
