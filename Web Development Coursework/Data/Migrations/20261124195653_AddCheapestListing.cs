using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web_Development_Coursework.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCheapestListing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CheapestPrice",
                table: "MarketPrices",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CheapestTitle",
                table: "MarketPrices",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CheapestUrl",
                table: "MarketPrices",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheapestPrice",
                table: "MarketPrices");

            migrationBuilder.DropColumn(
                name: "CheapestTitle",
                table: "MarketPrices");

            migrationBuilder.DropColumn(
                name: "CheapestUrl",
                table: "MarketPrices");
        }
    }
}
