using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Ci2dt2.Application.Seguridad;
using Ci2dt2.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ci2dt2.IntegrationTests;

/// <summary>HU-05 Restringir el acceso según el rol y el programa.</summary>
[Collection(ColeccionApi.Nombre)]
public class PermisosTests(ApiFactory api)
{
    private const string A = "Admin", D = "Director", C = "Coordinador", I = "Investigador", E = "Externo";

    public static TheoryData<string, string> EndpointsUsuarios => new()
    {
        { "GET", "/api/v1/usuarios" },
        { "GET", "/api/v1/usuarios/999999" },
        { "POST", "/api/v1/usuarios" },
        { "PUT", "/api/v1/usuarios/999999" },
        { "PATCH", "/api/v1/usuarios/999999/estado" },
        { "PUT", "/api/v1/usuarios/999999/roles" },
        { "PUT", "/api/v1/usuarios/999999/contrasena" },
    };

    /// <summary>Anexo A, "Gestionar usuarios y roles": solo el Administrador.</summary>
    [Theory]
    [MemberData(nameof(EndpointsUsuarios))]
    public async Task Gestionar_usuarios_solo_lo_puede_hacer_el_administrador(string metodo, string ruta)
    {
        foreach (var rol in new[] { D, C, I, E })
        {
            var cliente = await api.ClienteComoAsync(rol);
            var respuesta = await cliente.SendAsync(Peticion(metodo, ruta));
            Assert.True(respuesta.StatusCode == HttpStatusCode.Forbidden, $"{rol} {metodo} {ruta} => {respuesta.StatusCode}");
        }

        var admin = await api.ClienteComoAsync(A);
        var respuestaAdmin = await admin.SendAsync(Peticion(metodo, ruta));
        Assert.NotEqual(HttpStatusCode.Forbidden, respuestaAdmin.StatusCode);
        Assert.NotEqual(HttpStatusCode.Unauthorized, respuestaAdmin.StatusCode);
        Assert.True((int)respuestaAdmin.StatusCode < 500, $"Administrador {metodo} {ruta} => {respuestaAdmin.StatusCode}");

        var anonimo = await api.CreateClient().SendAsync(Peticion(metodo, ruta));
        Assert.Equal(HttpStatusCode.Unauthorized, anonimo.StatusCode);
    }

    /// <summary>
    /// Anexo A, "Gestionar aliados y convocatorias": Administrador, Director y Coordinador. El
    /// Investigador solo consulta el catálogo (lo necesita para registrar cofinanciaciones, HU-10).
    /// </summary>
    [Theory]
    [InlineData(A, HttpStatusCode.OK, true)]
    [InlineData(D, HttpStatusCode.OK, true)]
    [InlineData(C, HttpStatusCode.OK, true)]
    [InlineData(I, HttpStatusCode.OK, false)]
    [InlineData(E, HttpStatusCode.Forbidden, false)]
    public async Task Consultar_y_gestionar_aliados_segun_el_rol(string rol, HttpStatusCode consulta, bool gestiona)
    {
        var cliente = await api.ClienteComoAsync(rol);
        Assert.Equal(consulta, (await cliente.GetAsync("/api/v1/aliados")).StatusCode);

        var crear = await cliente.PostAsJsonAsync("/api/v1/aliados", new
        {
            tipoAliadoId = 2,
            nombre = "Aliado " + rol,
            pais = "Suiza",
            email = "aliado@pruebas.local",
        });
        Assert.Equal(gestiona ? HttpStatusCode.Created : HttpStatusCode.Forbidden, crear.StatusCode);
    }

    [Fact]
    public async Task CA1_los_accesos_denegados_quedan_registrados_en_la_auditoria()
    {
        var email = ApiFactory.Unico("denegado");
        var usuario = await api.CrearUsuarioAsync(email, roles: [C]);
        var cliente = api.ClienteCon(await api.IniciarSesionAsync(email));

        var respuesta = await cliente.PutAsJsonAsync("/api/v1/usuarios/1", new { nombres = "x" });

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
        var registro = Assert.Single(await api.EsperarLogsAsync(l => l.UsuarioId == usuario.Id && l.EstadoHttp == 403));
        Assert.Equal("usuarios", registro.Recurso);
        Assert.Equal("PUT", registro.Accion);
        Assert.Equal("1", registro.RecursoId);
        Assert.Equal("PUT /api/v1/usuarios/1", registro.Ruta);
        Assert.Equal(ApiFactory.IpCliente, registro.IpOrigen);
    }

