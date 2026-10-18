using Ci2dt2.Application.Common;
using Ci2dt2.Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Ci2dt2.Application.Aliados;

public sealed record AliadoRequest(
    int TipoAliadoId,
    string Nombre,
    string? Nit,
    string Pais,
    string? Ciudad,
    string? Sector,
    string? RepresentanteLegal,
    string Email);

public static class ReglasAliado
{
    public const string PaisColombia = "Colombia";

    public static bool EsColombiano(string? pais) =>
        string.Equals(pais?.Trim(), PaisColombia, StringComparison.OrdinalIgnoreCase);

    /// <summary>NIT colombiano con dígito de verificación (formato 890800128-6), algoritmo de la DIAN.</summary>
    public static bool NitValido(string? nit)
    {
        if (string.IsNullOrWhiteSpace(nit))
            return false;
        var partes = nit.Trim().Split('-');
        if (partes.Length != 2 || partes[1].Length != 1 || !partes[0].All(char.IsAsciiDigit) || !char.IsAsciiDigit(partes[1][0])
            || partes[0].Length is < 6 or > 15)
            return false;
        return DigitoVerificacion(partes[0]) == partes[1][0] - '0';
    }

    public static int DigitoVerificacion(string numero)
    {
        int[] pesos = [3, 7, 13, 17, 19, 23, 29, 37, 41, 43, 47, 53, 59, 67, 71];
        var suma = 0;
        for (var i = 0; i < numero.Length; i++)
            suma += (numero[numero.Length - 1 - i] - '0') * pesos[i];
        var residuo = suma % 11;
        return residuo > 1 ? 11 - residuo : residuo;
    }
}

public sealed class AliadoRequestValidator : AbstractValidator<AliadoRequest>
{
    public AliadoRequestValidator()
    {
        RuleFor(x => x.TipoAliadoId).GreaterThan(0).WithMessage("Debe seleccionar el tipo de aliado.");
        RuleFor(x => x.Nombre).NotEmpty().WithMessage("El nombre es obligatorio.").MaximumLength(200);
        RuleFor(x => x.Pais).NotEmpty().WithMessage("El país es obligatorio.").MaximumLength(60);
        RuleFor(x => x.Email).NotEmpty().WithMessage("El correo es obligatorio.")
            .EmailAddress().WithMessage("El correo no tiene un formato válido.").MaximumLength(150);
        RuleFor(x => x.Nit).MaximumLength(30);
        RuleFor(x => x.Nit)
            .Must(ReglasAliado.NitValido)
            .WithMessage("El NIT es obligatorio para aliados colombianos y debe tener dígito de verificación válido (ej. 890800128-6).")
            .When(x => ReglasAliado.EsColombiano(x.Pais));
        RuleFor(x => x.Ciudad).MaximumLength(80);
        RuleFor(x => x.Sector).MaximumLength(80);
        RuleFor(x => x.RepresentanteLegal).MaximumLength(150);
    }
}

/// <summary>Comandos del catálogo de aliados (HU-07).</summary>
public sealed class AliadosService(IApplicationDbContext db)
{
    public async Task<int> CrearAsync(AliadoRequest request, CancellationToken ct = default)
    {
        await ValidarAsync(request, null, ct);
        var aliado = new Aliado();
        Aplicar(aliado, request);
        db.Aliados.Add(aliado);
        await db.SaveChangesAsync(ct);
        return aliado.Id;
    }

    public async Task ActualizarAsync(int id, AliadoRequest request, CancellationToken ct = default)
    {
        var aliado = await CargarAsync(id, ct);
        await ValidarAsync(request, id, ct);
        Aplicar(aliado, request);
        await db.SaveChangesAsync(ct);
    }

    /// <summary>Borrado lógico (regla general 5 del diccionario).</summary>
    public async Task EliminarAsync(int id, CancellationToken ct = default)
    {
        var aliado = await CargarAsync(id, ct);
        aliado.Activo = false;
        await db.SaveChangesAsync(ct);
    }

    private async Task ValidarAsync(AliadoRequest request, int? excluirId, CancellationToken ct)
    {
        if (!await db.TiposAliado.AnyAsync(t => t.Id == request.TipoAliadoId && t.Activo, ct))
            throw new ReglaNegocioException("El tipo de aliado no existe.");

        var nit = Limpiar(request.Nit);
        if (nit is null)
            return;
        // HU-07 CA-1: un NIT ya registrado impide la creación y se muestra el aliado existente.
        var existente = await db.Aliados
            .Where(a => a.Nit == nit && a.Activo && a.Id != excluirId)
            .Select(a => new { a.Id, a.Nombre })
            .FirstOrDefaultAsync(ct);
        if (existente is not null)
            throw new ReglaNegocioException($"Ya existe un aliado con este NIT: {existente.Nombre} (Id {existente.Id}).");
    }

    private async Task<Aliado> CargarAsync(int id, CancellationToken ct) =>
        await db.Aliados.FirstOrDefaultAsync(a => a.Id == id && a.Activo, ct)
        ?? throw new NoEncontradoException("El aliado no existe.");

    private static void Aplicar(Aliado aliado, AliadoRequest r)
    {
        aliado.TipoAliadoId = r.TipoAliadoId;
        aliado.Nombre = r.Nombre.Trim();
        aliado.Nit = Limpiar(r.Nit);
        aliado.Pais = r.Pais.Trim();
        aliado.Ciudad = Limpiar(r.Ciudad);
        aliado.Sector = Limpiar(r.Sector);
        aliado.RepresentanteLegal = Limpiar(r.RepresentanteLegal);
        aliado.Email = r.Email.Trim();
    }

    private static string? Limpiar(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
