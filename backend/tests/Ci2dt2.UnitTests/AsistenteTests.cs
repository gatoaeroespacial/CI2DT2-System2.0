using Ci2dt2.Application.Seguridad;
using Ci2dt2.Application.Asistente;
using Ci2dt2.Domain;

namespace Ci2dt2.UnitTests;

public class AsistentePromptTests
{
    [Fact]
    public void El_prompt_no_expone_esquema_a_roles_sin_acceso_al_asistente()
    {
        var prompt = AsistenteConsultas.CrearInstrucciones([Roles.Investigador]);

        Assert.DoesNotContain("- Proyectos:", prompt);
        Assert.DoesNotContain("- Usuarios:", prompt);
        Assert.Contains("Este rol no tiene acceso al asistente", prompt);
    }

    [Fact]
    public void El_prompt_de_administrador_permite_consultar_el_centro_sin_ampliar_el_esquema()
    {
        var prompt = AsistenteConsultas.CrearInstrucciones([Roles.Administrador]);

        Assert.Contains("- Proyectos:", prompt);
        Assert.Contains("- Indicadores:", prompt);
        Assert.Contains("- Convocatorias:", prompt);
        Assert.Contains("- Usuarios:", prompt);
        Assert.Contains("Email", prompt);
        Assert.Contains("- ProyectoIntegrantes:", prompt);
        Assert.DoesNotContain("- RefreshTokens:", prompt);
        Assert.DoesNotContain("PasswordHash", prompt);
        Assert.DoesNotContain("TokenHash", prompt);
        Assert.DoesNotContain("- LogActividades:", prompt);
    }

    [Fact]
    public void El_prompt_indica_denegar_sin_reemplazar_la_consulta_por_otra_entidad()
    {
        var prompt = AsistenteConsultas.CrearInstrucciones([Roles.Director]);

        Assert.Contains("\"count|list|deny|respond\"", prompt);
        Assert.Contains("Nunca reemplaces una solicitud denegada o no autorizada con una consulta sobre otra entidad", prompt);
        Assert.Contains("Este JSON es interno", prompt);
        Assert.Contains("Las respuestas visibles deben ser en español cotidiano", prompt);
        Assert.Contains("estas consultas requieren permisos de Administrador", prompt);
        Assert.DoesNotContain("- Usuarios:", prompt);
        Assert.DoesNotContain("- ProgramaIntegrantes:", prompt);
        Assert.DoesNotContain("- ProyectoIntegrantes:", prompt);
        Assert.Contains("Usa \"respond\" para saludos, conversación general, preguntas explicativas", prompt);
        Assert.Contains("explica brevemente cuando una pregunta quede fuera del alcance", prompt);
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
    public void La_solicitud_de_investigadores_se_deniega_sin_desviarse_para_director()
    {
        var mensaje = AsistenteConsultas.DetectarSolicitudProtegida(
            "Muéstrame la lista de investigadores",
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
