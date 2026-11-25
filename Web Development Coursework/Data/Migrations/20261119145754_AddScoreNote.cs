using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web_Development_Coursework.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddScoreNote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ScoreNote",
                table: "Listings",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ScoreNote",
                table: "Listings");
        }
    }
}
