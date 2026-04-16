using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FruitsInventorySystem.Migrations
{
    /// <inheritdoc />
    public partial class AddQualityToDistributor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FruitTransactions");

            migrationBuilder.CreateTable(
                name: "Distributors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DistributorId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DistributorName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FruitId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FruitName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Quality = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BoxCount = table.Column<int>(type: "int", nullable: true),
                    PricePerBox = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    GrandTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RemainingAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PaymentType = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Distributors", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Distributors");

            migrationBuilder.CreateTable(
                name: "FruitTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GrandTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RemainingAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SupplierName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FruitTransactions", x => x.Id);
                });
        }
    }
}
