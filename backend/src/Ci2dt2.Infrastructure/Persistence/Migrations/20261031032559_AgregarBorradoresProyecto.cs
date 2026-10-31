using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ci2dt2.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarBorradoresProyecto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BorradoresProyecto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    TituloTentativo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ProgramaId = table.Column<int>(type: "int", nullable: true),
                    LineaInvestigacion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ConvocatoriaNombre = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    ConvocatoriaMetadataJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocumentoTdrId = table.Column<int>(type: "int", nullable: true),
                    EstadoAvance = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BorradoresProyecto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BorradoresProyecto_Documentos_DocumentoTdrId",
                        column: x => x.DocumentoTdrId,
                        principalTable: "Documentos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BorradoresProyecto_Programas_ProgramaId",
                        column: x => x.ProgramaId,
                        principalTable: "Programas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BorradoresProyecto_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BorradoresProyecto_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BorradorProyectoMensajes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BorradorProyectoId = table.Column<int>(type: "int", nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Texto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TipoMensaje = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SeccionDestino = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MetadatosJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BorradorProyectoMensajes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BorradorProyectoMensajes_BorradoresProyecto_BorradorProyectoId",
                        column: x => x.BorradorProyectoId,
                        principalTable: "BorradoresProyecto",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BorradorProyectoSecciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BorradorProyectoId = table.Column<int>(type: "int", nullable: false),
                    CodigoSeccion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TituloSeccion = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Contenido = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ObservacionesIA = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreadoPorId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BorradorProyectoSecciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BorradorProyectoSecciones_BorradoresProyecto_BorradorProyectoId",
                        column: x => x.BorradorProyectoId,
                        principalTable: "BorradoresProyecto",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BorradorProyectoSecciones_Usuarios_CreadoPorId",
                        column: x => x.CreadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BorradoresProyecto_CreadoPorId",
                table: "BorradoresProyecto",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_BorradoresProyecto_DocumentoTdrId",
                table: "BorradoresProyecto",
                column: "DocumentoTdrId");

            migrationBuilder.CreateIndex(
                name: "IX_BorradoresProyecto_ProgramaId",
                table: "BorradoresProyecto",
                column: "ProgramaId");

            migrationBuilder.CreateIndex(
                name: "IX_BorradoresProyecto_UsuarioId",
                table: "BorradoresProyecto",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_BorradorProyectoMensajes_BorradorProyectoId",
                table: "BorradorProyectoMensajes",
                column: "BorradorProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_BorradorProyectoSecciones_BorradorProyectoId_CodigoSeccion",
                table: "BorradorProyectoSecciones",
                columns: new[] { "BorradorProyectoId", "CodigoSeccion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BorradorProyectoSecciones_CreadoPorId",
                table: "BorradorProyectoSecciones",
                column: "CreadoPorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BorradorProyectoMensajes");

            migrationBuilder.DropTable(
                name: "BorradorProyectoSecciones");

            migrationBuilder.DropTable(
                name: "BorradoresProyecto");
        }
    }
}
