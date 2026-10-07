using FluentValidation;

namespace Ci2dt2.Application.Seguridad;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().WithMessage("El correo es obligatorio.")
            .EmailAddress().WithMessage("El correo no tiene un formato válido.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("La contraseña es obligatoria.");
    }
}

public sealed class DefinirContrasenaRequestValidator : AbstractValidator<DefinirContrasenaRequest>
{
    public DefinirContrasenaRequestValidator()
    {
        RuleFor(x => x.Token).NotEmpty().WithMessage("El enlace no es válido.");
        RuleFor(x => x.NuevaContrasena).CumplePoliticaContrasena();
    }
}

public static class PoliticaContrasena
{
    public const int LongitudMinima = 10;

    /// <summary>Mínimo 10 caracteres, con al menos una mayúscula, una minúscula y un número.</summary>
    public static IRuleBuilderOptions<T, string> CumplePoliticaContrasena<T>(this IRuleBuilderInitial<T, string> regla) =>
        regla
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MinimumLength(LongitudMinima).WithMessage($"La contraseña debe tener al menos {LongitudMinima} caracteres.")
            .Must(p => p.Any(char.IsUpper)).WithMessage("La contraseña debe tener al menos una mayúscula.")
            .Must(p => p.Any(char.IsLower)).WithMessage("La contraseña debe tener al menos una minúscula.")
            .Must(p => p.Any(char.IsDigit)).WithMessage("La contraseña debe tener al menos un número.");
}
