using Ci2dt2.Application.Aliados;
using Ci2dt2.Application.Programas;
using Ci2dt2.Application.Proyectos;

namespace Ci2dt2.UnitTests;

/// <summary>HU-07: NIT con dígito de verificación (algoritmo de la DIAN).</summary>
public class AliadosReglasTests
{
    private readonly AliadoRequestValidator _validador = new();

    private static AliadoRequest Aliado(string? nit, string pais = "Colombia") =>
        new(2, "CHEC", nit, pais, "Manizales", "Energía", null, "contacto@chec.test");

    [Theory]
    [InlineData("890800128-6")]
    [InlineData("890805051-0")]
    [InlineData("860007386-1")]
    public void Acepta_nit_colombiano_con_digito_de_verificacion_correcto(string nit) =>
        Assert.True(_validador.Validate(Aliado(nit)).IsValid);

    [Theory]
    [InlineData("890800128-5")]
    [InlineData("890800128")]
    [InlineData("89080012A-6")]
    [InlineData(null)]
    public void Rechaza_nit_colombiano_ausente_o_con_digito_incorrecto(string? nit) =>
        Assert.False(_validador.Validate(Aliado(nit)).IsValid);

    [Fact]
    public void Para_aliados_extranjeros_el_nit_es_opcional_y_el_pais_obligatorio()
    {
        Assert.True(_validador.Validate(Aliado(null, "Suiza")).IsValid);
        Assert.False(_validador.Validate(Aliado(null, "")).IsValid);
    }

    [Fact]
    public void El_correo_es_obligatorio() =>
        Assert.False(_validador.Validate(Aliado("890800128-6") with { Email = "" }).IsValid);
}

/// <summary>HU-08 y HU-10: reglas de la propuesta y de las fuentes de financiamiento.</summary>
public class ProyectosReglasTests
{
    private readonly CrearProyectoRequestValidator _proyecto = new();
    private readonly FuenteRequestValidator _fuente = new();

    private static CrearProyectoRequest Propuesta() =>
        new("Bioinsumos a partir de pulpa de café", null, 1, 1, null, null, null, null, null, null);

    [Fact]
    public void Diez_mil_usd_a_tasa_cuatro_mil_son_cuarenta_millones_de_pesos() =>
        Assert.Equal(40_000_000m, ValoresProyecto.EnCop(10_000m, "USD", 4_000m));

    [Fact]
    public void En_cop_el_monto_no_se_convierte() =>
        Assert.Equal(1_500_000m, ValoresProyecto.EnCop(1_500_000m, "COP", null));

    [Fact]
    public void Un_borrador_solo_exige_los_campos_obligatorios() =>
        Assert.True(_proyecto.Validate(Propuesta()).IsValid);

    [Theory]
    [InlineData("Corto")]
    [InlineData("")]
    public void El_titulo_tiene_entre_10_y_300_caracteres(string titulo) =>
        Assert.False(_proyecto.Validate(Propuesta() with { Titulo = titulo }).IsValid);

    [Fact]
    public void El_resumen_admite_maximo_4000_caracteres() =>
        Assert.False(_proyecto.Validate(Propuesta() with { Resumen = new string('a', 4001) }).IsValid);

    [Fact]
    public void La_fecha_de_fin_es_posterior_a_la_de_inicio()
    {
        var inicio = new DateOnly(2026, 11, 1);
        Assert.False(_proyecto.Validate(Propuesta() with { FechaInicio = inicio, FechaFin = inicio }).IsValid);
        Assert.True(_proyecto.Validate(Propuesta() with { FechaInicio = inicio, FechaFin = inicio.AddDays(1) }).IsValid);
    }

    [Fact]
    public void Si_la_moneda_no_es_cop_la_tasa_de_cambio_es_obligatoria_y_positiva()
    {
        Assert.False(_proyecto.Validate(Propuesta() with { Moneda = "USD", PresupuestoTotal = 10_000 }).IsValid);
        Assert.False(_proyecto.Validate(Propuesta() with { Moneda = "USD", PresupuestoTotal = 10_000, TasaCambio = 0 }).IsValid);
        Assert.True(_proyecto.Validate(Propuesta() with { Moneda = "USD", PresupuestoTotal = 10_000, TasaCambio = 4_000 }).IsValid);
        Assert.False(_proyecto.Validate(Propuesta() with { Moneda = "GBP", PresupuestoTotal = 10 }).IsValid);
    }

    [Theory]
    [InlineData("Convocatoria", "Efectivo", "Estimado", true)]
    [InlineData("Contrapartida", "Especie", "Comprometido", true)]
    [InlineData("Recursos propios", "Efectivo", "Desembolsado", true)]
    [InlineData("Donación", "Efectivo", "Estimado", false)]
    [InlineData("Convocatoria", "Crédito", "Estimado", false)]
    [InlineData("Convocatoria", "Efectivo", "Aprobado", false)]
    public void Fuentes_con_los_valores_del_backlog(string tipoFuente, string tipoAporte, string estado, bool valida) =>
        Assert.Equal(valida, _fuente.Validate(new FuenteRequest(null, tipoFuente, tipoAporte, "COP", 1000, null, estado)).IsValid);

    [Fact]
    public void Una_fuente_en_moneda_extranjera_exige_tasa() =>
        Assert.False(_fuente.Validate(new FuenteRequest(null, "Convocatoria", "Efectivo", "EUR", 1000, null, "Estimado")).IsValid);

    [Theory]
    [InlineData("IP", true)]
    [InlineData("Co-investigador", true)]
    [InlineData("Auxiliar de campo", true)]
    [InlineData("Estudiante", true)]
    [InlineData("Asesor externo", false)]
    public void Roles_del_equipo_segun_el_backlog(string rol, bool valido) =>
        Assert.Equal(valido, new AgregarIntegranteRequestValidator().Validate(new AgregarIntegranteRequest(5, rol, null, null)).IsValid);
}

/// <summary>HU-06: validaciones de la vinculación al programa.</summary>
public class ProgramasReglasTests
{
    [Theory]
    [InlineData("Coordinador", true)]
    [InlineData("Investigador", true)]
    [InlineData("Director", false)]
    public void El_rol_en_el_programa_es_coordinador_o_investigador(string rol, bool valido) =>
        Assert.Equal(valido, new VincularIntegranteRequestValidator()
            .Validate(new VincularIntegranteRequest(3, rol, new DateOnly(2026, 1, 1), null)).IsValid);
}
