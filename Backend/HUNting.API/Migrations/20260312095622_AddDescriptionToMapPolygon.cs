using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HUNting.API.Migrations
{
    /// <inheritdoc />
    public partial class AddDescriptionToMapPolygon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "MapPolygons",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "MapPolygons");
        }
    }
}
