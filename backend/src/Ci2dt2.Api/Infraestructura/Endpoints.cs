using System.Reflection;
using System.Text.Json.Serialization;
using FluentValidation;

namespace Ci2dt2.Api.Infraestructura;

/// <summary>
/// Cada módulo registra sus rutas en una clase {Modulo}Endpoints con un método estático Map.
/// Program.cs no se modifica para agregar endpoints: los módulos se descubren por reflexión.
/// </summary>
public interface IEndpointModule
{
    static abstract void Map(IEndpointRouteBuilder app);
}

public static class EndpointExtensions
{
    public static IEndpointRouteBuilder MapModulos(this IEndpointRouteBuilder app)
    {
        var modulos = typeof(IEndpointModule).Assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false } && t.IsAssignableTo(typeof(IEndpointModule)));
        foreach (var modulo in modulos)
            modulo.GetMethod(nameof(IEndpointModule.Map), BindingFlags.Public | BindingFlags.Static)!.Invoke(null, [app]);
        return app;
    }

    /// <summary>Valida el cuerpo de la petición con FluentValidation antes de ejecutar el endpoint.</summary>
    public static RouteHandlerBuilder Validar<T>(this RouteHandlerBuilder builder) =>
        builder.AddEndpointFilter<FiltroValidacion<T>>().ProducesValidationProblem();
}

public sealed class FiltroValidacion<T>(IValidator<T> validador) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var modelo = context.Arguments.OfType<T>().FirstOrDefault();
        if (modelo is null)
            return Results.BadRequest(new ErrorApi("El cuerpo de la petición es obligatorio."));

        var resultado = await validador.ValidateAsync(modelo, context.HttpContext.RequestAborted);
        if (!resultado.IsValid)
        {
            var errores = resultado.Errors
                .GroupBy(e => JsonNamingPolicyCamel(e.PropertyName))
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).Distinct().ToArray());
            return Results.BadRequest(new ErrorApi("Hay datos inválidos en la solicitud.", errores));
        }

        return await next(context);
    }

    private static string JsonNamingPolicyCamel(string nombre) =>
        System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(nombre);
}

/// <summary>Formato estándar de error de la API.</summary>
public sealed record ErrorApi(
    string Error,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IDictionary<string, string[]>? Errores = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? SegundosRestantes = null);
