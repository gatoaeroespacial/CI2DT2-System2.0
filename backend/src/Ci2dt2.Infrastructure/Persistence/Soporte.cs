using System.Data;
using Ci2dt2.Application.Common;
using Ci2dt2.Application.Seguridad;
using Ci2dt2.Domain;
using Ci2dt2.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ci2dt2.Infrastructure.Persistence;

internal sealed class SqlConnectionFactory(IConfiguration configuracion) : IDbConnectionFactory
{
    public IDbConnection Crear() => new SqlConnection(DependencyInjection.CadenaConexion(configuracion));
}

public sealed class OpcionesBaseDatos
{
    public const string Seccion = "BaseDatos";

    /// <summary>Aplica las migraciones pendientes al iniciar la API.</summary>
    public bool MigrarAlIniciar { get; set; } = true;
}

/// <summary>Usuario Administrador inicial (HT-02). La contraseña se configura fuera del código.</summary>
public sealed class OpcionesAdminInicial
{
    public const string Seccion = "AdminInicial";

    public string? Email { get; set; }
    public string? Password { get; set; }
    public string Nombres { get; set; } = "Administrador";
    public string Apellidos { get; set; } = "del sistema";
}

public sealed class InicializadorBaseDatos(
    AppDbContext db,
    IPasswordHasher hasher,
    IOptions<OpcionesBaseDatos> opcionesBd,
    IOptions<OpcionesAdminInicial> opcionesAdmin,
    ILogger<InicializadorBaseDatos> logger)
{
    public async Task InicializarAsync(CancellationToken ct = default)
    {
        if (opcionesBd.Value.MigrarAlIniciar)
            await db.Database.MigrateAsync(ct);

        await CrearAdministradorInicialAsync(ct);
    }

    private async Task CrearAdministradorInicialAsync(CancellationToken ct)
    {
        var admin = opcionesAdmin.Value;
        if (string.IsNullOrWhiteSpace(admin.Email) || string.IsNullOrWhiteSpace(admin.Password))
            return;

        var hayAdministrador = await db.Usuarios.AnyAsync(u => u.Roles.Any(r => r.Rol!.Nombre == Roles.Administrador), ct);
        if (hayAdministrador)
            return;

        var rol = await db.Roles.SingleAsync(r => r.Nombre == Roles.Administrador, ct);
        db.Usuarios.Add(new Usuario
        {
            Email = AuthService.NormalizarEmail(admin.Email),
            Nombres = admin.Nombres,
            Apellidos = admin.Apellidos,
            PasswordHash = hasher.Hash(admin.Password),
            TieneAcceso = true,
            Roles = [new UsuarioRol { Rol = rol }],
        });
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Usuario Administrador inicial creado: {Email}", admin.Email);
    }
}

/// <summary>Fábrica para las herramientas de EF Core (dotnet ef migrations).</summary>
internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var conexion = Environment.GetEnvironmentVariable("CI2DT2_CONNECTION")
            ?? @"Server=(localdb)\MSSQLLocalDB;Database=Ci2dt2Dev;Trusted_Connection=True;TrustServerCertificate=True";
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(conexion).Options;
        return new AppDbContext(options);
    }
}
