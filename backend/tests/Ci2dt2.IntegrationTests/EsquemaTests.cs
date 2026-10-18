using Ci2dt2.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Ci2dt2.IntegrationTests;

/// <summary>Reglas del diccionario de datos (modelo v3) aplicadas por la propia base de datos.</summary>
[Collection(ColeccionApi.Nombre)]
public class EsquemaTests(ApiFactory api)
{
    private Task<int> EscalarAsync(string sql) =>
        api.EnBaseDatosAsync(db => db.Database.SqlQueryRaw<int>(sql).SingleAsync());

    [Fact]
    public async Task Crea_las_67_tablas_con_125_llaves_del_modelo_mas_las_de_auditoria()
    {
        Assert.Equal(67, await EscalarAsync("SELECT COUNT(*) AS Value FROM sys.tables WHERE name <> '__EFMigrationsHistory'"));
        // 125 llaves del modelo + 64 CreadoPorId (todas las tablas menos las 3 inmutables).
        Assert.Equal(125 + 64, await EscalarAsync("SELECT COUNT(*) AS Value FROM sys.foreign_keys"));
        Assert.Equal(0, await EscalarAsync("SELECT COUNT(*) AS Value FROM sys.foreign_keys WHERE delete_referential_action <> 0"));
    }

    [Fact]
    public async Task Los_textos_son_nvarchar_y_las_tablas_inmutables_no_llevan_auditoria()
    {
        Assert.Equal(0, await EscalarAsync(
            "SELECT COUNT(*) AS Value FROM INFORMATION_SCHEMA.COLUMNS WHERE DATA_TYPE IN ('varchar', 'text', 'ntext')"));
        Assert.Equal(64, await EscalarAsync(
            "SELECT COUNT(*) AS Value FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'Activo'"));
        Assert.Equal(0, await EscalarAsync(
            "SELECT COUNT(*) AS Value FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'CreadoEn' " +
            "AND TABLE_NAME IN ('LogActividades', 'DocumentoFirmas', 'ConsentimientosDatos')"));
    }

    [Fact]
    public async Task Carga_los_catalogos_del_diccionario()
    {
        Assert.Equal(["Admin", "Coordinador", "Director", "Docente", "Estudiante", "Externo", "Investigador", "Logistica", "Secretaria"],
            await api.EnBaseDatosAsync(db => db.Roles.OrderBy(r => r.Nombre).Select(r => r.Nombre).ToListAsync()));
        Assert.Equal(9, await api.EnBaseDatosAsync(db => db.EstadosProyecto.CountAsync()));
        Assert.True(await api.EnBaseDatosAsync(db => db.TiposProyecto.AnyAsync(t => t.Nombre == "Reto Empresarial" && t.RequiereHitosFijos)));
        Assert.Equal(3, await api.EnBaseDatosAsync(db => db.Modalidades.CountAsync()));
    }

    [Fact]
    public async Task Las_columnas_de_auditoria_se_llenan_solas()
    {
        var antes = DateTime.UtcNow.AddMinutes(-1);
        var usuario = await api.CrearUsuarioAsync(ApiFactory.Unico("auditcol"), roles: ["Investigador"]);

        var creado = await api.EnBaseDatosAsync(db => db.Usuarios.AsNoTracking().SingleAsync(u => u.Id == usuario.Id));
        Assert.True(creado.CreadoEn >= antes);
        Assert.True(creado.Activo);
        Assert.Null(creado.ActualizadoEn);

        await api.EnBaseDatosAsync(async db =>
        {
            var u = await db.Usuarios.SingleAsync(x => x.Id == usuario.Id);
            u.Cargo = "Cambio";
            return await db.SaveChangesAsync();
        });
        var modificado = await api.EnBaseDatosAsync(db => db.Usuarios.AsNoTracking().SingleAsync(u => u.Id == usuario.Id));
        Assert.NotNull(modificado.ActualizadoEn);
    }

