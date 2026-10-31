using Ci2dt2.Application.Common;
using Ci2dt2.Application.Seguridad;
using Ci2dt2.Domain;
using Ci2dt2.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ci2dt2.Application.Asistente;

public sealed class BorradorProyectosService(
    IApplicationDbContext db,
    TimeProvider reloj)
{
    private static readonly (string Codigo, string Titulo, int Orden)[] SeccionesPredeterminadas =
    [
        ("titulo", "Título del Proyecto", 1),
        ("resumen", "Resumen Ejecutivo", 2),
        ("justificacion", "Planteamiento del Problema y Justificación", 3),
        ("estado_arte", "Marco Teórico y Estado del Arte", 4),
        ("objetivo_general", "Objetivo General", 5),
        ("objetivos_especificos", "Objetivos Específicos", 6),
        ("metodologia", "Metodología y Diseño Experimental", 7),
        ("cronograma", "Plan de Trabajo y Cronograma", 8),
        ("presupuesto", "Presupuesto y Rubros Solicitados", 9),
        ("impactos", "Resultados Esperados e Impactos", 10)
    ];

    public async Task<int> CrearBorradorAsync(
        CrearBorradorRequest request,
        UsuarioAutenticado usuario,
        CancellationToken ct = default)
    {
        var ahora = reloj.GetUtcNow().UtcDateTime;
        var borrador = new BorradorProyecto
        {
            UsuarioId = usuario.Id,
            TituloTentativo = string.IsNullOrWhiteSpace(request.TituloTentativo)
                ? "Nuevo Proyecto en Formulación"
                : request.TituloTentativo.Trim(),
            ProgramaId = request.ProgramaId,
            LineaInvestigacion = request.LineaInvestigacion?.Trim(),
            ConvocatoriaNombre = request.ConvocatoriaNombre?.Trim(),
            ConvocatoriaMetadataJson = request.ConvocatoriaMetadataJson,
            EstadoAvance = "Ideacion",
            CreadoEn = ahora,
            CreadoPorId = usuario.Id,
            Activo = true
        };

        foreach (var (codigo, titulo, orden) in SeccionesPredeterminadas)
        {
            borrador.Secciones.Add(new BorradorProyectoSeccion
            {
                CodigoSeccion = codigo,
                TituloSeccion = titulo,
                Orden = orden,
                Contenido = string.Empty,
                Estado = "Vacio",
                CreadoEn = ahora,
                CreadoPorId = usuario.Id,
                Activo = true
            });
        }

        db.BorradoresProyecto.Add(borrador);
        await db.SaveChangesAsync(ct);

        return borrador.Id;
    }

    public async Task<IReadOnlyList<BorradorResumenItem>> ListarBorradoresUsuarioAsync(
        UsuarioAutenticado usuario,
        CancellationToken ct = default)
    {
        var consulta = db.BorradoresProyecto
            .AsNoTracking()
            .Where(b => b.Activo);

        // Si no es Administrador ni Director, solo ve sus propios borradores
        if (!usuario.Roles.Contains(Roles.Administrador) && !usuario.Roles.Contains(Roles.Director))
        {
            consulta = consulta.Where(b => b.UsuarioId == usuario.Id);
        }

        var lista = await consulta
            .OrderByDescending(b => b.ActualizadoEn ?? b.CreadoEn)
            .Select(b => new BorradorResumenItem(
                b.Id,
                b.TituloTentativo,
                b.LineaInvestigacion,
                b.ConvocatoriaNombre,
                b.EstadoAvance,
                b.Secciones.Count(s => s.Activo && s.Estado != "Vacio"),
                b.Secciones.Count(s => s.Activo),
                b.CreadoEn,
                b.ActualizadoEn))
            .ToListAsync(ct);

        return lista;
    }

    public async Task<BorradorProyectoDetalle?> ObtenerBorradorAsync(
        int id,
        UsuarioAutenticado usuario,
        CancellationToken ct = default)
    {
        var borrador = await db.BorradoresProyecto
            .Include(b => b.Programa)
            .Include(b => b.Secciones.Where(s => s.Activo))
            .Include(b => b.Mensajes.OrderBy(m => m.FechaUtc))
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id && b.Activo, ct);

        if (borrador is null)
            return null;

        ValidarAccesoBorrador(borrador, usuario);

        var secciones = borrador.Secciones
            .OrderBy(s => s.Orden)
            .Select(s => new BorradorSeccionDetalle(
                s.Id,
                s.CodigoSeccion,
                s.TituloSeccion,
                s.Contenido,
                s.Orden,
                s.Estado,
                s.ObservacionesIA,
                ContarPalabras(s.Contenido)))
            .ToList();

        var mensajes = borrador.Mensajes
            .OrderBy(m => m.FechaUtc)
            .Select(m => new BorradorMensajeDetalle(
                m.Id,
                m.Rol,
                m.Texto,
                m.TipoMensaje,
                m.SeccionDestino,
                m.MetadatosJson,
                m.FechaUtc))
            .ToList();

        return new BorradorProyectoDetalle(
            borrador.Id,
            borrador.UsuarioId,
            borrador.TituloTentativo,
            borrador.ProgramaId,
            borrador.Programa?.Nombre,
            borrador.LineaInvestigacion,
            borrador.ConvocatoriaNombre,
            borrador.ConvocatoriaMetadataJson,
            borrador.EstadoAvance,
            borrador.CreadoEn,
            borrador.ActualizadoEn,
            secciones,
            mensajes);
    }

    public async Task<bool> ActualizarSeccionAsync(
        int borradorId,
        string codigoSeccion,
        ActualizarSeccionRequest request,
        UsuarioAutenticado usuario,
        CancellationToken ct = default)
    {
        var borrador = await db.BorradoresProyecto
            .Include(b => b.Secciones)
            .FirstOrDefaultAsync(b => b.Id == borradorId && b.Activo, ct);

        if (borrador is null)
            return false;

        ValidarAccesoBorrador(borrador, usuario);

        var seccion = borrador.Secciones.FirstOrDefault(s => s.CodigoSeccion == codigoSeccion && s.Activo);
        var ahora = reloj.GetUtcNow().UtcDateTime;

        if (seccion is null)
        {
            var maxOrden = borrador.Secciones.Count > 0 ? borrador.Secciones.Max(s => s.Orden) : 0;
            seccion = new BorradorProyectoSeccion
            {
                BorradorProyectoId = borradorId,
                CodigoSeccion = codigoSeccion,
                TituloSeccion = codigoSeccion,
                Orden = maxOrden + 1,
                CreadoEn = ahora,
                CreadoPorId = usuario.Id,
                Activo = true
            };
            borrador.Secciones.Add(seccion);
        }

        seccion.Contenido = request.Contenido;
        seccion.Estado = string.IsNullOrWhiteSpace(request.Estado)
            ? (string.IsNullOrWhiteSpace(request.Contenido) ? "Vacio" : "Modificado")
            : request.Estado;
        seccion.ObservacionesIA = request.ObservacionesIA ?? seccion.ObservacionesIA;
        seccion.ActualizadoEn = ahora;

        borrador.ActualizadoEn = ahora;
        if (borrador.EstadoAvance == "Ideacion")
            borrador.EstadoAvance = "EnRedaccion";

        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<long> AgregarMensajeAsync(
        int borradorId,
        AgregarMensajeBorradorRequest request,
        UsuarioAutenticado usuario,
        CancellationToken ct = default)
    {
        var borrador = await db.BorradoresProyecto
            .FirstOrDefaultAsync(b => b.Id == borradorId && b.Activo, ct);

        if (borrador is null)
            throw new KeyNotFoundException($"No se encontró el borrador con id {borradorId}.");

        ValidarAccesoBorrador(borrador, usuario);

        var ahora = reloj.GetUtcNow().UtcDateTime;
        var mensaje = new BorradorProyectoMensaje
        {
            BorradorProyectoId = borradorId,
            Rol = request.Rol,
            Texto = request.Texto,
            TipoMensaje = request.TipoMensaje,
            SeccionDestino = request.SeccionDestino,
            MetadatosJson = request.MetadatosJson,
            FechaUtc = ahora
        };

        db.BorradorProyectoMensajes.Add(mensaje);
        borrador.ActualizadoEn = ahora;

        await db.SaveChangesAsync(ct);
        return mensaje.Id;
    }

    public async Task ActualizarMetadataConvocatoriaAsync(
        int borradorId,
        string convocatoriaNombre,
        string metadataJson,
        UsuarioAutenticado usuario,
        CancellationToken ct = default)
    {
        var borrador = await db.BorradoresProyecto
            .FirstOrDefaultAsync(b => b.Id == borradorId && b.Activo, ct);

        if (borrador is null)
            throw new KeyNotFoundException($"No se encontró el borrador con id {borradorId}.");

        ValidarAccesoBorrador(borrador, usuario);

        borrador.ConvocatoriaNombre = convocatoriaNombre;
        borrador.ConvocatoriaMetadataJson = metadataJson;
        borrador.ActualizadoEn = reloj.GetUtcNow().UtcDateTime;

        await db.SaveChangesAsync(ct);
    }

    private static void ValidarAccesoBorrador(BorradorProyecto borrador, UsuarioAutenticado usuario)
    {
        if (borrador.UsuarioId == usuario.Id)
            return;

        if (usuario.Roles.Contains(Roles.Administrador) || usuario.Roles.Contains(Roles.Director))
            return;

        throw new UnauthorizedAccessException("No tiene permiso para acceder a este borrador de proyecto.");
    }

    private static int ContarPalabras(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return 0;

        return texto.Split([' ', '\r', '\n', '\t'], StringSplitOptions.RemoveEmptyEntries).Length;
    }
}

