using Ci2dt2.Domain;
using FluentValidation;

namespace Ci2dt2.Application.Usuarios;

public static class ReglasUsuario
{
    public static readonly string[] TiposDocumento = ["CC", "CE", "TI", "PA", "PEP"];

    /// <summary>Formato ORCID 0000-0000-0000-000X.</summary>
    public const string PatronOrcid = @"^\d{4}-\d{4}-\d{4}-\d{3}[\dX]$";

    public const string DominioCvLac = "scienti.minciencias.gov.co";

    public static bool EsUrlCvLac(string? url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp)
        && uri.Host.Equals(DominioCvLac, StringComparison.OrdinalIgnoreCase);
}

/// <summary>Reglas compartidas por la creación y la edición de usuarios.</summary>
public abstract class DatosUsuarioValidator<T> : AbstractValidator<T> where T : IDatosUsuario
{
    protected DatosUsuarioValidator()
    {
        RuleFor(x => x.Nombres).NotEmpty().WithMessage("Los nombres son obligatorios.").MaximumLength(100);
        RuleFor(x => x.Apellidos).NotEmpty().WithMessage("Los apellidos son obligatorios.").MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().WithMessage("El correo es obligatorio.")
            .EmailAddress().WithMessage("El correo no tiene un formato válido.")
            .MaximumLength(150);
        RuleFor(x => x.TipoDocumento!)
            .Must(t => ReglasUsuario.TiposDocumento.Contains(t))
            .WithMessage("El tipo de documento no es válido.")
            .When(x => !string.IsNullOrEmpty(x.TipoDocumento));
        RuleFor(x => x.NumeroDocumento).MaximumLength(30);
        RuleFor(x => x.Telefono).MaximumLength(30);
        RuleFor(x => x.Cargo).MaximumLength(100);
        RuleFor(x => x.Dependencia).MaximumLength(150);
        RuleFor(x => x.Orcid!)
            .Matches(ReglasUsuario.PatronOrcid)
            .WithMessage("El ORCID debe tener el formato 0000-0000-0000-000X.")
            .When(x => !string.IsNullOrEmpty(x.Orcid));
        RuleFor(x => x.CvLacUrl!)
            .MaximumLength(300)
            .Must(ReglasUsuario.EsUrlCvLac)
            .WithMessage($"El CvLAC debe ser una URL del dominio {ReglasUsuario.DominioCvLac}.")
            .When(x => !string.IsNullOrEmpty(x.CvLacUrl));
    }
}

public sealed class CrearUsuarioRequestValidator : DatosUsuarioValidator<CrearUsuarioRequest>
{
    public CrearUsuarioRequestValidator()
    {
        RuleFor(x => x.Roles).ValidarRoles();
        RuleFor(x => x.AliadoId)
            .NotNull().WithMessage("Un usuario Externo debe estar asociado a un aliado.")
            .When(x => x.Roles?.Contains(Roles.Externo) == true);
    }
}

public sealed class ActualizarUsuarioRequestValidator : DatosUsuarioValidator<ActualizarUsuarioRequest>;

public sealed class CambiarEstadoUsuarioRequestValidator : AbstractValidator<CambiarEstadoUsuarioRequest>
{
    public CambiarEstadoUsuarioRequestValidator()
    {
        RuleFor(x => x.TieneAcceso).NotNull().WithMessage("Debe indicar si el usuario queda activo o inactivo.");
    }
}

public sealed class AsignarRolesRequestValidator : AbstractValidator<AsignarRolesRequest>
{
    public AsignarRolesRequestValidator()
    {
        RuleFor(x => x.Roles).ValidarRoles();
    }
}

internal static class ReglasRoles
{
    public static IRuleBuilderOptions<T, IReadOnlyList<string>> ValidarRoles<T>(this IRuleBuilderInitial<T, IReadOnlyList<string>> regla) =>
        regla
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Debe asignar al menos un rol.")
            .Must(roles => roles.All(r => Roles.Todos.Contains(r)))
            .WithMessage("Hay roles no válidos.");
}
