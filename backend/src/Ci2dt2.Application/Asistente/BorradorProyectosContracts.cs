namespace Ci2dt2.Application.Asistente;

public sealed record CrearBorradorRequest(
    string TituloTentativo,
    int? ProgramaId,
    string? LineaInvestigacion,
    string? ConvocatoriaNombre,
    string? ConvocatoriaMetadataJson);

public sealed record ActualizarSeccionRequest(
    string Contenido,
    string? Estado = null,
    string? ObservacionesIA = null);

public sealed record AgregarMensajeBorradorRequest(
    string Rol,
    string Texto,
    string TipoMensaje = "chat",
    string? SeccionDestino = null,
    string? MetadatosJson = null);

public sealed record BorradorResumenItem(
    int Id,
    string TituloTentativo,
    string? LineaInvestigacion,
    string? ConvocatoriaNombre,
    string EstadoAvance,
    int SeccionesCompletadas,
    int TotalSecciones,
    DateTime CreadoEn,
    DateTime? ActualizadoEn);

public sealed record BorradorSeccionDetalle(
    int Id,
    string CodigoSeccion,
    string TituloSeccion,
    string Contenido,
    int Orden,
    string Estado,
    string? ObservacionesIA,
    int ConteoPalabras);

public sealed record BorradorMensajeDetalle(
    long Id,
    string Rol,
    string Texto,
    string TipoMensaje,
    string? SeccionDestino,
    string? MetadatosJson,
    DateTime FechaUtc);

public sealed record BorradorProyectoDetalle(
    int Id,
    int UsuarioId,
    string TituloTentativo,
    int? ProgramaId,
    string? NombrePrograma,
    string? LineaInvestigacion,
    string? ConvocatoriaNombre,
    string? ConvocatoriaMetadataJson,
    string EstadoAvance,
    DateTime CreadoEn,
    DateTime? ActualizadoEn,
    IReadOnlyList<BorradorSeccionDetalle> Secciones,
    IReadOnlyList<BorradorMensajeDetalle> Mensajes);

