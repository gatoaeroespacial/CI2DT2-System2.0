using Ci2dt2.Domain;
using FluentValidation;

namespace Ci2dt2.Application.Proyectos;

/// <summary>Valores permitidos según el diccionario de datos y el backlog.</summary>
public static class ValoresProyecto
{
    public static readonly string[] Monedas = ["COP", "USD", "EUR"];

    // HU-09: IP, Co-investigador, Auxiliar de campo y Estudiante.
    public const string RolIp = RolesProyecto.InvestigadorPrincipal;
    public static readonly string[] RolesEnProyecto = [RolIp, "Co-investigador", "Auxiliar de campo", "Estudiante"];

    // HU-10.
    public const string Cofinanciacion = "Cofinanciación";
    public static readonly string[] TiposFuente = ["Convocatoria", "Contrapartida", Cofinanciacion, "Recursos propios"];
    public static readonly string[] TiposAporte = ["Efectivo", "Especie"];
    public static readonly string[] EstadosFuente = ["Estimado", "Comprometido", "Desembolsado"];

    // Id fijos del catálogo EstadosProyecto (datos semilla).
    public const int EnElaboracion = 1;
    public const int Devuelto = 3;

    /// <summary>Monto en pesos: igual al monto si es COP; si no, monto × tasa de cambio.</summary>
    public static decimal? EnCop(decimal? monto, string? moneda, decimal? tasa) =>
        monto is null ? null
        : moneda == "COP" ? monto
        : tasa is > 0 ? Math.Round(monto.Value * tasa.Value, 2) : null;
}

// ----- HU-08 -----

public interface IDatosProyecto
{
    string Titulo { get; }
    string? Resumen { get; }
    int TipoProyectoId { get; }
    DateOnly? FechaInicio { get; }
    DateOnly? FechaFin { get; }
    string? Moneda { get; }
    decimal? PresupuestoTotal { get; }
    decimal? TasaCambio { get; }
}

public sealed record CrearProyectoRequest(
    string Titulo, string? Resumen, int ProgramaId, int TipoProyectoId, int? InvestigadorPrincipalId,
    DateOnly? FechaInicio, DateOnly? FechaFin, string? Moneda, decimal? PresupuestoTotal, decimal? TasaCambio) : IDatosProyecto;

public sealed record ActualizarProyectoRequest(
    string Titulo, string? Resumen, int TipoProyectoId,
    DateOnly? FechaInicio, DateOnly? FechaFin, string? Moneda, decimal? PresupuestoTotal, decimal? TasaCambio) : IDatosProyecto;

public abstract class DatosProyectoValidator<T> : AbstractValidator<T> where T : IDatosProyecto
{
    protected DatosProyectoValidator()
    {
        RuleFor(x => x.Titulo).NotEmpty().WithMessage("El título es obligatorio.")
            .Length(10, 300).WithMessage("El título debe tener entre 10 y 300 caracteres.");
        RuleFor(x => x.Resumen).MaximumLength(4000).WithMessage("El resumen admite máximo 4.000 caracteres.");
        RuleFor(x => x.TipoProyectoId).GreaterThan(0).WithMessage("Debe seleccionar el tipo de proyecto.");
        RuleFor(x => x.FechaFin).GreaterThan(x => x.FechaInicio)
            .WithMessage("La fecha de fin debe ser posterior a la fecha de inicio.")
            .When(x => x.FechaInicio is not null && x.FechaFin is not null);
        RuleFor(x => x.Moneda!).Must(m => ValoresProyecto.Monedas.Contains(m))
            .WithMessage("La moneda debe ser COP, USD o EUR.").When(x => x.Moneda is not null);
        RuleFor(x => x.Moneda).NotEmpty().WithMessage("Indique la moneda del presupuesto.")
            .When(x => x.PresupuestoTotal is not null);
        RuleFor(x => x.PresupuestoTotal).GreaterThanOrEqualTo(0).WithMessage("El presupuesto no puede ser negativo.");
        RuleFor(x => x.TasaCambio).NotNull().GreaterThan(0)
            .WithMessage("La tasa de cambio es obligatoria y mayor que 0 cuando la moneda no es COP.")
            .When(x => x.Moneda is not null && x.Moneda != "COP");
    }
}

