using Ci2dt2.Application.Asistente;
using Ci2dt2.Domain;
using Ci2dt2.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Ci2dt2.IntegrationTests;

[Collection(ColeccionApi.Nombre)]
public sealed class AsistenteRagTests(ApiFactory api)
{
    [Fact]
    public async Task Recupera_conocimiento_actual_y_filtra_integrantes_segun_el_rol()
    {
        var sufijo = Guid.NewGuid().ToString("N")[..10];
        var nombrePrograma = $"RagNogal{sufijo}";
        var textoIntegrante = $"MiembroRag{sufijo}";
        var usuario = await api.CrearUsuarioAsync(ApiFactory.Unico("rag"), roles: [Roles.Investigador]);

        await api.EnBaseDatosAsync(async db =>
        {
            var programa = new Programa
            {
                Nombre = nombrePrograma,
                Descripcion = "Investigación sobre conservación de bosques.",
            };
            db.Programas.Add(programa);
            await db.SaveChangesAsync();
            db.ProgramaIntegrantes.Add(new ProgramaIntegrante
            {
                ProgramaId = programa.Id,
                UsuarioId = usuario.Id,
                RolEnPrograma = textoIntegrante,
            });
            await db.SaveChangesAsync();
            return true;
        });

        using var scope = api.Services.CreateScope();
        var recuperador = scope.ServiceProvider.GetRequiredService<IRecuperadorConocimientoAsistente>();
        var consultaCentro = await recuperador.RecuperarAsync(nombrePrograma, [Roles.Director], default);
        var consultaPrivada = await recuperador.RecuperarAsync(textoIntegrante, [Roles.Director], default);
        var consultaAdministrador = await recuperador.RecuperarAsync(textoIntegrante, [Roles.Administrador], default);
        var consultaDatosPersonales = await recuperador.RecuperarAsync(usuario.Email, [Roles.Administrador], default);

        Assert.Contains(consultaCentro, fragmento => fragmento.Contenido.Contains(nombrePrograma, StringComparison.Ordinal));
        Assert.Empty(consultaPrivada);
        Assert.Contains(consultaAdministrador, fragmento => fragmento.Contenido.Contains(textoIntegrante, StringComparison.Ordinal));
        Assert.Empty(consultaDatosPersonales);
    }
}
