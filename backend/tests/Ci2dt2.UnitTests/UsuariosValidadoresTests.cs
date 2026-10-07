using Ci2dt2.Application.Usuarios;

namespace Ci2dt2.UnitTests;

public class UsuariosValidadoresTests
{
    private readonly CrearUsuarioRequestValidator _validador = new();

    private static CrearUsuarioRequest Valido(
        string? orcid = null, string? cvlac = null, int? aliadoId = null, string email = "ana@ucaldas.edu.co", params string[] roles) =>
        new("Ana", "Pérez", "CC", "123", email, null, null, null, cvlac, orcid, aliadoId,
            roles.Length == 0 ? ["Investigador"] : roles);

    private bool EsValido(CrearUsuarioRequest r) => _validador.Validate(r).IsValid;

    [Theory]
    [InlineData("0000-0002-1825-0097")]
    [InlineData("0000-0002-1694-233X")]
    public void Acepta_orcid_con_formato_valido(string orcid) => Assert.True(EsValido(Valido(orcid: orcid)));

    [Theory]
    [InlineData("0000-0002-1825-009")]
    [InlineData("0000000218250097")]
    [InlineData("0000-0002-1825-009x")]
    [InlineData("https://orcid.org/0000-0002-1825-0097")]
    public void Rechaza_orcid_con_formato_invalido(string orcid) => Assert.False(EsValido(Valido(orcid: orcid)));

    [Theory]
    [InlineData("https://scienti.minciencias.gov.co/cvlac/visualizador/generarCurriculoCv.do?cod_rh=0000123")]
    [InlineData("http://scienti.minciencias.gov.co/cvlac/")]
    public void Acepta_cvlac_del_dominio_de_minciencias(string url) => Assert.True(EsValido(Valido(cvlac: url)));

    [Theory]
    [InlineData("https://otro.dominio.com/cvlac")]
    [InlineData("https://scienti.minciencias.gov.co.malicioso.com/cvlac")]
    [InlineData("scienti.minciencias.gov.co/cvlac")]
    [InlineData("ftp://scienti.minciencias.gov.co/cvlac")]
    public void Rechaza_cvlac_fuera_del_dominio(string url) => Assert.False(EsValido(Valido(cvlac: url)));

    [Fact]
    public void Un_usuario_externo_requiere_aliado()
    {
        Assert.False(EsValido(Valido(roles: "Externo")));
        Assert.True(EsValido(Valido(aliadoId: 3, roles: "Externo")));
    }

    [Fact]
    public void Exige_al_menos_un_rol_valido()
    {
        Assert.False(EsValido(Valido() with { Roles = [] }));
        Assert.False(EsValido(Valido(roles: "SuperUsuario")));
        Assert.True(EsValido(Valido(roles: ["Coordinador", "Investigador"])));
    }

    [Theory]
    [InlineData("")]
    [InlineData("no-es-correo")]
    public void Exige_un_correo_valido(string email) => Assert.False(EsValido(Valido(email: email)));

    [Fact]
    public void Rechaza_tipos_de_documento_desconocidos() =>
        Assert.False(EsValido(Valido() with { TipoDocumento = "XYZ" }));
}