public sealed class CrearProyectoRequestValidator : DatosProyectoValidator<CrearProyectoRequest>
{
    public CrearProyectoRequestValidator()
    {
        RuleFor(x => x.ProgramaId).GreaterThan(0).WithMessage("Debe seleccionar el programa.");
    }
}

public sealed class ActualizarProyectoRequestValidator : DatosProyectoValidator<ActualizarProyectoRequest>;

// ----- HU-09 -----

public sealed record AgregarIntegranteRequest(int UsuarioId, string RolEnProyecto, DateOnly? FechaInicio, DateOnly? FechaFin);

public sealed record ActualizarIntegranteRequest(DateOnly? FechaInicio, DateOnly? FechaFin);

public sealed class AgregarIntegranteRequestValidator : AbstractValidator<AgregarIntegranteRequest>
{
    public AgregarIntegranteRequestValidator()
    {
        RuleFor(x => x.UsuarioId).GreaterThan(0).WithMessage("Debe seleccionar un usuario.");
        RuleFor(x => x.RolEnProyecto).Must(r => ValoresProyecto.RolesEnProyecto.Contains(r))
            .WithMessage("El rol debe ser IP, Co-investigador, Auxiliar de campo o Estudiante.");
    }
}

public sealed record IntegranteProyectoItem(
    int Id, int UsuarioId, string Nombre, string Email, string RolEnProyecto,
    DateTime? FechaInicio, DateTime? FechaFin, bool Vigente, string? CvLacUrl, string? Orcid);

// ----- HU-10 -----

public sealed record FuenteRequest(
    int? AliadoId, string TipoFuente, string TipoAporte, string Moneda, decimal Monto, decimal? TasaCambio, string Estado);

public sealed class FuenteRequestValidator : AbstractValidator<FuenteRequest>
{
    public FuenteRequestValidator()
    {
        RuleFor(x => x.TipoFuente).Must(t => ValoresProyecto.TiposFuente.Contains(t))
            .WithMessage("El tipo de fuente debe ser Convocatoria, Contrapartida, Cofinanciación o Recursos propios.");
        RuleFor(x => x.TipoAporte).Must(t => ValoresProyecto.TiposAporte.Contains(t))
            .WithMessage("El tipo de aporte debe ser Efectivo o Especie.");
        RuleFor(x => x.Estado).Must(e => ValoresProyecto.EstadosFuente.Contains(e))
            .WithMessage("El estado debe ser Estimado, Comprometido o Desembolsado.");
        RuleFor(x => x.Moneda).Must(m => ValoresProyecto.Monedas.Contains(m)).WithMessage("La moneda debe ser COP, USD o EUR.");
        RuleFor(x => x.Monto).GreaterThan(0).WithMessage("El monto debe ser mayor que 0.");
        RuleFor(x => x.TasaCambio).NotNull().GreaterThan(0)
            .WithMessage("La tasa de cambio es obligatoria y mayor que 0 cuando la moneda no es COP.")
            .When(x => x.Moneda != "COP");
    }
}

public sealed record FuenteItem(
    int Id, int? AliadoId, string? Aliado, string? TipoFuente, string? TipoAporte, string? Moneda,
    decimal? Monto, decimal? TasaCambio, decimal? MontoCop, string Estado);

public sealed record TotalCategoria(string Categoria, decimal TotalCop);

/// <summary>Pestaña "Financiación" (HU-10 CA-1 y CA-2).</summary>
public sealed record FinanciacionProyecto(
    IReadOnlyList<FuenteItem> Fuentes,
    IReadOnlyList<TotalCategoria> PorTipoFuente,
    IReadOnlyList<TotalCategoria> PorTipoAporte,
    decimal TotalCop,
    decimal? PresupuestoTotalCop,
    decimal? Diferencia);
