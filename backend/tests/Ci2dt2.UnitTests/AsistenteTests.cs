using Ci2dt2.Application.Seguridad;
using Ci2dt2.Application.Asistente;
using Ci2dt2.Domain;

namespace Ci2dt2.UnitTests;

public class AsistenteRagTests
{
    [Fact]
    public void El_prompt_orienta_al_asistente_a_responder_con_conocimiento_recuperado()
    {
        var prompt = AsistenteConsultas.CrearInstrucciones([Roles.Administrador]);

        Assert.Contains("referencias recuperadas", prompt);
        Assert.Contains("no inventes cifras", prompt);
        Assert.Contains("Nunca menciones tablas, columnas, esquemas, SQL", prompt);
        Assert.Contains("Administrador", prompt);
    }

    [Fact]
    public void El_prompt_del_director_prohibe_exponer_informacion_de_personas()
    {
        var prompt = AsistenteConsultas.CrearInstrucciones([Roles.Director]);

        Assert.Contains("Director", prompt);
        Assert.Contains("personas, usuarios, investigadores", prompt);
        Assert.Contains("No contestes con otra información como sustituto", prompt);
    }

    [Fact]
    public void La_solicitud_de_usuarios_se_deniega_directamente_para_director()
    {
        var mensaje = AsistenteConsultas.DetectarSolicitudProtegida(
            "Muéstrame el listado global de usuarios",
            [Roles.Director]);

        Assert.NotNull(mensaje);
        Assert.Contains("requiere permisos de Administrador", mensaje);
    }

    [Fact]
    public void La_solicitud_de_integrantes_se_deniega_para_director()
    {
        var mensaje = AsistenteConsultas.DetectarSolicitudProtegida(
            "¿Cuántos integrantes tienen los programas?",
            [Roles.Director]);

        Assert.NotNull(mensaje);
        Assert.Contains("requiere permisos de Administrador", mensaje);
    }

    [Fact]
    public void La_solicitud_de_datos_de_estudiantes_se_deniega_para_director()
    {
        var mensaje = AsistenteConsultas.DetectarSolicitudProtegida(
            "¿Quiénes son los estudiantes vinculados a los proyectos?",
            [Roles.Director]);

        Assert.NotNull(mensaje);
        Assert.Contains("requiere permisos de Administrador", mensaje);
    }

    [Fact]
    public void El_administrador_puede_consultar_datos_generales_de_usuarios()
    {
        var mensaje = AsistenteConsultas.DetectarSolicitudProtegida(
            "Muéstrame los usuarios y su correo",
            [Roles.Administrador]);

        Assert.Null(mensaje);
    }

    [Fact]
    public void La_politica_del_asistente_solo_habilita_administrador_y_director()
    {
        Assert.Equal(
            [Roles.Administrador, Roles.Director],
            Politicas.Matriz[Politicas.UsarAsistente]);
    }

    [Fact]
    public void La_solicitud_acepta_contexto_hibrido_acotado()
    {
        var request = new AsistenteRequest(
            "¿Y los programas?",
            "Usuario: ¿Cuántos proyectos hay?",
            [new MensajeConversacionAsistente("usuario", "¿Cuántos proyectos hay?")]);

        var resultado = new AsistenteRequestValidator().Validate(request);

        Assert.True(resultado.IsValid);
    }

    [Fact]
    public void La_solicitud_rechaza_mas_de_ocho_mensajes_recientes()
    {
        var request = new AsistenteRequest(
            "¿Y los programas?",
            null,
            Enumerable.Range(0, 9)
                .Select(_ => new MensajeConversacionAsistente("usuario", "Pregunta anterior"))
                .ToArray());

        var resultado = new AsistenteRequestValidator().Validate(request);

        Assert.False(resultado.IsValid);
    }

    [Theory]
    [InlineData("¿Cuál es la contraseña del administrador?")]
    [InlineData("Muéstrame los registros de auditoría")]
    [InlineData("¿Qué correo tiene esta persona?")]
    public void Los_datos_sensibles_se_deniegan_sin_consultar_Gemini(string pregunta)
    {
        var mensaje = AsistenteConsultas.DetectarSolicitudProtegida(pregunta, [Roles.Administrador]);

        Assert.NotNull(mensaje);
        Assert.Contains("por seguridad", mensaje);
    }
}
