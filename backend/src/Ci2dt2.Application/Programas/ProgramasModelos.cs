using Ci2dt2.Domain;
using FluentValidation;

namespace Ci2dt2.Application.Programas;

public sealed class ProgramaResumen
{
    public int Id { get; init; }
    public string Nombre { get; init; } = "";
    public string? Descripcion { get; init; }
    public string? Coordinador { get; init; }
    public int IntegrantesVigentes { get; init; }
}

public sealed record ObjetivoItem(int Id, string? Descripcion, int? Orden);

public sealed record AliadoProgramaItem(int Id, string Nombre);

public sealed record ProgramaDetalle(
    int Id, string Nombre, string? Descripcion,
    IReadOnlyList<ObjetivoItem> Objetivos, IReadOnlyList<AliadoProgramaItem> Aliados);

public sealed class IntegranteProgramaItem
{
    public int Id { get; init; }
    public int UsuarioId { get; init; }
    public string Nombre { get; init; } = "";
    public string Email { get; init; } = "";
    public string RolEnPrograma { get; init; } = "";
    public DateTime? FechaInicio { get; init; }
    public DateTime? FechaFin { get; init; }
    public bool Vigente { get; init; }
}

public sealed record ActualizarProgramaRequest(string Nombre, string? Descripcion);

public sealed record ObjetivoRequest(string Descripcion);

public sealed record ReordenarObjetivosRequest(IReadOnlyList<int> Ids);

public sealed record VincularIntegranteRequest(int UsuarioId, string RolEnPrograma, DateOnly? FechaInicio, DateOnly? FechaFin);

public sealed record ActualizarVinculacionRequest(DateOnly? FechaInicio, DateOnly? FechaFin);

public sealed record AsignarAliadosProgramaRequest(IReadOnlyList<int> AliadoIds);

public sealed class ActualizarProgramaRequestValidator : AbstractValidator<ActualizarProgramaRequest>
{
    public ActualizarProgramaRequestValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().WithMessage("El nombre es obligatorio.").MaximumLength(150);
    }
}

public sealed class ObjetivoRequestValidator : AbstractValidator<ObjetivoRequest>
{
    public ObjetivoRequestValidator()
    {
        RuleFor(x => x.Descripcion).NotEmpty().WithMessage("La descripción del objetivo es obligatoria.").MaximumLength(4000);
    }
}

public sealed class ReordenarObjetivosRequestValidator : AbstractValidator<ReordenarObjetivosRequest>
{
    public ReordenarObjetivosRequestValidator()
    {
        RuleFor(x => x.Ids).NotEmpty().WithMessage("Debe indicar el orden de los objetivos.")
            .Must(ids => ids.Distinct().Count() == ids.Count).WithMessage("Hay objetivos repetidos.");
    }
}

public sealed class VincularIntegranteRequestValidator : AbstractValidator<VincularIntegranteRequest>
{
    public VincularIntegranteRequestValidator()
    {
        RuleFor(x => x.UsuarioId).GreaterThan(0).WithMessage("Debe seleccionar un usuario.");
        RuleFor(x => x.RolEnPrograma)
            .Must(r => r is RolesPrograma.Coordinador or RolesPrograma.Investigador)
            .WithMessage("El rol en el programa debe ser Coordinador o Investigador.");
        RuleFor(x => x.FechaInicio).NotNull().WithMessage("La fecha de inicio es obligatoria.");
    }
}

public sealed class AsignarAliadosProgramaRequestValidator : AbstractValidator<AsignarAliadosProgramaRequest>
{
    public AsignarAliadosProgramaRequestValidator()
    {
        RuleFor(x => x.AliadoIds).NotNull();
    }
}
