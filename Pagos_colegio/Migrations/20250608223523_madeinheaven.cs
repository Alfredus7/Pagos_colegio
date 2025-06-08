using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pagos_colegio_web.Migrations
{
    /// <inheritdoc />
    public partial class madeinheaven : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pagos_Tarifas_TarifaId",
                table: "Pagos");

            migrationBuilder.AlterColumn<int>(
                name: "TarifaId",
                table: "Pagos",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<decimal>(
                name: "Descuento",
                table: "Pagos",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPago",
                table: "Pagos",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaInscripcion",
                table: "Estudiantes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "TarifaId",
                table: "Estudiantes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Estudiantes_TarifaId",
                table: "Estudiantes",
                column: "TarifaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Estudiantes_Tarifas_TarifaId",
                table: "Estudiantes",
                column: "TarifaId",
                principalTable: "Tarifas",
                principalColumn: "TarifaId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Pagos_Tarifas_TarifaId",
                table: "Pagos",
                column: "TarifaId",
                principalTable: "Tarifas",
                principalColumn: "TarifaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Estudiantes_Tarifas_TarifaId",
                table: "Estudiantes");

            migrationBuilder.DropForeignKey(
                name: "FK_Pagos_Tarifas_TarifaId",
                table: "Pagos");

            migrationBuilder.DropIndex(
                name: "IX_Estudiantes_TarifaId",
                table: "Estudiantes");

            migrationBuilder.DropColumn(
                name: "Descuento",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "TotalPago",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "FechaInscripcion",
                table: "Estudiantes");

            migrationBuilder.DropColumn(
                name: "TarifaId",
                table: "Estudiantes");

            migrationBuilder.AlterColumn<int>(
                name: "TarifaId",
                table: "Pagos",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Pagos_Tarifas_TarifaId",
                table: "Pagos",
                column: "TarifaId",
                principalTable: "Tarifas",
                principalColumn: "TarifaId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
