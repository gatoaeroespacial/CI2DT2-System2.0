using Ci2dt2.Application.Seguridad;
using Ci2dt2.Application.Usuarios;
using Microsoft.Extensions.Caching.Memory;

namespace Ci2dt2.UnitTests;

internal sealed class RelojFalso(DateTimeOffset inicio) : TimeProvider
{
    public DateTimeOffset Ahora { get; set; } = inicio;
    public override DateTimeOffset GetUtcNow() => Ahora;
    public void Avanzar(TimeSpan tiempo) => Ahora += tiempo;
}

public class ControlIntentosLoginTests
{
    private readonly RelojFalso _reloj = new(new DateTimeOffset(2026, 10, 7, 12, 0, 0, TimeSpan.Zero));
    private readonly ControlIntentosLogin _control;

    public ControlIntentosLoginTests() =>
        _control = new ControlIntentosLogin(new MemoryCache(new MemoryCacheOptions()), _reloj);

    [Fact]
    public void Cuatro_fallos_no_bloquean_la_cuenta()
    {
        for (var i = 0; i < 4; i++) _control.RegistrarFallo("a@b.co");
        Assert.Null(_control.TiempoRestanteBloqueo("a@b.co"));
    }

    [Fact]
    public void Cinco_fallos_consecutivos_bloquean_15_minutos()
    {
        for (var i = 0; i < 5; i++) _control.RegistrarFallo("a@b.co");

        Assert.Equal(TimeSpan.FromMinutes(15), _control.TiempoRestanteBloqueo("A@B.CO "));
        _reloj.Avanzar(TimeSpan.FromMinutes(10));
        Assert.Equal(TimeSpan.FromMinutes(5), _control.TiempoRestanteBloqueo("a@b.co"));
        _reloj.Avanzar(TimeSpan.FromMinutes(5));
        Assert.Null(_control.TiempoRestanteBloqueo("a@b.co"));
    }

    [Fact]
    public void Los_fallos_fuera_de_la_ventana_de_15_minutos_no_se_acumulan()
    {
        for (var i = 0; i < 4; i++) _control.RegistrarFallo("a@b.co");
        _reloj.Avanzar(TimeSpan.FromMinutes(16));
        _control.RegistrarFallo("a@b.co");
        Assert.Null(_control.TiempoRestanteBloqueo("a@b.co"));
    }

    [Fact]
    public void Un_inicio_exitoso_reinicia_el_contador()
    {
        for (var i = 0; i < 4; i++) _control.RegistrarFallo("a@b.co");
        _control.Reiniciar("a@b.co");
        _control.RegistrarFallo("a@b.co");
        Assert.Null(_control.TiempoRestanteBloqueo("a@b.co"));
    }

    [Fact]
    public void Tras_el_bloqueo_el_contador_empieza_de_nuevo()
    {
        for (var i = 0; i < 5; i++) _control.RegistrarFallo("a@b.co");
        _reloj.Avanzar(TimeSpan.FromMinutes(15));
        _control.RegistrarFallo("a@b.co");
        Assert.Null(_control.TiempoRestanteBloqueo("a@b.co"));
    }
}

public class CriptografiaTests
{
    [Fact]
    public void El_token_de_refresco_es_aleatorio_y_se_guarda_como_hash_sha256()
    {
        var a = Criptografia.GenerarTokenRefresco();
        var b = Criptografia.GenerarTokenRefresco();

        Assert.NotEqual(a, b);
        Assert.True(a.Length >= 80);
        Assert.Equal(64, Criptografia.Sha256Hex(a).Length);
        Assert.Equal(Criptografia.Sha256Hex(a), Criptografia.Sha256Hex(a));
    }
}

public class PoliticaContrasenaTests
{
    private readonly AsignarContrasenaRequestValidator _validador = new();

    [Theory]
    [InlineData("Corta1A")]          // menos de 10 caracteres
    [InlineData("sinmayuscula1")]
    [InlineData("SINMINUSCULA1")]
    [InlineData("SinNumeroAqui")]
    [InlineData("")]
    public void Rechaza_contrasenas_que_no_cumplen_la_politica(string contrasena) =>
        Assert.False(_validador.Validate(new AsignarContrasenaRequest(contrasena)).IsValid);

    [Fact]
    public void Acepta_una_contrasena_valida() =>
        Assert.True(_validador.Validate(new AsignarContrasenaRequest("Valida12345")).IsValid);
}

/// <summary>Verifica la matriz de permisos contra el Anexo A del backlog (celda por celda).</summary>
public class MatrizPermisosTests
{
    private const string A = "Admin", D = "Director", C = "Coordinador", I = "Investigador", E = "Externo";

    public static TheoryData<string, string[]> AnexoA => new()
    {
        { Politicas.GestionarUsuarios, [A] },
        { Politicas.EditarProgramas, [A, D] },
        { Politicas.AsignarCoordinadores, [A, D] },
        { Politicas.VincularInvestigadores, [A, D, C] },
        { Politicas.GestionarAliadosConvocatorias, [A, D, C] },
        { Politicas.CrearPropuestas, [A, D, C, I] },
        { Politicas.EditarPropuestas, [A, D, C, I] },
        { Politicas.RevisarPropuestas, [D, C] },
        { Politicas.AvalarAprobarProyectos, [A, D] },
        { Politicas.VerProyectos, [A, D, C, I] },
        { Politicas.GestionarHitos, [A, D, C, I] },
        { Politicas.RegistrarAvances, [A, D, C, I] },
        { Politicas.ValidarAvances, [D, C, I] },
        { Politicas.GestionarDocumentos, [A, D, C, I] },
        { Politicas.VerTableroCentro, [A, D] },
        { Politicas.VerNotificaciones, [A, D, C, I, E] },
    };

    [Theory]
    [MemberData(nameof(AnexoA))]
    public void Cada_accion_tiene_exactamente_los_roles_del_anexo_A(string politica, string[] roles) =>
        Assert.Equal(roles.Order(), Politicas.Matriz[politica].Order());

    [Fact]
    public void La_matriz_tiene_las_16_acciones_del_anexo_A() =>
        Assert.Equal(16, Politicas.Matriz.Count);

    [Fact]
    public void El_rol_Externo_solo_ve_sus_notificaciones() =>
        Assert.Equal([Politicas.VerNotificaciones], Politicas.Matriz.Where(p => p.Value.Contains(E)).Select(p => p.Key));
}