    [Fact]
    public async Task Un_usuario_con_acceso_debe_tener_contrasena()
    {
        var error = await Assert.ThrowsAsync<DbUpdateException>(() => api.EnBaseDatosAsync(db =>
        {
            db.Usuarios.Add(new Usuario { Email = ApiFactory.Unico("sinclave"), Nombres = "A", Apellidos = "B", TieneAcceso = true });
            return db.SaveChangesAsync();
        }));
        Assert.Contains("CK_Usuarios_PasswordSiTieneAcceso", error.InnerException!.Message);

        // Sin acceso (jurado, coinventor, aspirante) sí puede existir sin contraseña.
        await api.EnBaseDatosAsync(db =>
        {
            db.Usuarios.Add(new Usuario { Email = ApiFactory.Unico("jurado"), Nombres = "A", Apellidos = "B", TieneAcceso = false });
            return db.SaveChangesAsync();
        });
    }

    [Fact]
    public async Task Un_cronograma_pertenece_a_exactamente_una_entidad()
    {
        var ip = await api.CrearUsuarioAsync(ApiFactory.Unico("crono"), roles: ["Investigador"]);
        var proyectoId = await api.EnBaseDatosAsync(async db =>
        {
            var p = new Proyecto { Titulo = "Proyecto cronograma", ProgramaId = 1, TipoProyectoId = 1, EstadoId = 1, InvestigadorPrincipalId = ip.Id };
            db.Proyectos.Add(p);
            await db.SaveChangesAsync();
            return p.Id;
        });

        var sinEntidad = await Assert.ThrowsAsync<DbUpdateException>(() => api.EnBaseDatosAsync(db =>
        {
            db.Cronogramas.Add(new Cronograma { Nombre = "Sin entidad" });
            return db.SaveChangesAsync();
        }));
        Assert.Contains("CK_Cronogramas_UnaEntidad", sinEntidad.InnerException!.Message);

        await api.EnBaseDatosAsync(db =>
        {
            db.Cronogramas.Add(new Cronograma { Nombre = "Del proyecto", ProyectoId = proyectoId });
            return db.SaveChangesAsync();
        });
    }

    [Fact]
    public async Task El_porcentaje_de_avance_esta_entre_0_y_100()
    {
        var ip = await api.CrearUsuarioAsync(ApiFactory.Unico("pct"), roles: ["Investigador"]);
        var error = await Assert.ThrowsAsync<DbUpdateException>(() => api.EnBaseDatosAsync(db =>
        {
            db.Proyectos.Add(new Proyecto
            {
                Titulo = "Avance inválido",
                ProgramaId = 1,
                TipoProyectoId = 1,
                EstadoId = 1,
                InvestigadorPrincipalId = ip.Id,
                PorcentajeAvance = 150,
            });
            return db.SaveChangesAsync();
        }));
        Assert.Contains("CK_Proyectos_PorcentajeAvance", error.InnerException!.Message);
    }

    [Fact]
    public async Task Las_tablas_inmutables_no_admiten_update_ni_delete()
    {
        var id = await api.EnBaseDatosAsync(async db =>
        {
            var log = new LogActividad { Recurso = "Prueba", Accion = "POST", FechaUtc = DateTime.UtcNow };
            db.LogActividades.Add(log);
            await db.SaveChangesAsync();
            return log.Id;
        });

        var update = await Assert.ThrowsAsync<SqlException>(() => api.EnBaseDatosAsync(db =>
            db.LogActividades.Where(l => l.Id == id).ExecuteUpdateAsync(s => s.SetProperty(l => l.Accion, "PUT"))));
        var delete = await Assert.ThrowsAsync<SqlException>(() => api.EnBaseDatosAsync(db =>
            db.LogActividades.Where(l => l.Id == id).ExecuteDeleteAsync()));

        Assert.Contains("inmutable", update.Message);
        Assert.Contains("inmutable", delete.Message);
        Assert.Equal("POST", await api.EnBaseDatosAsync(db => db.LogActividades.Where(l => l.Id == id).Select(l => l.Accion).SingleAsync()));
    }

    [Fact]
    public async Task Los_textos_conservan_tildes_y_caracteres_unicode()
    {
        var usuario = await api.CrearUsuarioAsync(ApiFactory.Unico("unicode"), roles: ["Investigador"]);
        await api.EnBaseDatosAsync(db => db.Usuarios.Where(u => u.Id == usuario.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.Nombres, "Ñandú Łukasz 李")));

        Assert.Equal("Ñandú Łukasz 李", await api.EnBaseDatosAsync(db =>
            db.Usuarios.Where(u => u.Id == usuario.Id).Select(u => u.Nombres).SingleAsync()));
    }
}
