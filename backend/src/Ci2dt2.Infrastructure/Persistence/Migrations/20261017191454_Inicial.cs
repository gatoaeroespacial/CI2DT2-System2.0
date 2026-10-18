using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Ci2dt2.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Aliados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TipoAliadoId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Nit = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Pais = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Ciudad = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Sector = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    RepresentanteLegal = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Aliados", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Nombres = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Apellidos = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TipoDocumento = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    NumeroDocumento = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Cargo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Dependencia = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    AliadoId = table.Column<int>(type: "int", nullable: true),
                    CvLacUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Orcid = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ExpoPushToken = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TieneAcceso = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                    table.CheckConstraint("CK_Usuarios_PasswordSiTieneAcceso", "[TieneAcceso] = 0 OR [PasswordHash] IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_Usuarios_Aliados_AliadoId",
                        column: x => x.AliadoId,
                        principalTable: "Aliados",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Usuarios_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EstadosProyecto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: true),
                    EsFinal = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadosProyecto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EstadosProyecto_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LogActividades",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: true),
                    IpOrigen = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    Recurso = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    RecursoId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Accion = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Ruta = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    EstadoHttp = table.Column<int>(type: "int", nullable: true),
                    ValoresAnteriores = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValoresNuevos = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HashEncadenado = table.Column<string>(type: "char(64)", unicode: false, fixedLength: true, maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogActividades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LogActividades_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Modalidades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modalidades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Modalidades_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Notificaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Mensaje = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReferenciaTipo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    ReferenciaId = table.Column<int>(type: "int", nullable: true),
                    Leida = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    EnviadaPush = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    FechaEnvioPush = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EnviadaEmail = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    FechaEnvioEmail = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Intentos = table.Column<int>(type: "int", nullable: true),
                    UltimoError = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notificaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notificaciones_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Notificaciones_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PoliticasTratamientoDatos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Version = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TextoUrl = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    HashTexto = table.Column<string>(type: "char(64)", unicode: false, fixedLength: true, maxLength: 64, nullable: true),
                    VigenteDesde = table.Column<DateOnly>(type: "date", nullable: false),
                    VigenteHasta = table.Column<DateOnly>(type: "date", nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoliticasTratamientoDatos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PoliticasTratamientoDatos_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PreferenciasNotificacion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    RecibirEmail = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RecibirPush = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreferenciasNotificacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreferenciasNotificacion_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PreferenciasNotificacion_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Programas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Programas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Programas_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ExpiraEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevocadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IpOrigen = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Roles_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TiposAliado",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposAliado", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TiposAliado_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TiposConvocatoria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposConvocatoria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TiposConvocatoria_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TiposDocumento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposDocumento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TiposDocumento_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TiposEvento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    DuracionMinutosDefecto = table.Column<int>(type: "int", nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposEvento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TiposEvento_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TiposFormacion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposFormacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TiposFormacion_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TiposHito",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposHito", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TiposHito_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TiposIntegranteSemillero",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposIntegranteSemillero", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TiposIntegranteSemillero_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TiposProyecto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    RequiereHitosFijos = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposProyecto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TiposProyecto_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TiposPublicacion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposPublicacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TiposPublicacion_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ConsentimientosDatos",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PoliticaId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: true),
                    Correo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Contexto = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Aceptado = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    FechaAceptacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpOrigen = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    HashEvidencia = table.Column<string>(type: "char(64)", unicode: false, fixedLength: true, maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsentimientosDatos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsentimientosDatos_PoliticasTratamientoDatos_PoliticaId",
                        column: x => x.PoliticaId,
                        principalTable: "PoliticasTratamientoDatos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ConsentimientosDatos_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "GruposInvestigacion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramaId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CodigoGrupLac = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    CategoriaMinciencias = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GruposInvestigacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GruposInvestigacion_Programas_ProgramaId",
                        column: x => x.ProgramaId,
                        principalTable: "Programas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GruposInvestigacion_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Indicadores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramaId = table.Column<int>(type: "int", nullable: true),
                    Codigo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Unidad = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ConsultaClave = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    UmbralVerdePct = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    UmbralAmarilloPct = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Periodicidad = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Indicadores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Indicadores_Programas_ProgramaId",
                        column: x => x.ProgramaId,
                        principalTable: "Programas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Indicadores_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ObjetivosEspecificos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramaId = table.Column<int>(type: "int", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Orden = table.Column<int>(type: "int", nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObjetivosEspecificos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ObjetivosEspecificos_Programas_ProgramaId",
                        column: x => x.ProgramaId,
                        principalTable: "Programas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ObjetivosEspecificos_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProgramaAliados",
                columns: table => new
                {
                    ProgramaId = table.Column<int>(type: "int", nullable: false),
                    AliadoId = table.Column<int>(type: "int", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramaAliados", x => new { x.ProgramaId, x.AliadoId });
                    table.ForeignKey(
                        name: "FK_ProgramaAliados_Aliados_AliadoId",
                        column: x => x.AliadoId,
                        principalTable: "Aliados",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProgramaAliados_Programas_ProgramaId",
                        column: x => x.ProgramaId,
                        principalTable: "Programas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProgramaAliados_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProgramaIntegrantes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramaId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    RolEnPrograma = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaInicio = table.Column<DateOnly>(type: "date", nullable: true),
                    FechaFin = table.Column<DateOnly>(type: "date", nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramaIntegrantes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgramaIntegrantes_Programas_ProgramaId",
                        column: x => x.ProgramaId,
                        principalTable: "Programas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProgramaIntegrantes_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProgramaIntegrantes_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Semilleros",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramaId = table.Column<int>(type: "int", nullable: false),
                    CoordinadorId = table.Column<int>(type: "int", nullable: true),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Semilleros", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Semilleros_Programas_ProgramaId",
                        column: x => x.ProgramaId,
                        principalTable: "Programas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Semilleros_Usuarios_CoordinadorId",
                        column: x => x.CoordinadorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Semilleros_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UsuarioRoles",
                columns: table => new
                {
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    RolId = table.Column<int>(type: "int", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioRoles", x => new { x.UsuarioId, x.RolId });
                    table.ForeignKey(
                        name: "FK_UsuarioRoles_Roles_RolId",
                        column: x => x.RolId,
                        principalTable: "Roles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UsuarioRoles_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UsuarioRoles_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Convocatorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Numero = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    TipoConvocatoriaId = table.Column<int>(type: "int", nullable: false),
                    EntidadConvocanteId = table.Column<int>(type: "int", nullable: true),
                    Plataforma = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    FechaApertura = table.Column<DateOnly>(type: "date", nullable: true),
                    FechaCierre = table.Column<DateOnly>(type: "date", nullable: true),
                    Moneda = table.Column<string>(type: "char(3)", unicode: false, fixedLength: true, maxLength: 3, nullable: true),
                    MontoMaximo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Convocatorias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Convocatorias_Aliados_EntidadConvocanteId",
                        column: x => x.EntidadConvocanteId,
                        principalTable: "Aliados",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Convocatorias_TiposConvocatoria_TipoConvocatoriaId",
                        column: x => x.TipoConvocatoriaId,
                        principalTable: "TiposConvocatoria",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Convocatorias_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Documentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TipoDocumentoId = table.Column<int>(type: "int", nullable: false),
                    EntidadTipo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    EntidadId = table.Column<int>(type: "int", nullable: true),
                    NombreArchivo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UrlArchivo = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    HashSha256 = table.Column<string>(type: "char(64)", unicode: false, fixedLength: true, maxLength: 64, nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    EsPublico = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    SubidoPorId = table.Column<int>(type: "int", nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documentos_TiposDocumento_TipoDocumentoId",
                        column: x => x.TipoDocumentoId,
                        principalTable: "TiposDocumento",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Documentos_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Documentos_Usuarios_SubidoPorId",
                        column: x => x.SubidoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Eventos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramaId = table.Column<int>(type: "int", nullable: true),
                    TipoEventoId = table.Column<int>(type: "int", nullable: false),
                    ModalidadId = table.Column<int>(type: "int", nullable: true),
                    ResponsableLogisticaId = table.Column<int>(type: "int", nullable: true),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Lugar = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CupoMaximo = table.Column<int>(type: "int", nullable: true),
                    Agenda = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Eventos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Eventos_Modalidades_ModalidadId",
                        column: x => x.ModalidadId,
                        principalTable: "Modalidades",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Eventos_Programas_ProgramaId",
                        column: x => x.ProgramaId,
                        principalTable: "Programas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Eventos_TiposEvento_TipoEventoId",
                        column: x => x.TipoEventoId,
                        principalTable: "TiposEvento",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Eventos_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Eventos_Usuarios_ResponsableLogisticaId",
                        column: x => x.ResponsableLogisticaId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Formaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramaId = table.Column<int>(type: "int", nullable: false),
                    TipoFormacionId = table.Column<int>(type: "int", nullable: false),
                    ModalidadId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IntensidadHoras = table.Column<int>(type: "int", nullable: true),
                    ValorMatricula = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    CupoMaximo = table.Column<int>(type: "int", nullable: true),
                    PuntoEquilibrio = table.Column<int>(type: "int", nullable: true),
                    MetaFinancieraCop = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    FechaInicioInscripcion = table.Column<DateOnly>(type: "date", nullable: true),
                    FechaFinInscripcion = table.Column<DateOnly>(type: "date", nullable: true),
                    FechaInicio = table.Column<DateOnly>(type: "date", nullable: true),
                    FechaFin = table.Column<DateOnly>(type: "date", nullable: true),
                    AsistenciaMinimaPct = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    NotaMinima = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    AutorizadaPorId = table.Column<int>(type: "int", nullable: true),
                    FechaAutorizacion = table.Column<DateOnly>(type: "date", nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Formaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Formaciones_Modalidades_ModalidadId",
                        column: x => x.ModalidadId,
                        principalTable: "Modalidades",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Formaciones_Programas_ProgramaId",
                        column: x => x.ProgramaId,
                        principalTable: "Programas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Formaciones_TiposFormacion_TipoFormacionId",
                        column: x => x.TipoFormacionId,
                        principalTable: "TiposFormacion",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Formaciones_Usuarios_AutorizadaPorId",
                        column: x => x.AutorizadaPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Formaciones_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Proyectos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Titulo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Resumen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProgramaId = table.Column<int>(type: "int", nullable: false),
                    TipoProyectoId = table.Column<int>(type: "int", nullable: false),
                    EstadoId = table.Column<int>(type: "int", nullable: false),
                    InvestigadorPrincipalId = table.Column<int>(type: "int", nullable: false),
                    FechaInicio = table.Column<DateOnly>(type: "date", nullable: true),
                    FechaFin = table.Column<DateOnly>(type: "date", nullable: true),
                    PorcentajeAvance = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Moneda = table.Column<string>(type: "char(3)", unicode: false, fixedLength: true, maxLength: 3, nullable: true),
                    PresupuestoTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    TasaCambio = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    PresupuestoTotalCop = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proyectos", x => x.Id);
                    table.CheckConstraint("CK_Proyectos_PorcentajeAvance", "[PorcentajeAvance] BETWEEN 0 AND 100");
                    table.ForeignKey(
                        name: "FK_Proyectos_EstadosProyecto_EstadoId",
                        column: x => x.EstadoId,
                        principalTable: "EstadosProyecto",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Proyectos_Programas_ProgramaId",
                        column: x => x.ProgramaId,
                        principalTable: "Programas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Proyectos_TiposProyecto_TipoProyectoId",
                        column: x => x.TipoProyectoId,
                        principalTable: "TiposProyecto",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Proyectos_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Proyectos_Usuarios_InvestigadorPrincipalId",
                        column: x => x.InvestigadorPrincipalId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Visitas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConsentimientoId = table.Column<long>(type: "bigint", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: true),
                    RegistradaPorId = table.Column<int>(type: "int", nullable: true),
                    Identificacion = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    NombreCompleto = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Correo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Celular = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Dependencia = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Cargo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MotivoVisita = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    FechaHoraEntrada = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaHoraSalida = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MinutosPermanencia = table.Column<int>(type: "int", nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Visitas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Visitas_ConsentimientosDatos_ConsentimientoId",
                        column: x => x.ConsentimientoId,
                        principalTable: "ConsentimientosDatos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Visitas_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Visitas_Usuarios_RegistradaPorId",
                        column: x => x.RegistradaPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Visitas_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "IndicadorMediciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IndicadorId = table.Column<int>(type: "int", nullable: false),
                    Periodo = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    Meta = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorActual = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Semaforo = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
                    FechaCalculo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndicadorMediciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndicadorMediciones_Indicadores_IndicadorId",
                        column: x => x.IndicadorId,
                        principalTable: "Indicadores",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IndicadorMediciones_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DocumentoFirmas",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentoId = table.Column<int>(type: "int", nullable: false),
                    FirmanteId = table.Column<int>(type: "int", nullable: false),
                    RolFirmante = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    HashDocumento = table.Column<string>(type: "char(64)", unicode: false, fixedLength: true, maxLength: 64, nullable: false),
                    IpFirma = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    FechaFirma = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HashEncadenado = table.Column<string>(type: "char(64)", unicode: false, fixedLength: true, maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentoFirmas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentoFirmas_Documentos_DocumentoId",
                        column: x => x.DocumentoId,
                        principalTable: "Documentos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocumentoFirmas_Usuarios_FirmanteId",
                        column: x => x.FirmanteId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FuentesConocimiento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentoId = table.Column<int>(type: "int", nullable: true),
                    Titulo = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    TipoFuente = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EntidadTipo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    EntidadId = table.Column<int>(type: "int", nullable: true),
                    EsPublica = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    HashContenido = table.Column<string>(type: "char(64)", unicode: false, fixedLength: true, maxLength: 64, nullable: true),
                    EstadoIndexacion = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                    IdVectorStore = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaIndexacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FuentesConocimiento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FuentesConocimiento_Documentos_DocumentoId",
                        column: x => x.DocumentoId,
                        principalTable: "Documentos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FuentesConocimiento_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ReportesGenerados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramaId = table.Column<int>(type: "int", nullable: true),
                    GeneradoPorId = table.Column<int>(type: "int", nullable: false),
                    DocumentoId = table.Column<int>(type: "int", nullable: true),
                    Tipo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Periodo = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    Formato = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportesGenerados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportesGenerados_Documentos_DocumentoId",
                        column: x => x.DocumentoId,
                        principalTable: "Documentos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReportesGenerados_Programas_ProgramaId",
                        column: x => x.ProgramaId,
                        principalTable: "Programas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReportesGenerados_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReportesGenerados_Usuarios_GeneradoPorId",
                        column: x => x.GeneradoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EventoAsistentes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventoId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    AliadoId = table.Column<int>(type: "int", nullable: true),
                    ConsentimientoId = table.Column<long>(type: "bigint", nullable: true),
                    Cargo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    HoraRegistro = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Retroalimentacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventoAsistentes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventoAsistentes_Aliados_AliadoId",
                        column: x => x.AliadoId,
                        principalTable: "Aliados",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EventoAsistentes_ConsentimientosDatos_ConsentimientoId",
                        column: x => x.ConsentimientoId,
                        principalTable: "ConsentimientosDatos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EventoAsistentes_Eventos_EventoId",
                        column: x => x.EventoId,
                        principalTable: "Eventos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EventoAsistentes_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EventoAsistentes_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FormacionDocentes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FormacionId = table.Column<int>(type: "int", nullable: false),
                    DocenteId = table.Column<int>(type: "int", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormacionDocentes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormacionDocentes_Formaciones_FormacionId",
                        column: x => x.FormacionId,
                        principalTable: "Formaciones",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FormacionDocentes_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FormacionDocentes_Usuarios_DocenteId",
                        column: x => x.DocenteId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FormacionSesiones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FormacionId = table.Column<int>(type: "int", nullable: false),
                    DocenteId = table.Column<int>(type: "int", nullable: true),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    HoraInicio = table.Column<TimeOnly>(type: "time", nullable: true),
                    HoraFin = table.Column<TimeOnly>(type: "time", nullable: true),
                    Tema = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormacionSesiones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormacionSesiones_Formaciones_FormacionId",
                        column: x => x.FormacionId,
                        principalTable: "Formaciones",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FormacionSesiones_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FormacionSesiones_Usuarios_DocenteId",
                        column: x => x.DocenteId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Inscripciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FormacionId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    ConsentimientoId = table.Column<long>(type: "bigint", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    ValorPagado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    SoportePagoDocId = table.Column<int>(type: "int", nullable: true),
                    PorcentajeAsistencia = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    NotaFinal = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: true),
                    HabilitadoCertificacion = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inscripciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inscripciones_ConsentimientosDatos_ConsentimientoId",
                        column: x => x.ConsentimientoId,
                        principalTable: "ConsentimientosDatos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Inscripciones_Documentos_SoportePagoDocId",
                        column: x => x.SoportePagoDocId,
                        principalTable: "Documentos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Inscripciones_Formaciones_FormacionId",
                        column: x => x.FormacionId,
                        principalTable: "Formaciones",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Inscripciones_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Inscripciones_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Cronogramas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ProyectoId = table.Column<int>(type: "int", nullable: true),
                    FormacionId = table.Column<int>(type: "int", nullable: true),
                    EventoId = table.Column<int>(type: "int", nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cronogramas", x => x.Id);
                    table.CheckConstraint("CK_Cronogramas_UnaEntidad", "IIF([ProyectoId] IS NULL, 0, 1) + IIF([FormacionId] IS NULL, 0, 1) + IIF([EventoId] IS NULL, 0, 1) = 1");
                    table.ForeignKey(
                        name: "FK_Cronogramas_Eventos_EventoId",
                        column: x => x.EventoId,
                        principalTable: "Eventos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Cronogramas_Formaciones_FormacionId",
                        column: x => x.FormacionId,
                        principalTable: "Formaciones",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Cronogramas_Proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "Proyectos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Cronogramas_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FuentesFinanciamiento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProyectoId = table.Column<int>(type: "int", nullable: false),
                    AliadoId = table.Column<int>(type: "int", nullable: true),
                    TipoFuente = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    TipoAporte = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Moneda = table.Column<string>(type: "char(3)", unicode: false, fixedLength: true, maxLength: 3, nullable: true),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    TasaCambio = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    MontoCop = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FuentesFinanciamiento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FuentesFinanciamiento_Aliados_AliadoId",
                        column: x => x.AliadoId,
                        principalTable: "Aliados",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FuentesFinanciamiento_Proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "Proyectos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FuentesFinanciamiento_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PropiedadIntelectual",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProyectoId = table.Column<int>(type: "int", nullable: true),
                    Tipo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    EntidadTramite = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    NumeroRadicado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FechaRadicacion = table.Column<DateOnly>(type: "date", nullable: true),
                    NumeroRegistro = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NumeroResolucion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FechaConcesion = table.Column<DateOnly>(type: "date", nullable: true),
                    ParticipacionUcaldasPct = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropiedadIntelectual", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropiedadIntelectual_Proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "Proyectos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PropiedadIntelectual_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PropuestasConvocatoria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProyectoId = table.Column<int>(type: "int", nullable: false),
                    ConvocatoriaId = table.Column<int>(type: "int", nullable: false),
                    FechaPresentacion = table.Column<DateOnly>(type: "date", nullable: true),
                    IdPostulacionExterna = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    CodigoMga = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    MontoSolicitado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Moneda = table.Column<string>(type: "char(3)", unicode: false, fixedLength: true, maxLength: 3, nullable: true),
                    TasaCambioEstimada = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    MontoSolicitadoCop = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Resultado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Puntaje = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    FaseExterna = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    FechaResultado = table.Column<DateOnly>(type: "date", nullable: true),
                    ComprobanteDocId = table.Column<int>(type: "int", nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropuestasConvocatoria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropuestasConvocatoria_Convocatorias_ConvocatoriaId",
                        column: x => x.ConvocatoriaId,
                        principalTable: "Convocatorias",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PropuestasConvocatoria_Documentos_ComprobanteDocId",
                        column: x => x.ComprobanteDocId,
                        principalTable: "Documentos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PropuestasConvocatoria_Proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "Proyectos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PropuestasConvocatoria_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProyectoAliados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProyectoId = table.Column<int>(type: "int", nullable: false),
                    AliadoId = table.Column<int>(type: "int", nullable: false),
                    RolAliado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    CartaCompromisoDocId = table.Column<int>(type: "int", nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProyectoAliados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProyectoAliados_Aliados_AliadoId",
                        column: x => x.AliadoId,
                        principalTable: "Aliados",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProyectoAliados_Documentos_CartaCompromisoDocId",
                        column: x => x.CartaCompromisoDocId,
                        principalTable: "Documentos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProyectoAliados_Proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "Proyectos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProyectoAliados_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProyectoAntecedentes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProyectoId = table.Column<int>(type: "int", nullable: false),
                    ConvocatoriaId = table.Column<int>(type: "int", nullable: true),
                    NombreConvocatoria = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Anio = table.Column<int>(type: "int", nullable: true),
                    Resultado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Puntaje = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    EstadoBanco = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProyectoAntecedentes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProyectoAntecedentes_Convocatorias_ConvocatoriaId",
                        column: x => x.ConvocatoriaId,
                        principalTable: "Convocatorias",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProyectoAntecedentes_Proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "Proyectos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProyectoAntecedentes_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProyectoCierres",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProyectoId = table.Column<int>(type: "int", nullable: false),
                    InformeTecnicoDocId = table.Column<int>(type: "int", nullable: true),
                    InformeFinancieroDocId = table.Column<int>(type: "int", nullable: true),
                    FechaRadicacion = table.Column<DateOnly>(type: "date", nullable: true),
                    ConceptoComite = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ActaComiteDocId = table.Column<int>(type: "int", nullable: true),
                    PazYSalvoDocId = table.Column<int>(type: "int", nullable: true),
                    FechaPazYSalvo = table.Column<DateOnly>(type: "date", nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProyectoCierres", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProyectoCierres_Documentos_ActaComiteDocId",
                        column: x => x.ActaComiteDocId,
                        principalTable: "Documentos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProyectoCierres_Documentos_InformeFinancieroDocId",
                        column: x => x.InformeFinancieroDocId,
                        principalTable: "Documentos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProyectoCierres_Documentos_InformeTecnicoDocId",
                        column: x => x.InformeTecnicoDocId,
                        principalTable: "Documentos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProyectoCierres_Documentos_PazYSalvoDocId",
                        column: x => x.PazYSalvoDocId,
                        principalTable: "Documentos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProyectoCierres_Proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "Proyectos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProyectoCierres_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProyectoGrupos",
                columns: table => new
                {
                    ProyectoId = table.Column<int>(type: "int", nullable: false),
                    GrupoId = table.Column<int>(type: "int", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProyectoGrupos", x => new { x.ProyectoId, x.GrupoId });
                    table.ForeignKey(
                        name: "FK_ProyectoGrupos_GruposInvestigacion_GrupoId",
                        column: x => x.GrupoId,
                        principalTable: "GruposInvestigacion",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProyectoGrupos_Proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "Proyectos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProyectoGrupos_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProyectoIntegrantes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProyectoId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    RolEnProyecto = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaInicio = table.Column<DateOnly>(type: "date", nullable: true),
                    FechaFin = table.Column<DateOnly>(type: "date", nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProyectoIntegrantes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProyectoIntegrantes_Proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "Proyectos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProyectoIntegrantes_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProyectoIntegrantes_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProyectoRevisiones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProyectoId = table.Column<int>(type: "int", nullable: false),
                    RevisorId = table.Column<int>(type: "int", nullable: false),
                    Etapa = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Decision = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EstadoAnteriorId = table.Column<int>(type: "int", nullable: true),
                    EstadoNuevoId = table.Column<int>(type: "int", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProyectoRevisiones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProyectoRevisiones_EstadosProyecto_EstadoAnteriorId",
                        column: x => x.EstadoAnteriorId,
                        principalTable: "EstadosProyecto",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProyectoRevisiones_EstadosProyecto_EstadoNuevoId",
                        column: x => x.EstadoNuevoId,
                        principalTable: "EstadosProyecto",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProyectoRevisiones_Proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "Proyectos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProyectoRevisiones_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProyectoRevisiones_Usuarios_RevisorId",
                        column: x => x.RevisorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Publicaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramaId = table.Column<int>(type: "int", nullable: true),
                    ProyectoId = table.Column<int>(type: "int", nullable: true),
                    TipoPublicacionId = table.Column<int>(type: "int", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Revista = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Anio = table.Column<int>(type: "int", nullable: true),
                    Doi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EsIndexada = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Publicaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Publicaciones_Programas_ProgramaId",
                        column: x => x.ProgramaId,
                        principalTable: "Programas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Publicaciones_Proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "Proyectos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Publicaciones_TiposPublicacion_TipoPublicacionId",
                        column: x => x.TipoPublicacionId,
                        principalTable: "TiposPublicacion",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Publicaciones_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SemilleroIntegrantes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SemilleroId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    TipoIntegranteId = table.Column<int>(type: "int", nullable: false),
                    ProyectoId = table.Column<int>(type: "int", nullable: true),
                    EstadoAprobacion = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    AprobadoPorId = table.Column<int>(type: "int", nullable: true),
                    FechaIngreso = table.Column<DateOnly>(type: "date", nullable: true),
                    FechaSalida = table.Column<DateOnly>(type: "date", nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SemilleroIntegrantes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SemilleroIntegrantes_Proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "Proyectos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SemilleroIntegrantes_Semilleros_SemilleroId",
                        column: x => x.SemilleroId,
                        principalTable: "Semilleros",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SemilleroIntegrantes_TiposIntegranteSemillero_TipoIntegranteId",
                        column: x => x.TipoIntegranteId,
                        principalTable: "TiposIntegranteSemillero",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SemilleroIntegrantes_Usuarios_AprobadoPorId",
                        column: x => x.AprobadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SemilleroIntegrantes_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SemilleroIntegrantes_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TrabajosGrado",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramaId = table.Column<int>(type: "int", nullable: true),
                    ProyectoId = table.Column<int>(type: "int", nullable: true),
                    EstudianteId = table.Column<int>(type: "int", nullable: false),
                    DirectorId = table.Column<int>(type: "int", nullable: true),
                    CodirectorId = table.Column<int>(type: "int", nullable: true),
                    PropuestaDocId = table.Column<int>(type: "int", nullable: true),
                    Tipo = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    ConceptoFinal = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrabajosGrado", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrabajosGrado_Documentos_PropuestaDocId",
                        column: x => x.PropuestaDocId,
                        principalTable: "Documentos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TrabajosGrado_Programas_ProgramaId",
                        column: x => x.ProgramaId,
                        principalTable: "Programas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TrabajosGrado_Proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "Proyectos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TrabajosGrado_Usuarios_CodirectorId",
                        column: x => x.CodirectorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TrabajosGrado_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TrabajosGrado_Usuarios_DirectorId",
                        column: x => x.DirectorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TrabajosGrado_Usuarios_EstudianteId",
                        column: x => x.EstudianteId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Asistencias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SesionId = table.Column<int>(type: "int", nullable: false),
                    InscripcionId = table.Column<int>(type: "int", nullable: false),
                    Presente = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asistencias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Asistencias_FormacionSesiones_SesionId",
                        column: x => x.SesionId,
                        principalTable: "FormacionSesiones",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Asistencias_Inscripciones_InscripcionId",
                        column: x => x.InscripcionId,
                        principalTable: "Inscripciones",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Asistencias_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Calificaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InscripcionId = table.Column<int>(type: "int", nullable: false),
                    Actividad = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Nota = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: false),
                    PesoPct = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Calificaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Calificaciones_Inscripciones_InscripcionId",
                        column: x => x.InscripcionId,
                        principalTable: "Inscripciones",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Calificaciones_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Certificados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InscripcionId = table.Column<int>(type: "int", nullable: false),
                    CodigoVerificacion = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    HashCertificado = table.Column<string>(type: "char(64)", unicode: false, fixedLength: true, maxLength: 64, nullable: true),
                    TxHashBlockchain = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaEmision = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UrlVerificacion = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    FirmaDirectorId = table.Column<int>(type: "int", nullable: true),
                    FirmaCoordinadorId = table.Column<int>(type: "int", nullable: true),
                    EnviadoEmail = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certificados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Certificados_Inscripciones_InscripcionId",
                        column: x => x.InscripcionId,
                        principalTable: "Inscripciones",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Certificados_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Certificados_Usuarios_FirmaCoordinadorId",
                        column: x => x.FirmaCoordinadorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Certificados_Usuarios_FirmaDirectorId",
                        column: x => x.FirmaDirectorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Hitos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CronogramaId = table.Column<int>(type: "int", nullable: false),
                    TipoHitoId = table.Column<int>(type: "int", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FechaLimite = table.Column<DateOnly>(type: "date", nullable: false),
                    AlertarDias = table.Column<int>(type: "int", nullable: true),
                    ResponsableId = table.Column<int>(type: "int", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    FechaCompletado = table.Column<DateOnly>(type: "date", nullable: true),
                    Orden = table.Column<int>(type: "int", nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hitos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Hitos_Cronogramas_CronogramaId",
                        column: x => x.CronogramaId,
                        principalTable: "Cronogramas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Hitos_TiposHito_TipoHitoId",
                        column: x => x.TipoHitoId,
                        principalTable: "TiposHito",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Hitos_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Hitos_Usuarios_ResponsableId",
                        column: x => x.ResponsableId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PropiedadIntelectualAutores",
                columns: table => new
                {
                    PropiedadIntelectualId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    RolAutor = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    ParticipacionPct = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropiedadIntelectualAutores", x => new { x.PropiedadIntelectualId, x.UsuarioId });
                    table.ForeignKey(
                        name: "FK_PropiedadIntelectualAutores_PropiedadIntelectual_PropiedadIntelectualId",
                        column: x => x.PropiedadIntelectualId,
                        principalTable: "PropiedadIntelectual",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PropiedadIntelectualAutores_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PropiedadIntelectualAutores_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProyectoProductos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProyectoId = table.Column<int>(type: "int", nullable: false),
                    TipoProducto = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaCompromiso = table.Column<DateOnly>(type: "date", nullable: true),
                    PublicacionId = table.Column<int>(type: "int", nullable: true),
                    PropiedadIntelectualId = table.Column<int>(type: "int", nullable: true),
                    DocumentoId = table.Column<int>(type: "int", nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProyectoProductos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProyectoProductos_Documentos_DocumentoId",
                        column: x => x.DocumentoId,
                        principalTable: "Documentos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProyectoProductos_PropiedadIntelectual_PropiedadIntelectualId",
                        column: x => x.PropiedadIntelectualId,
                        principalTable: "PropiedadIntelectual",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProyectoProductos_Proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "Proyectos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProyectoProductos_Publicaciones_PublicacionId",
                        column: x => x.PublicacionId,
                        principalTable: "Publicaciones",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProyectoProductos_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PublicacionAutores",
                columns: table => new
                {
                    PublicacionId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicacionAutores", x => new { x.PublicacionId, x.UsuarioId });
                    table.ForeignKey(
                        name: "FK_PublicacionAutores_Publicaciones_PublicacionId",
                        column: x => x.PublicacionId,
                        principalTable: "Publicaciones",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PublicacionAutores_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PublicacionAutores_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Sustentaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrabajoGradoId = table.Column<int>(type: "int", nullable: false),
                    ActaDocumentoId = table.Column<int>(type: "int", nullable: true),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Modalidad = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                    Lugar = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Concepto = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sustentaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sustentaciones_Documentos_ActaDocumentoId",
                        column: x => x.ActaDocumentoId,
                        principalTable: "Documentos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Sustentaciones_TrabajosGrado_TrabajoGradoId",
                        column: x => x.TrabajoGradoId,
                        principalTable: "TrabajosGrado",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Sustentaciones_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TrabajoGradoJurados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrabajoGradoId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    RolJurado = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    Concepto = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrabajoGradoJurados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrabajoGradoJurados_TrabajosGrado_TrabajoGradoId",
                        column: x => x.TrabajoGradoId,
                        principalTable: "TrabajosGrado",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TrabajoGradoJurados_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TrabajoGradoJurados_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Avances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProyectoId = table.Column<int>(type: "int", nullable: false),
                    HitoId = table.Column<int>(type: "int", nullable: true),
                    AutorId = table.Column<int>(type: "int", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PorcentajeAvance = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    OrigenRegistro = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Latitud = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: true),
                    Longitud = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: true),
                    PrecisionGpsM = table.Column<decimal>(type: "decimal(7,2)", precision: 7, scale: 2, nullable: true),
                    FechaCaptura = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdCliente = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EstadoValidacion = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Avances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Avances_Hitos_HitoId",
                        column: x => x.HitoId,
                        principalTable: "Hitos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Avances_Proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "Proyectos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Avances_Usuarios_AutorId",
                        column: x => x.AutorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Avances_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AvancesAdjuntos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AvanceId = table.Column<int>(type: "int", nullable: false),
                    UrlArchivo = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    TipoMime = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvancesAdjuntos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AvancesAdjuntos_Avances_AvanceId",
                        column: x => x.AvanceId,
                        principalTable: "Avances",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AvancesAdjuntos_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AvanceValidaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AvanceId = table.Column<int>(type: "int", nullable: false),
                    ValidadorId = table.Column<int>(type: "int", nullable: false),
                    RolValidador = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Periodo = table.Column<string>(type: "char(7)", unicode: false, fixedLength: true, maxLength: 7, nullable: true),
                    Resultado = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    PuntajeRubrica = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvanceValidaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AvanceValidaciones_Avances_AvanceId",
                        column: x => x.AvanceId,
                        principalTable: "Avances",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AvanceValidaciones_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AvanceValidaciones_Usuarios_ValidadorId",
                        column: x => x.ValidadorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "EstadosProyecto",
                columns: new[] { "Id", "Activo", "ActualizadoEn", "CreadoEn", "CreadoPorId", "Nombre", "Orden" },
                values: new object[,]
                {
                    { 1, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "En elaboración", 1 },
                    { 2, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "En revisión", 2 },
                    { 3, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Devuelto", 3 },
                    { 4, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Presentado", 4 },
                    { 5, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Aprobado", 5 },
                    { 6, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "En ejecución", 6 }
                });

            migrationBuilder.InsertData(
                table: "EstadosProyecto",
                columns: new[] { "Id", "Activo", "ActualizadoEn", "CreadoEn", "CreadoPorId", "EsFinal", "Nombre", "Orden" },
                values: new object[,]
                {
                    { 7, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "Finalizado", 7 },
                    { 8, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "No aprobado", 8 }
                });

            migrationBuilder.InsertData(
                table: "EstadosProyecto",
                columns: new[] { "Id", "Activo", "ActualizadoEn", "CreadoEn", "CreadoPorId", "Nombre", "Orden" },
                values: new object[] { 9, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Suspendido", 9 });

            migrationBuilder.InsertData(
                table: "Modalidades",
                columns: new[] { "Id", "Activo", "ActualizadoEn", "CreadoEn", "CreadoPorId", "Nombre" },
                values: new object[,]
                {
                    { 1, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Presencial" },
                    { 2, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Virtual" },
                    { 3, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Híbrido" }
                });

            migrationBuilder.InsertData(
                table: "PoliticasTratamientoDatos",
                columns: new[] { "Id", "Activo", "ActualizadoEn", "CreadoEn", "CreadoPorId", "HashTexto", "TextoUrl", "Version", "VigenteDesde", "VigenteHasta" },
                values: new object[] { 1, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "/politica-datos", "1.0", new DateOnly(2026, 10, 7), null });

            migrationBuilder.InsertData(
                table: "Programas",
                columns: new[] { "Id", "Activo", "ActualizadoEn", "CreadoEn", "CreadoPorId", "Descripcion", "Nombre" },
                values: new object[,]
                {
                    { 1, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "Biotecnología y Agroindustria" },
                    { 2, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "Bioingeniería" },
                    { 3, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "Inteligencia Artificial, Modelado y Simulación" },
                    { 4, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "Sistemas Computacionales y Bioinformática" },
                    { 5, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "Ciudades Inteligentes" }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Activo", "ActualizadoEn", "CreadoEn", "CreadoPorId", "Descripcion", "Nombre" },
                values: new object[,]
                {
                    { 1, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Configura el sistema y gestiona usuarios y roles", "Admin" },
                    { 2, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Director del Centro", "Director" },
                    { 3, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Coordinador de programa", "Coordinador" },
                    { 4, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Investigador / Investigador principal", "Investigador" },
                    { 5, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Representante de un aliado", "Externo" },
                    { 6, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Estudiante (educación continua, semilleros, trabajos de grado)", "Estudiante" },
                    { 7, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Docente de educación continua", "Docente" },
                    { 8, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Responsable de logística de eventos", "Logistica" },
                    { 9, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Registro de visitas y apoyo administrativo", "Secretaria" }
                });

            migrationBuilder.InsertData(
                table: "TiposAliado",
                columns: new[] { "Id", "Activo", "ActualizadoEn", "CreadoEn", "CreadoPorId", "Nombre" },
                values: new object[,]
                {
                    { 1, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Universidad" },
                    { 2, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Empresa" },
                    { 3, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Entidad pública" },
                    { 4, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Cámara de Comercio" },
                    { 5, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "ONG" }
                });

            migrationBuilder.InsertData(
                table: "TiposConvocatoria",
                columns: new[] { "Id", "Activo", "ActualizadoEn", "CreadoEn", "CreadoPorId", "Nombre" },
                values: new object[,]
                {
                    { 1, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Interna" },
                    { 2, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Nacional" },
                    { 3, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Internacional" },
                    { 4, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Minciencias FJC" },
                    { 5, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "SGR" },
                    { 6, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Conjunta" }
                });

            migrationBuilder.InsertData(
                table: "TiposDocumento",
                columns: new[] { "Id", "Activo", "ActualizadoEn", "CreadoEn", "CreadoPorId", "Nombre" },
                values: new object[,]
                {
                    { 1, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Acta" },
                    { 2, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Plan de trabajo" },
                    { 3, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Doc. Conceptual MGA" },
                    { 4, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Informe final" },
                    { 5, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Hoja de vida" },
                    { 6, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Propuesta técnica" },
                    { 7, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Presupuesto" },
                    { 8, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Acto administrativo de aprobación" },
                    { 9, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Carta de compromiso" },
                    { 10, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Términos de referencia" },
                    { 11, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Comprobante de radicación" },
                    { 12, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Informe técnico" },
                    { 13, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Informe financiero" },
                    { 14, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Paz y salvo" },
                    { 15, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Otro" }
                });

            migrationBuilder.InsertData(
                table: "TiposEvento",
                columns: new[] { "Id", "Activo", "ActualizadoEn", "CreadoEn", "CreadoPorId", "DuracionMinutosDefecto", "Nombre" },
                values: new object[,]
                {
                    { 1, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "Congreso" },
                    { 2, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "Seminario" },
                    { 3, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "Taller" },
                    { 4, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "Innovación en Órbita" }
                });

            migrationBuilder.InsertData(
                table: "TiposFormacion",
                columns: new[] { "Id", "Activo", "ActualizadoEn", "CreadoEn", "CreadoPorId", "Nombre" },
                values: new object[,]
                {
                    { 1, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Diplomado" },
                    { 2, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Curso" },
                    { 3, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Microcertificación" },
                    { 4, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Taller" }
                });

            migrationBuilder.InsertData(
                table: "TiposHito",
                columns: new[] { "Id", "Activo", "ActualizadoEn", "CreadoEn", "CreadoPorId", "Nombre" },
                values: new object[,]
                {
                    { 1, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Entregable" },
                    { 2, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Informe" },
                    { 3, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Reunión" },
                    { 4, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Pago" },
                    { 5, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Sprint" },
                    { 6, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "MVP" }
                });

            migrationBuilder.InsertData(
                table: "TiposIntegranteSemillero",
                columns: new[] { "Id", "Activo", "ActualizadoEn", "CreadoEn", "CreadoPorId", "Nombre" },
                values: new object[,]
                {
                    { 1, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Estudiante" },
                    { 2, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Docente" },
                    { 3, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Egresado" },
                    { 4, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Joven investigador" }
                });

            migrationBuilder.InsertData(
                table: "TiposProyecto",
                columns: new[] { "Id", "Activo", "ActualizadoEn", "CreadoEn", "CreadoPorId", "Nombre" },
                values: new object[] { 1, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "I+D+i" });

            migrationBuilder.InsertData(
                table: "TiposProyecto",
                columns: new[] { "Id", "Activo", "ActualizadoEn", "CreadoEn", "CreadoPorId", "Nombre", "RequiereHitosFijos" },
                values: new object[] { 2, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Reto Empresarial", true });

            migrationBuilder.InsertData(
                table: "TiposProyecto",
                columns: new[] { "Id", "Activo", "ActualizadoEn", "CreadoEn", "CreadoPorId", "Nombre" },
                values: new object[,]
                {
                    { 3, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Consultoría" },
                    { 4, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Tesis" }
                });

            migrationBuilder.InsertData(
                table: "TiposPublicacion",
                columns: new[] { "Id", "Activo", "ActualizadoEn", "CreadoEn", "CreadoPorId", "Nombre" },
                values: new object[,]
                {
                    { 1, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Q1" },
                    { 2, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Q2" },
                    { 3, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Q3" },
                    { 4, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Q4" },
                    { 5, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Libro" },
                    { 6, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Capítulo" },
                    { 7, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Ponencia" },
                    { 8, true, null, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Tesis" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Aliados_CreadoPorId",
                table: "Aliados",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Aliados_TipoAliadoId",
                table: "Aliados",
                column: "TipoAliadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Asistencias_CreadoPorId",
                table: "Asistencias",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Asistencias_InscripcionId",
                table: "Asistencias",
                column: "InscripcionId");

            migrationBuilder.CreateIndex(
                name: "IX_Asistencias_SesionId",
                table: "Asistencias",
                column: "SesionId");

            migrationBuilder.CreateIndex(
                name: "IX_Avances_AutorId",
                table: "Avances",
                column: "AutorId");

            migrationBuilder.CreateIndex(
                name: "IX_Avances_CreadoPorId",
                table: "Avances",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Avances_HitoId",
                table: "Avances",
                column: "HitoId");

            migrationBuilder.CreateIndex(
                name: "IX_Avances_IdCliente",
                table: "Avances",
                column: "IdCliente",
                unique: true,
                filter: "[IdCliente] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Avances_ProyectoId_FechaCaptura",
                table: "Avances",
                columns: new[] { "ProyectoId", "FechaCaptura" });

            migrationBuilder.CreateIndex(
                name: "IX_AvancesAdjuntos_AvanceId",
                table: "AvancesAdjuntos",
                column: "AvanceId");

            migrationBuilder.CreateIndex(
                name: "IX_AvancesAdjuntos_CreadoPorId",
                table: "AvancesAdjuntos",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_AvanceValidaciones_AvanceId",
                table: "AvanceValidaciones",
                column: "AvanceId");

            migrationBuilder.CreateIndex(
                name: "IX_AvanceValidaciones_CreadoPorId",
                table: "AvanceValidaciones",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_AvanceValidaciones_ValidadorId",
                table: "AvanceValidaciones",
                column: "ValidadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Calificaciones_CreadoPorId",
                table: "Calificaciones",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Calificaciones_InscripcionId",
                table: "Calificaciones",
                column: "InscripcionId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificados_CodigoVerificacion",
                table: "Certificados",
                column: "CodigoVerificacion",
                unique: true,
                filter: "[CodigoVerificacion] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Certificados_CreadoPorId",
                table: "Certificados",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificados_FirmaCoordinadorId",
                table: "Certificados",
                column: "FirmaCoordinadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificados_FirmaDirectorId",
                table: "Certificados",
                column: "FirmaDirectorId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificados_InscripcionId",
                table: "Certificados",
                column: "InscripcionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConsentimientosDatos_PoliticaId",
                table: "ConsentimientosDatos",
                column: "PoliticaId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsentimientosDatos_UsuarioId",
                table: "ConsentimientosDatos",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Convocatorias_CreadoPorId",
                table: "Convocatorias",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Convocatorias_EntidadConvocanteId",
                table: "Convocatorias",
                column: "EntidadConvocanteId");

            migrationBuilder.CreateIndex(
                name: "IX_Convocatorias_TipoConvocatoriaId",
                table: "Convocatorias",
                column: "TipoConvocatoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Cronogramas_CreadoPorId",
                table: "Cronogramas",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Cronogramas_EventoId",
                table: "Cronogramas",
                column: "EventoId");

            migrationBuilder.CreateIndex(
                name: "IX_Cronogramas_FormacionId",
                table: "Cronogramas",
                column: "FormacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Cronogramas_ProyectoId",
                table: "Cronogramas",
                column: "ProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoFirmas_DocumentoId",
                table: "DocumentoFirmas",
                column: "DocumentoId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoFirmas_FirmanteId",
                table: "DocumentoFirmas",
                column: "FirmanteId");

            migrationBuilder.CreateIndex(
                name: "IX_Documentos_CreadoPorId",
                table: "Documentos",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Documentos_EntidadTipo_EntidadId",
                table: "Documentos",
                columns: new[] { "EntidadTipo", "EntidadId" });

            migrationBuilder.CreateIndex(
                name: "IX_Documentos_SubidoPorId",
                table: "Documentos",
                column: "SubidoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Documentos_TipoDocumentoId",
                table: "Documentos",
                column: "TipoDocumentoId");

            migrationBuilder.CreateIndex(
                name: "IX_EstadosProyecto_CreadoPorId",
                table: "EstadosProyecto",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_EventoAsistentes_AliadoId",
                table: "EventoAsistentes",
                column: "AliadoId");

            migrationBuilder.CreateIndex(
                name: "IX_EventoAsistentes_ConsentimientoId",
                table: "EventoAsistentes",
                column: "ConsentimientoId");

            migrationBuilder.CreateIndex(
                name: "IX_EventoAsistentes_CreadoPorId",
                table: "EventoAsistentes",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_EventoAsistentes_EventoId",
                table: "EventoAsistentes",
                column: "EventoId");

            migrationBuilder.CreateIndex(
                name: "IX_EventoAsistentes_UsuarioId",
                table: "EventoAsistentes",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Eventos_CreadoPorId",
                table: "Eventos",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Eventos_ModalidadId",
                table: "Eventos",
                column: "ModalidadId");

            migrationBuilder.CreateIndex(
                name: "IX_Eventos_ProgramaId",
                table: "Eventos",
                column: "ProgramaId");

            migrationBuilder.CreateIndex(
                name: "IX_Eventos_ResponsableLogisticaId",
                table: "Eventos",
                column: "ResponsableLogisticaId");

            migrationBuilder.CreateIndex(
                name: "IX_Eventos_TipoEventoId",
                table: "Eventos",
                column: "TipoEventoId");

            migrationBuilder.CreateIndex(
                name: "IX_FormacionDocentes_CreadoPorId",
                table: "FormacionDocentes",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_FormacionDocentes_DocenteId",
                table: "FormacionDocentes",
                column: "DocenteId");

            migrationBuilder.CreateIndex(
                name: "IX_FormacionDocentes_FormacionId",
                table: "FormacionDocentes",
                column: "FormacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Formaciones_AutorizadaPorId",
                table: "Formaciones",
                column: "AutorizadaPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Formaciones_CreadoPorId",
                table: "Formaciones",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Formaciones_ModalidadId",
                table: "Formaciones",
                column: "ModalidadId");

            migrationBuilder.CreateIndex(
                name: "IX_Formaciones_ProgramaId",
                table: "Formaciones",
                column: "ProgramaId");

            migrationBuilder.CreateIndex(
                name: "IX_Formaciones_TipoFormacionId",
                table: "Formaciones",
                column: "TipoFormacionId");

            migrationBuilder.CreateIndex(
                name: "IX_FormacionSesiones_CreadoPorId",
                table: "FormacionSesiones",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_FormacionSesiones_DocenteId",
                table: "FormacionSesiones",
                column: "DocenteId");

            migrationBuilder.CreateIndex(
                name: "IX_FormacionSesiones_FormacionId",
                table: "FormacionSesiones",
                column: "FormacionId");

            migrationBuilder.CreateIndex(
                name: "IX_FuentesConocimiento_CreadoPorId",
                table: "FuentesConocimiento",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_FuentesConocimiento_DocumentoId",
                table: "FuentesConocimiento",
                column: "DocumentoId");

            migrationBuilder.CreateIndex(
                name: "IX_FuentesConocimiento_EntidadTipo_EntidadId",
                table: "FuentesConocimiento",
                columns: new[] { "EntidadTipo", "EntidadId" });

            migrationBuilder.CreateIndex(
                name: "IX_FuentesFinanciamiento_AliadoId",
                table: "FuentesFinanciamiento",
                column: "AliadoId");

            migrationBuilder.CreateIndex(
                name: "IX_FuentesFinanciamiento_CreadoPorId",
                table: "FuentesFinanciamiento",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_FuentesFinanciamiento_ProyectoId",
                table: "FuentesFinanciamiento",
                column: "ProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_GruposInvestigacion_CreadoPorId",
                table: "GruposInvestigacion",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_GruposInvestigacion_ProgramaId",
                table: "GruposInvestigacion",
                column: "ProgramaId");

            migrationBuilder.CreateIndex(
                name: "IX_Hitos_CreadoPorId",
                table: "Hitos",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Hitos_CronogramaId",
                table: "Hitos",
                column: "CronogramaId");

            migrationBuilder.CreateIndex(
                name: "IX_Hitos_FechaLimite_Estado",
                table: "Hitos",
                columns: new[] { "FechaLimite", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_Hitos_ResponsableId",
                table: "Hitos",
                column: "ResponsableId");

            migrationBuilder.CreateIndex(
                name: "IX_Hitos_TipoHitoId",
                table: "Hitos",
                column: "TipoHitoId");

            migrationBuilder.CreateIndex(
                name: "IX_Indicadores_CreadoPorId",
                table: "Indicadores",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Indicadores_ProgramaId",
                table: "Indicadores",
                column: "ProgramaId");

            migrationBuilder.CreateIndex(
                name: "IX_IndicadorMediciones_CreadoPorId",
                table: "IndicadorMediciones",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_IndicadorMediciones_IndicadorId_Periodo",
                table: "IndicadorMediciones",
                columns: new[] { "IndicadorId", "Periodo" });

            migrationBuilder.CreateIndex(
                name: "IX_Inscripciones_ConsentimientoId",
                table: "Inscripciones",
                column: "ConsentimientoId");

            migrationBuilder.CreateIndex(
                name: "IX_Inscripciones_CreadoPorId",
                table: "Inscripciones",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Inscripciones_FormacionId_UsuarioId",
                table: "Inscripciones",
                columns: new[] { "FormacionId", "UsuarioId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inscripciones_SoportePagoDocId",
                table: "Inscripciones",
                column: "SoportePagoDocId");

            migrationBuilder.CreateIndex(
                name: "IX_Inscripciones_UsuarioId",
                table: "Inscripciones",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_LogActividades_Recurso_RecursoId",
                table: "LogActividades",
                columns: new[] { "Recurso", "RecursoId" });

            migrationBuilder.CreateIndex(
                name: "IX_LogActividades_UsuarioId_FechaUtc",
                table: "LogActividades",
                columns: new[] { "UsuarioId", "FechaUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Modalidades_CreadoPorId",
                table: "Modalidades",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_CreadoPorId",
                table: "Notificaciones",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_UsuarioId_Leida",
                table: "Notificaciones",
                columns: new[] { "UsuarioId", "Leida" });

            migrationBuilder.CreateIndex(
                name: "IX_ObjetivosEspecificos_CreadoPorId",
                table: "ObjetivosEspecificos",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_ObjetivosEspecificos_ProgramaId",
                table: "ObjetivosEspecificos",
                column: "ProgramaId");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticasTratamientoDatos_CreadoPorId",
                table: "PoliticasTratamientoDatos",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_PreferenciasNotificacion_CreadoPorId",
                table: "PreferenciasNotificacion",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_PreferenciasNotificacion_UsuarioId",
                table: "PreferenciasNotificacion",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramaAliados_AliadoId",
                table: "ProgramaAliados",
                column: "AliadoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramaAliados_CreadoPorId",
                table: "ProgramaAliados",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramaIntegrantes_CreadoPorId",
                table: "ProgramaIntegrantes",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramaIntegrantes_ProgramaId",
                table: "ProgramaIntegrantes",
                column: "ProgramaId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramaIntegrantes_UsuarioId",
                table: "ProgramaIntegrantes",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Programas_CreadoPorId",
                table: "Programas",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_PropiedadIntelectual_CreadoPorId",
                table: "PropiedadIntelectual",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_PropiedadIntelectual_ProyectoId",
                table: "PropiedadIntelectual",
                column: "ProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_PropiedadIntelectualAutores_CreadoPorId",
                table: "PropiedadIntelectualAutores",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_PropiedadIntelectualAutores_UsuarioId",
                table: "PropiedadIntelectualAutores",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_PropuestasConvocatoria_ComprobanteDocId",
                table: "PropuestasConvocatoria",
                column: "ComprobanteDocId");

            migrationBuilder.CreateIndex(
                name: "IX_PropuestasConvocatoria_ConvocatoriaId",
                table: "PropuestasConvocatoria",
                column: "ConvocatoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_PropuestasConvocatoria_CreadoPorId",
                table: "PropuestasConvocatoria",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_PropuestasConvocatoria_ProyectoId",
                table: "PropuestasConvocatoria",
                column: "ProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProyectoAliados_AliadoId",
                table: "ProyectoAliados",
                column: "AliadoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProyectoAliados_CartaCompromisoDocId",
                table: "ProyectoAliados",
                column: "CartaCompromisoDocId");

            migrationBuilder.CreateIndex(
                name: "IX_ProyectoAliados_CreadoPorId",
                table: "ProyectoAliados",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_ProyectoAliados_ProyectoId",
                table: "ProyectoAliados",
                column: "ProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProyectoAntecedentes_ConvocatoriaId",
                table: "ProyectoAntecedentes",
                column: "ConvocatoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_ProyectoAntecedentes_CreadoPorId",
                table: "ProyectoAntecedentes",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_ProyectoAntecedentes_ProyectoId",
                table: "ProyectoAntecedentes",
                column: "ProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProyectoCierres_ActaComiteDocId",
                table: "ProyectoCierres",
                column: "ActaComiteDocId");

            migrationBuilder.CreateIndex(
                name: "IX_ProyectoCierres_CreadoPorId",
                table: "ProyectoCierres",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_ProyectoCierres_InformeFinancieroDocId",
                table: "ProyectoCierres",
                column: "InformeFinancieroDocId");

            migrationBuilder.CreateIndex(
                name: "IX_ProyectoCierres_InformeTecnicoDocId",
                table: "ProyectoCierres",
                column: "InformeTecnicoDocId");

            migrationBuilder.CreateIndex(
                name: "IX_ProyectoCierres_PazYSalvoDocId",
                table: "ProyectoCierres",
                column: "PazYSalvoDocId");

            migrationBuilder.CreateIndex(
                name: "IX_ProyectoCierres_ProyectoId",
                table: "ProyectoCierres",
                column: "ProyectoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProyectoGrupos_CreadoPorId",
                table: "ProyectoGrupos",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_ProyectoGrupos_GrupoId",
                table: "ProyectoGrupos",
                column: "GrupoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProyectoIntegrantes_CreadoPorId",
                table: "ProyectoIntegrantes",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_ProyectoIntegrantes_ProyectoId",
                table: "ProyectoIntegrantes",
                column: "ProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProyectoIntegrantes_UsuarioId",
                table: "ProyectoIntegrantes",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ProyectoProductos_CreadoPorId",
                table: "ProyectoProductos",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_ProyectoProductos_DocumentoId",
                table: "ProyectoProductos",
                column: "DocumentoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProyectoProductos_PropiedadIntelectualId",
                table: "ProyectoProductos",
                column: "PropiedadIntelectualId");

            migrationBuilder.CreateIndex(
                name: "IX_ProyectoProductos_ProyectoId",
                table: "ProyectoProductos",
                column: "ProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProyectoProductos_PublicacionId",
                table: "ProyectoProductos",
                column: "PublicacionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProyectoRevisiones_CreadoPorId",
                table: "ProyectoRevisiones",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_ProyectoRevisiones_EstadoAnteriorId",
                table: "ProyectoRevisiones",
                column: "EstadoAnteriorId");

            migrationBuilder.CreateIndex(
                name: "IX_ProyectoRevisiones_EstadoNuevoId",
                table: "ProyectoRevisiones",
                column: "EstadoNuevoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProyectoRevisiones_ProyectoId",
                table: "ProyectoRevisiones",
                column: "ProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProyectoRevisiones_RevisorId",
                table: "ProyectoRevisiones",
                column: "RevisorId");

            migrationBuilder.CreateIndex(
                name: "IX_Proyectos_Codigo",
                table: "Proyectos",
                column: "Codigo",
                unique: true,
                filter: "[Codigo] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Proyectos_CreadoPorId",
                table: "Proyectos",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Proyectos_EstadoId",
                table: "Proyectos",
                column: "EstadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Proyectos_InvestigadorPrincipalId",
                table: "Proyectos",
                column: "InvestigadorPrincipalId");

            migrationBuilder.CreateIndex(
                name: "IX_Proyectos_ProgramaId",
                table: "Proyectos",
                column: "ProgramaId");

            migrationBuilder.CreateIndex(
                name: "IX_Proyectos_TipoProyectoId",
                table: "Proyectos",
                column: "TipoProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_PublicacionAutores_CreadoPorId",
                table: "PublicacionAutores",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_PublicacionAutores_UsuarioId",
                table: "PublicacionAutores",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Publicaciones_CreadoPorId",
                table: "Publicaciones",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Publicaciones_ProgramaId",
                table: "Publicaciones",
                column: "ProgramaId");

            migrationBuilder.CreateIndex(
                name: "IX_Publicaciones_ProyectoId",
                table: "Publicaciones",
                column: "ProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_Publicaciones_TipoPublicacionId",
                table: "Publicaciones",
                column: "TipoPublicacionId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_CreadoPorId",
                table: "RefreshTokens",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_TokenHash",
                table: "RefreshTokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UsuarioId",
                table: "RefreshTokens",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportesGenerados_CreadoPorId",
                table: "ReportesGenerados",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportesGenerados_DocumentoId",
                table: "ReportesGenerados",
                column: "DocumentoId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportesGenerados_GeneradoPorId",
                table: "ReportesGenerados",
                column: "GeneradoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportesGenerados_ProgramaId",
                table: "ReportesGenerados",
                column: "ProgramaId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_CreadoPorId",
                table: "Roles",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Nombre",
                table: "Roles",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SemilleroIntegrantes_AprobadoPorId",
                table: "SemilleroIntegrantes",
                column: "AprobadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_SemilleroIntegrantes_CreadoPorId",
                table: "SemilleroIntegrantes",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_SemilleroIntegrantes_ProyectoId",
                table: "SemilleroIntegrantes",
                column: "ProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_SemilleroIntegrantes_SemilleroId",
                table: "SemilleroIntegrantes",
                column: "SemilleroId");

            migrationBuilder.CreateIndex(
                name: "IX_SemilleroIntegrantes_TipoIntegranteId",
                table: "SemilleroIntegrantes",
                column: "TipoIntegranteId");

            migrationBuilder.CreateIndex(
                name: "IX_SemilleroIntegrantes_UsuarioId",
                table: "SemilleroIntegrantes",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Semilleros_CoordinadorId",
                table: "Semilleros",
                column: "CoordinadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Semilleros_CreadoPorId",
                table: "Semilleros",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Semilleros_ProgramaId",
                table: "Semilleros",
                column: "ProgramaId");

            migrationBuilder.CreateIndex(
                name: "IX_Sustentaciones_ActaDocumentoId",
                table: "Sustentaciones",
                column: "ActaDocumentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Sustentaciones_CreadoPorId",
                table: "Sustentaciones",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Sustentaciones_TrabajoGradoId",
                table: "Sustentaciones",
                column: "TrabajoGradoId");

            migrationBuilder.CreateIndex(
                name: "IX_TiposAliado_CreadoPorId",
                table: "TiposAliado",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_TiposConvocatoria_CreadoPorId",
                table: "TiposConvocatoria",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_TiposDocumento_CreadoPorId",
                table: "TiposDocumento",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_TiposEvento_CreadoPorId",
                table: "TiposEvento",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_TiposFormacion_CreadoPorId",
                table: "TiposFormacion",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_TiposHito_CreadoPorId",
                table: "TiposHito",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_TiposIntegranteSemillero_CreadoPorId",
                table: "TiposIntegranteSemillero",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_TiposProyecto_CreadoPorId",
                table: "TiposProyecto",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_TiposPublicacion_CreadoPorId",
                table: "TiposPublicacion",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_TrabajoGradoJurados_CreadoPorId",
                table: "TrabajoGradoJurados",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_TrabajoGradoJurados_TrabajoGradoId",
                table: "TrabajoGradoJurados",
                column: "TrabajoGradoId");

            migrationBuilder.CreateIndex(
                name: "IX_TrabajoGradoJurados_UsuarioId",
                table: "TrabajoGradoJurados",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_TrabajosGrado_CodirectorId",
                table: "TrabajosGrado",
                column: "CodirectorId");

            migrationBuilder.CreateIndex(
                name: "IX_TrabajosGrado_CreadoPorId",
                table: "TrabajosGrado",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_TrabajosGrado_DirectorId",
                table: "TrabajosGrado",
                column: "DirectorId");

            migrationBuilder.CreateIndex(
                name: "IX_TrabajosGrado_EstudianteId",
                table: "TrabajosGrado",
                column: "EstudianteId");

            migrationBuilder.CreateIndex(
                name: "IX_TrabajosGrado_ProgramaId",
                table: "TrabajosGrado",
                column: "ProgramaId");

            migrationBuilder.CreateIndex(
                name: "IX_TrabajosGrado_PropuestaDocId",
                table: "TrabajosGrado",
                column: "PropuestaDocId");

            migrationBuilder.CreateIndex(
                name: "IX_TrabajosGrado_ProyectoId",
                table: "TrabajosGrado",
                column: "ProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioRoles_CreadoPorId",
                table: "UsuarioRoles",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioRoles_RolId",
                table: "UsuarioRoles",
                column: "RolId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_AliadoId",
                table: "Usuarios",
                column: "AliadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_CreadoPorId",
                table: "Usuarios",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Visitas_ConsentimientoId",
                table: "Visitas",
                column: "ConsentimientoId");

            migrationBuilder.CreateIndex(
                name: "IX_Visitas_CreadoPorId",
                table: "Visitas",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Visitas_FechaHoraEntrada",
                table: "Visitas",
                column: "FechaHoraEntrada");

            migrationBuilder.CreateIndex(
                name: "IX_Visitas_RegistradaPorId",
                table: "Visitas",
                column: "RegistradaPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Visitas_UsuarioId",
                table: "Visitas",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Aliados_TiposAliado_TipoAliadoId",
                table: "Aliados",
                column: "TipoAliadoId",
                principalTable: "TiposAliado",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Aliados_Usuarios_CreadoPorId",
                table: "Aliados",
                column: "CreadoPorId",
                principalTable: "Usuarios",
                principalColumn: "Id");

            // Tablas inmutables (regla 8): se bloquean UPDATE y DELETE con un trigger INSTEAD OF.
            foreach (var tabla in new[] { "LogActividades", "DocumentoFirmas", "ConsentimientosDatos" })
            {
                migrationBuilder.Sql($"""
                    CREATE TRIGGER [dbo].[TR_{tabla}_Inmutable] ON [dbo].[{tabla}]
                    INSTEAD OF UPDATE, DELETE
                    AS
                    BEGIN
                        SET NOCOUNT ON;
                        THROW 50001, N'La tabla {tabla} es inmutable: no admite UPDATE ni DELETE.', 1;
                    END
                    """);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var tabla in new[] { "LogActividades", "DocumentoFirmas", "ConsentimientosDatos" })
                migrationBuilder.Sql($"DROP TRIGGER IF EXISTS [dbo].[TR_{tabla}_Inmutable];");

            migrationBuilder.DropForeignKey(
                name: "FK_Aliados_TiposAliado_TipoAliadoId",
                table: "Aliados");

            migrationBuilder.DropForeignKey(
                name: "FK_Aliados_Usuarios_CreadoPorId",
                table: "Aliados");

            migrationBuilder.DropTable(
                name: "Asistencias");

            migrationBuilder.DropTable(
                name: "AvancesAdjuntos");

            migrationBuilder.DropTable(
                name: "AvanceValidaciones");

            migrationBuilder.DropTable(
                name: "Calificaciones");

            migrationBuilder.DropTable(
                name: "Certificados");

            migrationBuilder.DropTable(
                name: "DocumentoFirmas");

            migrationBuilder.DropTable(
                name: "EventoAsistentes");

            migrationBuilder.DropTable(
                name: "FormacionDocentes");

            migrationBuilder.DropTable(
                name: "FuentesConocimiento");

            migrationBuilder.DropTable(
                name: "FuentesFinanciamiento");

            migrationBuilder.DropTable(
                name: "IndicadorMediciones");

            migrationBuilder.DropTable(
                name: "LogActividades");

            migrationBuilder.DropTable(
                name: "Notificaciones");

            migrationBuilder.DropTable(
                name: "ObjetivosEspecificos");

            migrationBuilder.DropTable(
                name: "PreferenciasNotificacion");

            migrationBuilder.DropTable(
                name: "ProgramaAliados");

            migrationBuilder.DropTable(
                name: "ProgramaIntegrantes");

            migrationBuilder.DropTable(
                name: "PropiedadIntelectualAutores");

            migrationBuilder.DropTable(
                name: "PropuestasConvocatoria");

            migrationBuilder.DropTable(
                name: "ProyectoAliados");

            migrationBuilder.DropTable(
                name: "ProyectoAntecedentes");

            migrationBuilder.DropTable(
                name: "ProyectoCierres");

            migrationBuilder.DropTable(
                name: "ProyectoGrupos");

            migrationBuilder.DropTable(
                name: "ProyectoIntegrantes");

            migrationBuilder.DropTable(
                name: "ProyectoProductos");

            migrationBuilder.DropTable(
                name: "ProyectoRevisiones");

            migrationBuilder.DropTable(
                name: "PublicacionAutores");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "ReportesGenerados");

            migrationBuilder.DropTable(
                name: "SemilleroIntegrantes");

            migrationBuilder.DropTable(
                name: "Sustentaciones");

            migrationBuilder.DropTable(
                name: "TrabajoGradoJurados");

            migrationBuilder.DropTable(
                name: "UsuarioRoles");

            migrationBuilder.DropTable(
                name: "Visitas");

            migrationBuilder.DropTable(
                name: "FormacionSesiones");

            migrationBuilder.DropTable(
                name: "Avances");

            migrationBuilder.DropTable(
                name: "Inscripciones");

            migrationBuilder.DropTable(
                name: "Indicadores");

            migrationBuilder.DropTable(
                name: "Convocatorias");

            migrationBuilder.DropTable(
                name: "GruposInvestigacion");

            migrationBuilder.DropTable(
                name: "PropiedadIntelectual");

            migrationBuilder.DropTable(
                name: "Publicaciones");

            migrationBuilder.DropTable(
                name: "Semilleros");

            migrationBuilder.DropTable(
                name: "TiposIntegranteSemillero");

            migrationBuilder.DropTable(
                name: "TrabajosGrado");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Hitos");

            migrationBuilder.DropTable(
                name: "ConsentimientosDatos");

            migrationBuilder.DropTable(
                name: "TiposConvocatoria");

            migrationBuilder.DropTable(
                name: "TiposPublicacion");

            migrationBuilder.DropTable(
                name: "Documentos");

            migrationBuilder.DropTable(
                name: "Cronogramas");

            migrationBuilder.DropTable(
                name: "TiposHito");

            migrationBuilder.DropTable(
                name: "PoliticasTratamientoDatos");

            migrationBuilder.DropTable(
                name: "TiposDocumento");

            migrationBuilder.DropTable(
                name: "Eventos");

            migrationBuilder.DropTable(
                name: "Formaciones");

            migrationBuilder.DropTable(
                name: "Proyectos");

            migrationBuilder.DropTable(
                name: "TiposEvento");

            migrationBuilder.DropTable(
                name: "Modalidades");

            migrationBuilder.DropTable(
                name: "TiposFormacion");

            migrationBuilder.DropTable(
                name: "EstadosProyecto");

            migrationBuilder.DropTable(
                name: "Programas");

            migrationBuilder.DropTable(
                name: "TiposProyecto");

            migrationBuilder.DropTable(
                name: "TiposAliado");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Aliados");
        }
    }
}
