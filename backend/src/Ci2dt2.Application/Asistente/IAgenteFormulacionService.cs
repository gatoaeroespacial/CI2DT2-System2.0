namespace Ci2dt2.Application.Asistente;

public sealed record ResumenConvocatoria(
    string Entidad,
    string ObjetivoConvocatoria,
    string? MontoMaximo,
    IReadOnlyList<string> RubrosFinanciables,
    IReadOnlyList<string> RequisitosElegibilidad,
    IReadOnlyList<string> CriteriosEvaluacion,
    IReadOnlyList<string> EntregablesEsperados,
    string? FechaCierre);

public sealed record IdeaProyectoPropuesta(
    string Id,
    string Titulo,
    string DescripcionBreve,
    string JustificacionAlineacion,
    string ImpactoEsperado,
    IReadOnlyList<string> PalabrasClave);

public sealed record AnalisisTdrRequest(
    string TextoTdr,
    string? NombreConvocatoria,
    string LineaInvestigacionUsuario,
    string? NombrePrograma,
    string? NombreInvestigador);

public sealed record AnalisisTdrResultado(
    ResumenConvocatoria Resumen,
    IReadOnlyList<IdeaProyectoPropuesta> IdeasPropuestas);

public sealed record RedactarSeccionRequest(
    int BorradorId,
    string CodigoSeccion,
    string TituloProyecto,
    string? IdeaSeleccionada,
    string? ConvocatoriaMetadataJson,
    IReadOnlyDictionary<string, string> SeccionesExistentes,
    string? InstruccionesUsuario);

public sealed record SeccionAuditoriaItem(
    string CodigoSeccion,
    string TituloSeccion,
    string Estado, // Completa | Incompleta | Faltante
    string? ContenidoDetectado,
    string Recomendaciones);

public sealed record AuditoriaBorradorRequest(
    string TextoBorradorCompleto,
    string? ConvocatoriaMetadataJson);

public sealed record AuditoriaBorradorResultado(
    string DiagnosticoGeneral,
    IReadOnlyList<SeccionAuditoriaItem> Secciones);

public interface IAgenteFormulacionService
{
    Task<AnalisisTdrResultado> AnalizarTdrEIdearAsync(AnalisisTdrRequest request, CancellationToken ct = default);
    IAsyncEnumerable<string> RedactarSeccionStreamAsync(RedactarSeccionRequest request, CancellationToken ct = default);
    Task<AuditoriaBorradorResultado> AuditarBorradorAsync(AuditoriaBorradorRequest request, CancellationToken ct = default);
}

