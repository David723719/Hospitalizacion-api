using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalizacionAPI.Migrations
{
    /// <inheritdoc />
    public partial class FixCamasColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodigoLogistica",
                table: "Camas",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EstadoOperativo",
                table: "Camas",
                type: "text",
                nullable: false,
                defaultValue: "Disponible");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaRegistro",
                table: "Camas",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodigoLogistica",
                table: "Camas");

            migrationBuilder.DropColumn(
                name: "EstadoOperativo",
                table: "Camas");

            migrationBuilder.DropColumn(
                name: "FechaRegistro",
                table: "Camas");
        }
    }
}
