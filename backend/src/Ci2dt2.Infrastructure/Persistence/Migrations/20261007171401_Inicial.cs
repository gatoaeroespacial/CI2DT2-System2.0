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
                name: "EstadosProyecto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    EsFinal = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadosProyecto", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PoliticasTratamientoDatos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Version = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    TextoUrl = table.Column<string>(type: "varchar(400)", unicode: false, maxLength: 400, nullable: true),
                    HashTexto = table.Column<string>(type: "char(64)", unicode: false, fixedLength: true, maxLength: 64, nullable: true),
                    VigenteDesde = table.Column<DateOnly>(type: "date", nullable: false),
                    VigenteHasta = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoliticasTratamientoDatos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Programas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Programas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TiposAliado",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposAliado", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TiposConvocatoria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposConvocatoria", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TiposDocumento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposDocumento", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TiposHito",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposHito", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TiposProyecto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: false),
                    RequiereHitosFijos = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposProyecto", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ObjetivosEspecificos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramaId = table.Column<int>(type: "int", nullable: false),
                    Descripcion = table.Column<string>(type: "varchar(max)", unicode: false, nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObjetivosEspecificos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ObjetivosEspecificos_Programas_ProgramaId",
                        column: x => x.ProgramaId,
                        principalTable: "Programas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Aliados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TipoAliadoId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    Nit = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    Pais = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: true),
                    Ciudad = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: true),
                    Sector = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: true),
                    RepresentanteLegal = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: true),
                    Email = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Aliados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Aliados_TiposAliado_TipoAliadoId",
                        column: x => x.TipoAliadoId,
                        principalTable: "TiposAliado",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Convocatorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "varchar(250)", unicode: false, maxLength: 250, nullable: false),
                    Numero = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    TipoConvocatoriaId = table.Column<int>(type: "int", nullable: false),
                    EntidadConvocanteId = table.Column<int>(type: "int", nullable: true),
                    Plataforma = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Url = table.Column<string>(type: "varchar(400)", unicode: false, maxLength: 400, nullable: true),
                    FechaApertura = table.Column<DateOnly>(type: "date", nullable: true),
                    FechaCierre = table.Column<DateOnly>(type: "date", nullable: true),
                    Moneda = table.Column<string>(type: "char(3)", unicode: false, fixedLength: true, maxLength: 3, nullable: true),
                    MontoMaximo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Convocatorias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Convocatorias_Aliados_EntidadConvocanteId",
                        column: x => x.EntidadConvocanteId,
                        principalTable: "Aliados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Convocatorias_TiposConvocatoria_TipoConvocatoriaId",
                        column: x => x.TipoConvocatoriaId,
                        principalTable: "TiposConvocatoria",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProgramaAliados",
                columns: table => new
                {
                    ProgramaId = table.Column<int>(type: "int", nullable: false),
                    AliadoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramaAliados", x => new { x.ProgramaId, x.AliadoId });
                    table.ForeignKey(
                        name: "FK_ProgramaAliados_Aliados_AliadoId",
                        column: x => x.AliadoId,
                        principalTable: "Aliados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProgramaAliados_Programas_ProgramaId",
                        column: x => x.ProgramaId,
                        principalTable: "Programas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: false),
                    PasswordHash = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    Nombres = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Apellidos = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    TipoDocumento = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    NumeroDocumento = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    Telefono = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    Cargo = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Dependencia = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: true),
                    AliadoId = table.Column<int>(type: "int", nullable: true),
                    CvLacUrl = table.Column<string>(type: "varchar(300)", unicode: false, maxLength: 300, nullable: true),
                    Orcid = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    ExpoPushToken = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    TieneAcceso = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Usuarios_Aliados_AliadoId",
                        column: x => x.AliadoId,
                        principalTable: "Aliados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConsentimientosDatos",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PoliticaId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: true),
                    Correo = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: true),
                    Contexto = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    Aceptado = table.Column<bool>(type: "bit", nullable: false),
                    FechaAceptacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpOrigen = table.Column<string>(type: "varchar(45)", unicode: false, maxLength: 45, nullable: true),
                    HashEvidencia = table.Column<string>(type: "char(64)", unicode: false, fixedLength: true, maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsentimientosDatos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsentimientosDatos_PoliticasTratamientoDatos_PoliticaId",
                        column: x => x.PoliticaId,
                        principalTable: "PoliticasTratamientoDatos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConsentimientosDatos_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Documentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TipoDocumentoId = table.Column<int>(type: "int", nullable: false),
                    EntidadTipo = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    EntidadId = table.Column<int>(type: "int", nullable: false),
                    NombreArchivo = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    UrlArchivo = table.Column<string>(type: "varchar(400)", unicode: false, maxLength: 400, nullable: false),
                    HashSha256 = table.Column<string>(type: "char(64)", unicode: false, fixedLength: true, maxLength: 64, nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    EsPublico = table.Column<bool>(type: "bit", nullable: false),
                    SubidoPorId = table.Column<int>(type: "int", nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documentos_TiposDocumento_TipoDocumentoId",
                        column: x => x.TipoDocumentoId,
                        principalTable: "TiposDocumento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documentos_Usuarios_SubidoPorId",
                        column: x => x.SubidoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LogActividades",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: true),
                    IpOrigen = table.Column<string>(type: "varchar(45)", unicode: false, maxLength: 45, nullable: true),
                    Recurso = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: false),
                    RecursoId = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: true),
                    Accion = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    Ruta = table.Column<string>(type: "varchar(300)", unicode: false, maxLength: 300, nullable: true),
                    EstadoHttp = table.Column<int>(type: "int", nullable: true),
                    ValoresAnteriores = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    ValoresNuevos = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
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
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notificaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    Titulo = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    Mensaje = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    ReferenciaTipo = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: true),
                    ReferenciaId = table.Column<int>(type: "int", nullable: true),
                    Leida = table.Column<bool>(type: "bit", nullable: false),
                    EnviadaPush = table.Column<bool>(type: "bit", nullable: false),
                    FechaEnvioPush = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EnviadaEmail = table.Column<bool>(type: "bit", nullable: false),
                    FechaEnvioEmail = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Intentos = table.Column<int>(type: "int", nullable: false),
                    UltimoError = table.Column<string>(type: "varchar(300)", unicode: false, maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notificaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notificaciones_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProgramaIntegrantes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramaId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    RolEnPrograma = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    FechaInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaFin = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramaIntegrantes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgramaIntegrantes_Programas_ProgramaId",
                        column: x => x.ProgramaId,
                        principalTable: "Programas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProgramaIntegrantes_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Proyectos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    Titulo = table.Column<string>(type: "varchar(300)", unicode: false, maxLength: 300, nullable: false),
                    Resumen = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    ProgramaId = table.Column<int>(type: "int", nullable: false),
                    TipoProyectoId = table.Column<int>(type: "int", nullable: false),
                    EstadoId = table.Column<int>(type: "int", nullable: false),
                    InvestigadorPrincipalId = table.Column<int>(type: "int", nullable: false),
                    FechaInicio = table.Column<DateOnly>(type: "date", nullable: true),
                    FechaFin = table.Column<DateOnly>(type: "date", nullable: true),
                    PorcentajeAvance = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Moneda = table.Column<string>(type: "char(3)", unicode: false, fixedLength: true, maxLength: 3, nullable: false),
                    PresupuestoTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    TasaCambio = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    PresupuestoTotalCop = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proyectos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Proyectos_EstadosProyecto_EstadoId",
                        column: x => x.EstadoId,
                        principalTable: "EstadosProyecto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Proyectos_Programas_ProgramaId",
                        column: x => x.ProgramaId,
                        principalTable: "Programas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Proyectos_TiposProyecto_TipoProyectoId",
                        column: x => x.TipoProyectoId,
                        principalTable: "TiposProyecto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Proyectos_Usuarios_InvestigadorPrincipalId",
                        column: x => x.InvestigadorPrincipalId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    TokenHash = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    ExpiraEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevocadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IpOrigen = table.Column<string>(type: "varchar(45)", unicode: false, maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UsuarioRoles",
                columns: table => new
                {
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    RolId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioRoles", x => new { x.UsuarioId, x.RolId });
                    table.ForeignKey(
                        name: "FK_UsuarioRoles_Roles_RolId",
                        column: x => x.RolId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UsuarioRoles_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Cronogramas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: false),
                    ProyectoId = table.Column<int>(type: "int", nullable: true),
                    FormacionId = table.Column<int>(type: "int", nullable: true),
                    EventoId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cronogramas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cronogramas_Proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "Proyectos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FuentesFinanciamiento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProyectoId = table.Column<int>(type: "int", nullable: false),
                    AliadoId = table.Column<int>(type: "int", nullable: true),
                    TipoFuente = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    TipoAporte = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    Moneda = table.Column<string>(type: "char(3)", unicode: false, fixedLength: true, maxLength: 3, nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TasaCambio = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    MontoCop = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Estado = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FuentesFinanciamiento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FuentesFinanciamiento_Aliados_AliadoId",
                        column: x => x.AliadoId,
                        principalTable: "Aliados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FuentesFinanciamiento_Proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "Proyectos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                    IdPostulacionExterna = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: true),
                    CodigoMga = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    MontoSolicitado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Moneda = table.Column<string>(type: "char(3)", unicode: false, fixedLength: true, maxLength: 3, nullable: true),
                    TasaCambioEstimada = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    MontoSolicitadoCop = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Resultado = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    Puntaje = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    FaseExterna = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: true),
                    FechaResultado = table.Column<DateOnly>(type: "date", nullable: true),
                    ComprobanteDocId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropuestasConvocatoria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropuestasConvocatoria_Convocatorias_ConvocatoriaId",
                        column: x => x.ConvocatoriaId,
                        principalTable: "Convocatorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PropuestasConvocatoria_Documentos_ComprobanteDocId",
                        column: x => x.ComprobanteDocId,
                        principalTable: "Documentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PropuestasConvocatoria_Proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "Proyectos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProyectoIntegrantes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProyectoId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    RolEnProyecto = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    FechaInicio = table.Column<DateOnly>(type: "date", nullable: true),
                    FechaFin = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProyectoIntegrantes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProyectoIntegrantes_Proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "Proyectos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProyectoIntegrantes_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProyectoRevisiones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProyectoId = table.Column<int>(type: "int", nullable: false),
                    RevisorId = table.Column<int>(type: "int", nullable: false),
                    Etapa = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    Decision = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    EstadoAnteriorId = table.Column<int>(type: "int", nullable: true),
                    EstadoNuevoId = table.Column<int>(type: "int", nullable: true),
                    Observaciones = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProyectoRevisiones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProyectoRevisiones_EstadosProyecto_EstadoAnteriorId",
                        column: x => x.EstadoAnteriorId,
                        principalTable: "EstadosProyecto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProyectoRevisiones_EstadosProyecto_EstadoNuevoId",
                        column: x => x.EstadoNuevoId,
                        principalTable: "EstadosProyecto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProyectoRevisiones_Proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "Proyectos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProyectoRevisiones_Usuarios_RevisorId",
                        column: x => x.RevisorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Hitos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CronogramaId = table.Column<int>(type: "int", nullable: false),
                    TipoHitoId = table.Column<int>(type: "int", nullable: false),
                    Titulo = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    FechaLimite = table.Column<DateOnly>(type: "date", nullable: false),
                    AlertarDias = table.Column<int>(type: "int", nullable: false),
                    ResponsableId = table.Column<int>(type: "int", nullable: true),
                    Estado = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: false),
                    FechaCompletado = table.Column<DateOnly>(type: "date", nullable: true),
                    Orden = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hitos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Hitos_Cronogramas_CronogramaId",
                        column: x => x.CronogramaId,
                        principalTable: "Cronogramas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Hitos_TiposHito_TipoHitoId",
                        column: x => x.TipoHitoId,
                        principalTable: "TiposHito",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Hitos_Usuarios_ResponsableId",
                        column: x => x.ResponsableId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                    Titulo = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    PorcentajeAvance = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    OrigenRegistro = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    Latitud = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: true),
                    Longitud = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: true),
                    PrecisionGpsM = table.Column<decimal>(type: "decimal(7,2)", precision: 7, scale: 2, nullable: true),
                    FechaCaptura = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdCliente = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EstadoValidacion = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Avances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Avances_Hitos_HitoId",
                        column: x => x.HitoId,
                        principalTable: "Hitos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Avances_Proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "Proyectos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Avances_Usuarios_AutorId",
                        column: x => x.AutorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AvancesAdjuntos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AvanceId = table.Column<int>(type: "int", nullable: false),
                    UrlArchivo = table.Column<string>(type: "varchar(400)", unicode: false, maxLength: 400, nullable: false),
                    TipoMime = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvancesAdjuntos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AvancesAdjuntos_Avances_AvanceId",
                        column: x => x.AvanceId,
                        principalTable: "Avances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AvanceValidaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AvanceId = table.Column<int>(type: "int", nullable: false),
                    ValidadorId = table.Column<int>(type: "int", nullable: false),
                    RolValidador = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: true),
                    Periodo = table.Column<string>(type: "char(7)", unicode: false, fixedLength: true, maxLength: 7, nullable: true),
                    Resultado = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: false),
                    PuntajeRubrica = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Observaciones = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvanceValidaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AvanceValidaciones_Avances_AvanceId",
                        column: x => x.AvanceId,
                        principalTable: "Avances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AvanceValidaciones_Usuarios_ValidadorId",
                        column: x => x.ValidadorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "EstadosProyecto",
                columns: new[] { "Id", "EsFinal", "Nombre", "Orden" },
                values: new object[,]
                {
                    { 1, false, "En elaboración", 1 },
                    { 2, false, "Presentado", 2 },
                    { 3, false, "Aprobado", 3 },
                    { 4, false, "En ejecución", 4 },
                    { 5, true, "Finalizado", 5 },
                    { 6, true, "No aprobado", 6 }
                });

            migrationBuilder.InsertData(
                table: "PoliticasTratamientoDatos",
                columns: new[] { "Id", "HashTexto", "TextoUrl", "Version", "VigenteDesde", "VigenteHasta" },
                values: new object[] { 1, null, "/politica-datos", "1.0", new DateOnly(2026, 10, 7), null });

            migrationBuilder.InsertData(
                table: "Programas",
                columns: new[] { "Id", "Descripcion", "Nombre" },
                values: new object[,]
                {
                    { 1, null, "Biotecnología y Agroindustria" },
                    { 2, null, "Bioingeniería" },
                    { 3, null, "Inteligencia Artificial, Modelado y Simulación" },
                    { 4, null, "Sistemas Computacionales y Bioinformática" },
                    { 5, null, "Ciudades Inteligentes" }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Descripcion", "Nombre" },
                values: new object[,]
                {
                    { 1, "Configura el sistema y gestiona usuarios y roles", "Administrador" },
                    { 2, "Director del Centro", "Director" },
                    { 3, "Coordinador de programa", "Coordinador" },
                    { 4, "Investigador / Investigador principal", "Investigador" },
                    { 5, "Representante de un aliado", "Externo" }
                });

            migrationBuilder.InsertData(
                table: "TiposAliado",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1, "Universidad" },
                    { 2, "Empresa" },
                    { 3, "Entidad pública" },
                    { 4, "ONG / Fundación" },
                    { 5, "Organismo internacional" },
                    { 6, "Gremio / Clúster" }
                });

            migrationBuilder.InsertData(
                table: "TiposConvocatoria",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1, "Interna (VIP UCaldas)" },
                    { 2, "Externa nacional" },
                    { 3, "Externa internacional" },
                    { 4, "Minciencias – Fondo FJC" },
                    { 5, "Minciencias – SGR (Fondo CTeI)" },
                    { 6, "Conjunta" }
                });

            migrationBuilder.InsertData(
                table: "TiposDocumento",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1, "Propuesta técnica" },
                    { 2, "Presupuesto" },
                    { 3, "Acto administrativo de aprobación" },
                    { 4, "Carta de compromiso" },
                    { 5, "Plan de trabajo" },
                    { 6, "Términos de referencia" },
                    { 7, "Comprobante de radicación" },
                    { 8, "Documento conceptual MGA" },
                    { 9, "Informe técnico" },
                    { 10, "Informe financiero" },
                    { 11, "Acta de comité" },
                    { 12, "Paz y salvo" },
                    { 13, "Otro" }
                });

            migrationBuilder.InsertData(
                table: "TiposHito",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1, "Entregable" },
                    { 2, "Informe de avance" },
                    { 3, "Informe final" },
                    { 4, "Reunión técnica" },
                    { 5, "Desembolso / pago" },
                    { 6, "Sprint" },
                    { 7, "Entrega de MVP" }
                });

            migrationBuilder.InsertData(
                table: "TiposProyecto",
                columns: new[] { "Id", "Nombre", "RequiereHitosFijos" },
                values: new object[,]
                {
                    { 1, "Investigación básica", false },
                    { 2, "Investigación aplicada", false },
                    { 3, "Desarrollo tecnológico", false },
                    { 4, "Innovación", false },
                    { 5, "Reto empresarial", true },
                    { 6, "Consultoría / servicio", false }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Aliados_TipoAliadoId",
                table: "Aliados",
                column: "TipoAliadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Avances_AutorId",
                table: "Avances",
                column: "AutorId");

            migrationBuilder.CreateIndex(
                name: "IX_Avances_HitoId",
                table: "Avances",
                column: "HitoId");

            migrationBuilder.CreateIndex(
                name: "IX_Avances_ProyectoId",
                table: "Avances",
                column: "ProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_AvancesAdjuntos_AvanceId",
                table: "AvancesAdjuntos",
                column: "AvanceId");

            migrationBuilder.CreateIndex(
                name: "IX_AvanceValidaciones_AvanceId",
                table: "AvanceValidaciones",
                column: "AvanceId");

            migrationBuilder.CreateIndex(
                name: "IX_AvanceValidaciones_ValidadorId",
                table: "AvanceValidaciones",
                column: "ValidadorId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsentimientosDatos_PoliticaId",
                table: "ConsentimientosDatos",
                column: "PoliticaId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsentimientosDatos_UsuarioId",
                table: "ConsentimientosDatos",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Convocatorias_EntidadConvocanteId",
                table: "Convocatorias",
                column: "EntidadConvocanteId");

            migrationBuilder.CreateIndex(
                name: "IX_Convocatorias_TipoConvocatoriaId",
                table: "Convocatorias",
                column: "TipoConvocatoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Cronogramas_ProyectoId",
                table: "Cronogramas",
                column: "ProyectoId");

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
                name: "IX_FuentesFinanciamiento_AliadoId",
                table: "FuentesFinanciamiento",
                column: "AliadoId");

            migrationBuilder.CreateIndex(
                name: "IX_FuentesFinanciamiento_ProyectoId",
                table: "FuentesFinanciamiento",
                column: "ProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_Hitos_CronogramaId",
                table: "Hitos",
                column: "CronogramaId");

            migrationBuilder.CreateIndex(
                name: "IX_Hitos_ResponsableId",
                table: "Hitos",
                column: "ResponsableId");

            migrationBuilder.CreateIndex(
                name: "IX_Hitos_TipoHitoId",
                table: "Hitos",
                column: "TipoHitoId");

            migrationBuilder.CreateIndex(
                name: "IX_LogActividades_FechaUtc",
                table: "LogActividades",
                column: "FechaUtc");

            migrationBuilder.CreateIndex(
                name: "IX_LogActividades_UsuarioId",
                table: "LogActividades",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_UsuarioId",
                table: "Notificaciones",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ObjetivosEspecificos_ProgramaId",
                table: "ObjetivosEspecificos",
                column: "ProgramaId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramaAliados_AliadoId",
                table: "ProgramaAliados",
                column: "AliadoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramaIntegrantes_ProgramaId",
                table: "ProgramaIntegrantes",
                column: "ProgramaId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramaIntegrantes_UsuarioId_ProgramaId",
                table: "ProgramaIntegrantes",
                columns: new[] { "UsuarioId", "ProgramaId" });

            migrationBuilder.CreateIndex(
                name: "IX_PropuestasConvocatoria_ComprobanteDocId",
                table: "PropuestasConvocatoria",
                column: "ComprobanteDocId");

            migrationBuilder.CreateIndex(
                name: "IX_PropuestasConvocatoria_ConvocatoriaId",
                table: "PropuestasConvocatoria",
                column: "ConvocatoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_PropuestasConvocatoria_ProyectoId",
                table: "PropuestasConvocatoria",
                column: "ProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProyectoIntegrantes_ProyectoId",
                table: "ProyectoIntegrantes",
                column: "ProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProyectoIntegrantes_UsuarioId_ProyectoId",
                table: "ProyectoIntegrantes",
                columns: new[] { "UsuarioId", "ProyectoId" });

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
                unique: true);

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
                name: "IX_RefreshTokens_TokenHash",
                table: "RefreshTokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UsuarioId",
                table: "RefreshTokens",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Nombre",
                table: "Roles",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioRoles_RolId",
                table: "UsuarioRoles",
                column: "RolId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_AliadoId",
                table: "Usuarios",
                column: "AliadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AvancesAdjuntos");

            migrationBuilder.DropTable(
                name: "AvanceValidaciones");

            migrationBuilder.DropTable(
                name: "ConsentimientosDatos");

            migrationBuilder.DropTable(
                name: "FuentesFinanciamiento");

            migrationBuilder.DropTable(
                name: "LogActividades");

            migrationBuilder.DropTable(
                name: "Notificaciones");

            migrationBuilder.DropTable(
                name: "ObjetivosEspecificos");

            migrationBuilder.DropTable(
                name: "ProgramaAliados");

            migrationBuilder.DropTable(
                name: "ProgramaIntegrantes");

            migrationBuilder.DropTable(
                name: "PropuestasConvocatoria");

            migrationBuilder.DropTable(
                name: "ProyectoIntegrantes");

            migrationBuilder.DropTable(
                name: "ProyectoRevisiones");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "UsuarioRoles");

            migrationBuilder.DropTable(
                name: "Avances");

            migrationBuilder.DropTable(
                name: "PoliticasTratamientoDatos");

            migrationBuilder.DropTable(
                name: "Convocatorias");

            migrationBuilder.DropTable(
                name: "Documentos");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Hitos");

            migrationBuilder.DropTable(
                name: "TiposConvocatoria");

            migrationBuilder.DropTable(
                name: "TiposDocumento");

            migrationBuilder.DropTable(
                name: "Cronogramas");

            migrationBuilder.DropTable(
                name: "TiposHito");

            migrationBuilder.DropTable(
                name: "Proyectos");

            migrationBuilder.DropTable(
                name: "EstadosProyecto");

            migrationBuilder.DropTable(
                name: "Programas");

            migrationBuilder.DropTable(
                name: "TiposProyecto");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Aliados");

            migrationBuilder.DropTable(
                name: "TiposAliado");
        }
    }
}