    [Fact]
    public async Task CA4_los_permisos_de_un_usuario_con_varios_roles_son_la_union()
    {
        var soloInvestigador = await api.ClienteComoAsync(I);
        var investigadorYAdmin = await api.ClienteComoAsync(I, A);

        Assert.Equal(HttpStatusCode.Forbidden, (await soloInvestigador.GetAsync("/api/v1/usuarios")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await investigadorYAdmin.GetAsync("/api/v1/usuarios")).StatusCode);
    }

    [Fact]
    public async Task Un_token_alterado_es_rechazado()
    {
        var cliente = api.CreateClient();
        cliente.DefaultRequestHeaders.Authorization = new("Bearer", "eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiIxIiwicm9sZSI6IkFkbWluaXN0cmFkb3IifQ.firma-falsa");
        Assert.Equal(HttpStatusCode.Unauthorized, (await cliente.GetAsync("/api/v1/usuarios")).StatusCode);
    }

    /// <summary>CA-5: todas las celdas de rol del Anexo A evaluadas con las políticas registradas en la API.</summary>
    [Fact]
    public async Task CA5_las_politicas_registradas_cumplen_todas_las_celdas_del_anexo_A()
    {
        var esperado = new Dictionary<string, string[]>
        {
            [Politicas.GestionarUsuarios] = [A],
            [Politicas.EditarProgramas] = [A, D],
            [Politicas.AsignarCoordinadores] = [A, D],
            [Politicas.VincularInvestigadores] = [A, D, C],
            [Politicas.GestionarAliadosConvocatorias] = [A, D, C],
            [Politicas.CrearPropuestas] = [A, D, C, I],
            [Politicas.EditarPropuestas] = [A, D, C, I],
            [Politicas.RevisarPropuestas] = [D, C],
            [Politicas.AvalarAprobarProyectos] = [A, D],
            [Politicas.VerProyectos] = [A, D, C, I],
            [Politicas.GestionarHitos] = [A, D, C, I],
            [Politicas.RegistrarAvances] = [A, D, C, I],
            [Politicas.ValidarAvances] = [D, C, I],
            [Politicas.GestionarDocumentos] = [A, D, C, I],
            [Politicas.VerTableroCentro] = [A, D],
            [Politicas.VerNotificaciones] = [A, D, C, I, E],
        };
        var autorizacion = api.Services.GetRequiredService<IAuthorizationService>();

        foreach (var (politica, permitidos) in esperado)
        {
            foreach (var rol in new[] { A, D, C, I, E })
            {
                var usuario = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", "1"), new Claim("role", rol)], "Bearer", "sub", "role"));
                var resultado = await autorizacion.AuthorizeAsync(usuario, null, politica);
                Assert.True(resultado.Succeeded == permitidos.Contains(rol), $"{politica} / {rol}");
            }
        }
    }

    /// <summary>Restricciones de alcance ("Su programa", "Sus proyectos", "Si es IP") verificadas contra la base de datos.</summary>
    [Fact]
    public async Task Alcance_por_programa_y_proyecto_se_valida_contra_la_base_de_datos()
    {
        var coordinadorA = await api.CrearUsuarioAsync(ApiFactory.Unico("coordA"), roles: [C]);
        var coordinadorVencido = await api.CrearUsuarioAsync(ApiFactory.Unico("coordV"), roles: [C]);
        var ip = await api.CrearUsuarioAsync(ApiFactory.Unico("ip"), roles: [I]);
        var integrante = await api.CrearUsuarioAsync(ApiFactory.Unico("integ"), roles: [I]);
        var ajeno = await api.CrearUsuarioAsync(ApiFactory.Unico("ajeno"), roles: [I]);
        var retirado = await api.CrearUsuarioAsync(ApiFactory.Unico("retir"), roles: [I]);
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);

