using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalizacionAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Camas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Unidad = table.Column<string>(type: "text", nullable: false),
                    Tipo = table.Column<string>(type: "text", nullable: false),
                    Codigo = table.Column<string>(type: "text", nullable: false),
                    Estado = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Camas", x => x.Id);
                    table.UniqueConstraint("AK_Camas_Codigo", x => x.Codigo);
                });

            migrationBuilder.CreateTable(
                name: "Pacientes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    FechaNacimiento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Codigo = table.Column<string>(type: "text", nullable: false),
                    Estado = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pacientes", x => x.Id);
                    table.UniqueConstraint("AK_Pacientes_Codigo", x => x.Codigo);
                });

            migrationBuilder.CreateTable(
                name: "Admisiones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PacienteCodigo = table.Column<string>(type: "text", nullable: false),
                    CamaCodigo = table.Column<string>(type: "text", nullable: false),
                    FechaIngreso = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaEgreso = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Especialidad = table.Column<string>(type: "text", nullable: false),
                    Codigo = table.Column<string>(type: "text", nullable: false),
                    Estado = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admisiones", x => x.Id);
                    table.UniqueConstraint("AK_Admisiones_Codigo", x => x.Codigo);
                    table.ForeignKey(
                        name: "FK_Admisiones_Camas_CamaCodigo",
                        column: x => x.CamaCodigo,
                        principalTable: "Camas",
                        principalColumn: "Codigo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Admisiones_Pacientes_PacienteCodigo",
                        column: x => x.PacienteCodigo,
                        principalTable: "Pacientes",
                        principalColumn: "Codigo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tratamientos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AdmisionCodigo = table.Column<string>(type: "text", nullable: false),
                    NombreMedicamento = table.Column<string>(type: "text", nullable: false),
                    Dosis = table.Column<string>(type: "text", nullable: false),
                    DuracionDias = table.Column<int>(type: "integer", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Codigo = table.Column<string>(type: "text", nullable: false),
                    Estado = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tratamientos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tratamientos_Admisiones_AdmisionCodigo",
                        column: x => x.AdmisionCodigo,
                        principalTable: "Admisiones",
                        principalColumn: "Codigo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Admisiones_CamaCodigo",
                table: "Admisiones",
                column: "CamaCodigo");

            migrationBuilder.CreateIndex(
                name: "IX_Admisiones_Codigo",
                table: "Admisiones",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Admisiones_PacienteCodigo",
                table: "Admisiones",
                column: "PacienteCodigo");

            migrationBuilder.CreateIndex(
                name: "IX_Camas_Codigo",
                table: "Camas",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pacientes_Codigo",
                table: "Pacientes",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tratamientos_AdmisionCodigo",
                table: "Tratamientos",
                column: "AdmisionCodigo");

            migrationBuilder.CreateIndex(
                name: "IX_Tratamientos_Codigo",
                table: "Tratamientos",
                column: "Codigo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tratamientos");

            migrationBuilder.DropTable(
                name: "Admisiones");

            migrationBuilder.DropTable(
                name: "Camas");

            migrationBuilder.DropTable(
                name: "Pacientes");
        }
    }
}
