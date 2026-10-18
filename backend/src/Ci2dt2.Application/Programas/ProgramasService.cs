using Ci2dt2.Application.Common;
using Ci2dt2.Application.Seguridad;
using Ci2dt2.Domain;
using Ci2dt2.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ci2dt2.Application.Programas;

/// <summary>Comandos de programas, objetivos, integrantes y aliados del programa (HU-06 y HU-07).</summary>
public sealed class ProgramasService(IApplicationDbContext db, AccesoRecursos acceso, Notificador notificador, TimeProvider reloj)
{
    private DateOnly Hoy => DateOnly.FromDateTime(reloj.GetUtcNow().UtcDateTime);

    public async Task ActualizarAsync(int id, ActualizarProgramaRequest request, CancellationToken ct = default)
    {
        var programa = await CargarProgramaAsync(id, ct);
        programa.Nombre = request.Nombre.Trim();
        programa.Descripcion = string.IsNullOrWhiteSpace(request.Descripcion) ? null : request.Descripcion.Trim();
        await db.SaveChangesAsync(ct);
    }

    // ----- Objetivos específicos -----

    public async Task<int> CrearObjetivoAsync(int programaId, ObjetivoRequest request, CancellationToken ct = default)
    {
        await CargarProgramaAsync(programaId, ct);
        var ultimo = await db.ObjetivosEspecificos.Where(o => o.ProgramaId == programaId && o.Activo)
            .MaxAsync(o => (int?)o.Orden, ct) ?? 0;
        var objetivo = new ObjetivoEspecifico { ProgramaId = programaId, Descripcion = request.Descripcion.Trim(), Orden = ultimo + 1 };
        db.ObjetivosEspecificos.Add(objetivo);
        await db.SaveChangesAsync(ct);
        return objetivo.Id;
    }

    public async Task ActualizarObjetivoAsync(int programaId, int objetivoId, ObjetivoRequest request, CancellationToken ct = default)
    {
        var objetivo = await CargarObjetivoAsync(programaId, objetivoId, ct);
        objetivo.Descripcion = request.Descripcion.Trim();
        await db.SaveChangesAsync(ct);
    }

    /// <summary>Borrado lógico del objetivo; el orden de los restantes se compacta.</summary>
    public async Task EliminarObjetivoAsync(int programaId, int objetivoId, CancellationToken ct = default)
    {
        var objetivo = await CargarObjetivoAsync(programaId, objetivoId, ct);
        objetivo.Activo = false;
        var restantes = await db.ObjetivosEspecificos
            .Where(o => o.ProgramaId == programaId && o.Activo && o.Id != objetivoId)
            .OrderBy(o => o.Orden).ThenBy(o => o.Id).ToListAsync(ct);
        for (var i = 0; i < restantes.Count; i++)
            restantes[i].Orden = i + 1;
        await db.SaveChangesAsync(ct);
    }

    /// <summary>Guarda el nuevo orden (HU-06 CA-2): la lista debe contener todos los objetivos activos.</summary>
    public async Task ReordenarObjetivosAsync(int programaId, IReadOnlyList<int> ids, CancellationToken ct = default)
    {
        await CargarProgramaAsync(programaId, ct);
        var objetivos = await db.ObjetivosEspecificos.Where(o => o.ProgramaId == programaId && o.Activo).ToListAsync(ct);
        if (objetivos.Count != ids.Count || objetivos.Any(o => !ids.Contains(o.Id)))
            throw new ReglaNegocioException("El nuevo orden debe incluir todos los objetivos del programa.");
        foreach (var objetivo in objetivos)
            objetivo.Orden = ids.ToList().IndexOf(objetivo.Id) + 1;
        await db.SaveChangesAsync(ct);
    }

    // ----- Integrantes -----

    /// <summary>
    /// Vincula un usuario al programa. Solo el Director y el Administrador asignan Coordinadores; un
    /// Coordinador vincula Investigadores de su propio programa.
    /// </summary>
    public async Task<int> VincularIntegranteAsync(int programaId, VincularIntegranteRequest request, UsuarioAutenticado actor, CancellationToken ct = default)
    {
        await CargarProgramaAsync(programaId, ct);
        await ValidarPermisoRolAsync(actor, programaId, request.RolEnPrograma, ct);
        ValidarFechas(request.FechaInicio, request.FechaFin);

        var usuario = await db.Usuarios.Include(u => u.Roles).ThenInclude(ur => ur.Rol)
            .FirstOrDefaultAsync(u => u.Id == request.UsuarioId && u.Activo, ct)
            ?? throw new ReglaNegocioException("El usuario seleccionado no existe.");

        // Sin el rol de sistema correspondiente la vinculación no daría permisos (HU-05).
        var rolSistema = request.RolEnPrograma == RolesPrograma.Coordinador ? Roles.Coordinador : Roles.Investigador;
        if (usuario.Roles.All(ur => ur.Rol!.Nombre != rolSistema))
            throw new ReglaNegocioException($"El usuario debe tener el rol {rolSistema} para vincularse como {request.RolEnPrograma}.");

        if (EsVigente(request.FechaInicio, request.FechaFin) || request.FechaFin is null)
        {
            var vinculaciones = await VinculacionesActivasAsync(programaId, ct);
            if (vinculaciones.Any(v => v.UsuarioId == request.UsuarioId && v.RolEnPrograma == request.RolEnPrograma && EsVigente(v.FechaInicio, v.FechaFin)))
                throw new ReglaNegocioException("El usuario ya tiene una vinculación vigente con ese rol en el programa.");
            if (request.RolEnPrograma == RolesPrograma.Coordinador
                && vinculaciones.Any(v => v.RolEnPrograma == RolesPrograma.Coordinador && EsVigente(v.FechaInicio, v.FechaFin)))
                throw new ReglaNegocioException("El programa ya tiene un Coordinador vigente; primero cierre su vinculación.");
        }

        var vinculacion = new ProgramaIntegrante
        {
            ProgramaId = programaId,
            UsuarioId = request.UsuarioId,
            RolEnPrograma = request.RolEnPrograma,
            FechaInicio = request.FechaInicio,
            FechaFin = request.FechaFin,
        };
        db.ProgramaIntegrantes.Add(vinculacion);

        // HU-06 CA-5: el usuario recibe una notificación; sus permisos se actualizan al renovar la sesión.
        var programa = await db.Programas.Where(p => p.Id == programaId).Select(p => p.Nombre).FirstAsync(ct);
        notificador.Notificar(request.UsuarioId, "VinculacionPrograma", $"Vinculación al programa {programa}",
            $"Fue vinculado al programa {programa} como {request.RolEnPrograma}.", "Programa", programaId);

        await db.SaveChangesAsync(ct);
        return vinculacion.Id;
    }

