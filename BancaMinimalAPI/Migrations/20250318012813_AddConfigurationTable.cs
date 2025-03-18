using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BancaMinimalAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddConfigurationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Configurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InterestRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MinimumPaymentRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configurations", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Configurations",
                columns: new[] { "Id", "InterestRate", "LastUpdated", "MinimumPaymentRate" },
                values: new object[] { 1, 25.0m, new DateTime(2025, 3, 18, 1, 28, 12, 798, DateTimeKind.Utc).AddTicks(3580), 5.0m });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Configurations");
        }
    }
}
