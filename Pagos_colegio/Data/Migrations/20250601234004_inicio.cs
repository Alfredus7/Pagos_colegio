using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pagos_colegio_web.Data.Migrations
{
    /// <inheritdoc />
    public partial class inicio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cuentas",
                columns: table => new
                {
                    ID_CUENTA = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PIN = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cuentas", x => x.ID_CUENTA);
                });

            migrationBuilder.CreateTable(
                name: "Tarifas",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FechaIni = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tarifas", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Familias",
                columns: table => new
                {
                    ID_FAMILIA = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApellidoMaterno = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ApellidoPaterno = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ID_CUENTA = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Familias", x => x.ID_FAMILIA);
                    table.ForeignKey(
                        name: "FK_Familias_Cuentas_ID_CUENTA",
                        column: x => x.ID_CUENTA,
                        principalTable: "Cuentas",
                        principalColumn: "ID_CUENTA",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Estudiantes",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ID_FAMILIA = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Estudiantes", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Estudiantes_Familias_ID_FAMILIA",
                        column: x => x.ID_FAMILIA,
                        principalTable: "Familias",
                        principalColumn: "ID_FAMILIA",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pagos",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FechaPago = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ID_ESTUDIANTE = table.Column<int>(type: "int", nullable: false),
                    ID_TARIFA = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagos", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Pagos_Estudiantes_ID_ESTUDIANTE",
                        column: x => x.ID_ESTUDIANTE,
                        principalTable: "Estudiantes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Pagos_Tarifas_ID_TARIFA",
                        column: x => x.ID_TARIFA,
                        principalTable: "Tarifas",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Recibos",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descripcion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ID_PAGO = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recibos", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Recibos_Pagos_ID_PAGO",
                        column: x => x.ID_PAGO,
                        principalTable: "Pagos",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Estudiantes_ID_FAMILIA",
                table: "Estudiantes",
                column: "ID_FAMILIA");

            migrationBuilder.CreateIndex(
                name: "IX_Familias_ID_CUENTA",
                table: "Familias",
                column: "ID_CUENTA",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_ID_ESTUDIANTE",
                table: "Pagos",
                column: "ID_ESTUDIANTE");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_ID_TARIFA",
                table: "Pagos",
                column: "ID_TARIFA");

            migrationBuilder.CreateIndex(
                name: "IX_Recibos_ID_PAGO",
                table: "Recibos",
                column: "ID_PAGO",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Recibos");

            migrationBuilder.DropTable(
                name: "Pagos");

            migrationBuilder.DropTable(
                name: "Estudiantes");

            migrationBuilder.DropTable(
                name: "Tarifas");

            migrationBuilder.DropTable(
                name: "Familias");

            migrationBuilder.DropTable(
                name: "Cuentas");
        }
    }
}