    /// <summary>Actualiza las fechas de una vinculación; el retiro se registra con FechaFin (no se borra).</summary>
    public async Task ActualizarVinculacionAsync(int programaId, int integranteId, ActualizarVinculacionRequest request, UsuarioAutenticado actor, CancellationToken ct = default)
    {
        var vinculacion = await db.ProgramaIntegrantes
            .FirstOrDefaultAsync(pi => pi.Id == integranteId && pi.ProgramaId == programaId && pi.Activo, ct)
            ?? throw new NoEncontradoException("La vinculación no existe.");
        await ValidarPermisoRolAsync(actor, programaId, vinculacion.RolEnPrograma, ct);
        ValidarFechas(request.FechaInicio, request.FechaFin);

        if (vinculacion.RolEnPrograma == RolesPrograma.Coordinador && EsVigente(request.FechaInicio, request.FechaFin))
        {
            var otroVigente = (await VinculacionesActivasAsync(programaId, ct)).Any(v =>
                v.Id != integranteId && v.RolEnPrograma == RolesPrograma.Coordinador && EsVigente(v.FechaInicio, v.FechaFin));
            if (otroVigente)
                throw new ReglaNegocioException("El programa ya tiene un Coordinador vigente; primero cierre su vinculación.");
        }

        vinculacion.FechaInicio = request.FechaInicio;
        vinculacion.FechaFin = request.FechaFin;
        await db.SaveChangesAsync(ct);
    }

    // ----- Aliados del programa (HU-07) -----

    public async Task AsignarAliadosAsync(int programaId, IReadOnlyList<int> aliadoIds, UsuarioAutenticado actor, CancellationToken ct = default)
    {
        await CargarProgramaAsync(programaId, ct);
        if (!await acceso.PuedeGestionarProgramaAsync(actor, programaId, ct))
            throw new AccesoDenegadoException();

        var ids = aliadoIds.Distinct().ToList();
        var existentes = await db.Aliados.CountAsync(a => ids.Contains(a.Id) && a.Activo, ct);
        if (existentes != ids.Count)
            throw new ReglaNegocioException("Uno o más aliados seleccionados no existen.");

        var actuales = await db.ProgramaAliados.Where(pa => pa.ProgramaId == programaId).ToListAsync(ct);
        foreach (var actual in actuales)
            actual.Activo = ids.Contains(actual.AliadoId);
        foreach (var id in ids.Where(id => actuales.All(a => a.AliadoId != id)))
            db.ProgramaAliados.Add(new ProgramaAliado { ProgramaId = programaId, AliadoId = id });
        await db.SaveChangesAsync(ct);
    }

    private async Task ValidarPermisoRolAsync(UsuarioAutenticado actor, int programaId, string rolEnPrograma, CancellationToken ct)
    {
        var esDirectivo = actor.Tiene(Roles.Administrador) || actor.Tiene(Roles.Director);
        var permitido = rolEnPrograma == RolesPrograma.Coordinador
            ? esDirectivo
            : await acceso.PuedeGestionarProgramaAsync(actor, programaId, ct);
        if (!permitido)
            throw new AccesoDenegadoException();
    }

    private static void ValidarFechas(DateOnly? inicio, DateOnly? fin)
    {
        if (inicio is not null && fin is not null && fin < inicio)
            throw new ReglaNegocioException("La fecha de fin no puede ser anterior a la fecha de inicio.");
    }

    private bool EsVigente(DateOnly? inicio, DateOnly? fin) =>
        (inicio is null || inicio <= Hoy) && (fin is null || fin > Hoy);

    private Task<List<ProgramaIntegrante>> VinculacionesActivasAsync(int programaId, CancellationToken ct) =>
        db.ProgramaIntegrantes.Where(pi => pi.ProgramaId == programaId && pi.Activo).ToListAsync(ct);

    private async Task<Programa> CargarProgramaAsync(int id, CancellationToken ct) =>
        await db.Programas.FirstOrDefaultAsync(p => p.Id == id && p.Activo, ct)
        ?? throw new NoEncontradoException("El programa no existe.");

    private async Task<ObjetivoEspecifico> CargarObjetivoAsync(int programaId, int objetivoId, CancellationToken ct) =>
        await db.ObjetivosEspecificos.FirstOrDefaultAsync(o => o.Id == objetivoId && o.ProgramaId == programaId && o.Activo, ct)
        ?? throw new NoEncontradoException("El objetivo no existe.");
}
