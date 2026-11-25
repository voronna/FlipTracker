using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web_Development_Coursework.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMarketOffers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MarketOffers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MarketPriceId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Shipping = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketOffers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarketOffers_MarketPrices_MarketPriceId",
                        column: x => x.MarketPriceId,
                        principalTable: "MarketPrices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MarketOffers_MarketPriceId",
                table: "MarketOffers",
                column: "MarketPriceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MarketOffers");
        }
    }
}