        var (proyectoA, proyectoB) = await api.EnBaseDatosAsync(async db =>
        {
            db.ProgramaIntegrantes.AddRange(
                new ProgramaIntegrante { ProgramaId = 1, UsuarioId = coordinadorA.Id, RolEnPrograma = "Coordinador", FechaInicio = hoy.AddYears(-1) },
                new ProgramaIntegrante { ProgramaId = 1, UsuarioId = coordinadorVencido.Id, RolEnPrograma = "Coordinador", FechaInicio = hoy.AddYears(-2), FechaFin = hoy.AddDays(-1) });
            var a = NuevoProyecto("A", 1, ip.Id);
            var b = NuevoProyecto("B", 2, ip.Id);
            db.Proyectos.AddRange(a, b);
            await db.SaveChangesAsync();
            db.ProyectoIntegrantes.AddRange(
                new ProyectoIntegrante { ProyectoId = a.Id, UsuarioId = ip.Id, RolEnProyecto = "IP" },
                new ProyectoIntegrante { ProyectoId = a.Id, UsuarioId = integrante.Id, RolEnProyecto = "Co-investigador" },
                new ProyectoIntegrante { ProyectoId = a.Id, UsuarioId = retirado.Id, RolEnProyecto = "Co-investigador", FechaFin = hoy.AddDays(-1) });
            await db.SaveChangesAsync();
            return (a.Id, b.Id);
        });

        using var scope = api.Services.CreateScope();
        var acceso = scope.ServiceProvider.GetRequiredService<AccesoRecursos>();
        UsuarioAutenticado Como(Usuario u, params string[] roles) => new(u.Id, roles);

        // Coordinador: solo su programa y con vinculación vigente.
        Assert.True(await acceso.PuedeGestionarProgramaAsync(Como(coordinadorA, C), 1));
        Assert.False(await acceso.PuedeGestionarProgramaAsync(Como(coordinadorA, C), 2));
        Assert.False(await acceso.PuedeGestionarProgramaAsync(Como(coordinadorVencido, C), 1));
        Assert.True(await acceso.PuedeVerProyectoAsync(Como(coordinadorA, C), proyectoA));
        Assert.False(await acceso.PuedeVerProyectoAsync(Como(coordinadorA, C), proyectoB));
        Assert.False(await acceso.PuedeModificarProyectoComoIpAsync(Como(coordinadorA, C), proyectoB));

        // Investigador: solo los proyectos donde es integrante vigente; editar solo si es IP.
        Assert.True(await acceso.PuedeVerProyectoAsync(Como(integrante, I), proyectoA));
        Assert.False(await acceso.PuedeVerProyectoAsync(Como(ajeno, I), proyectoA));
        // "Mis proyectos" incluye vinculaciones históricas (HU-13): el retirado consulta, pero no edita.
        Assert.True(await acceso.PuedeVerProyectoAsync(Como(retirado, I), proyectoA));
        Assert.False(await acceso.EsIntegranteProyectoAsync(retirado.Id, proyectoA));
        Assert.False(await acceso.PuedeVerProyectoAsync(Como(integrante, I), proyectoB));
        Assert.True(await acceso.PuedeModificarProyectoComoIpAsync(Como(ip, I), proyectoA));
        Assert.False(await acceso.PuedeModificarProyectoComoIpAsync(Como(integrante, I), proyectoA));
        Assert.False(await acceso.PuedeGestionarProgramaAsync(Como(ip, I), 1));

        // Administrador y Director: todos. Externo: ninguno. Varios roles: unión.
        Assert.True(await acceso.PuedeVerProyectoAsync(new UsuarioAutenticado(0, [D]), proyectoB));
        Assert.True(await acceso.PuedeGestionarProgramaAsync(new UsuarioAutenticado(0, [A]), 5));
        Assert.False(await acceso.PuedeVerProyectoAsync(Como(integrante, E), proyectoA));
        Assert.True(await acceso.PuedeVerProyectoAsync(Como(coordinadorA, I, C), proyectoA));
        Assert.True(await acceso.PuedeVerProyectoAsync(Como(integrante, C, I), proyectoA));
    }

    private static Proyecto NuevoProyecto(string sufijo, int programaId, int ipId) => new()
    {
        Codigo = $"T-{Guid.NewGuid():N}"[..20] + sufijo,
        Titulo = "Proyecto de prueba " + sufijo,
        ProgramaId = programaId,
        TipoProyectoId = 1,
        EstadoId = 1,
        InvestigadorPrincipalId = ipId,
    };

    private static HttpRequestMessage Peticion(string metodo, string ruta) =>
        new(new HttpMethod(metodo), ruta) { Content = metodo == "GET" ? null : JsonContent.Create(new { }) };
}
