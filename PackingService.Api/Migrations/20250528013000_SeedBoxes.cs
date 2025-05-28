using Microsoft.EntityFrameworkCore.Migrations;

namespace PackingService.Api.Migrations
{
    public partial class SeedBoxes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Boxes",
                columns: new[] { "BoxType", "Height", "Width", "Length" },
                values: new object[,]
                {
                    { "Box 1", 30m, 40m, 80m },
                    { "Box 2", 80m, 50m, 40m },
                    { "Box 3", 50m, 80m, 60m }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Boxes",
                keyColumn: "BoxType",
                keyValues: new object[] { "Box 1", "Box 2", "Box 3" }
            );
        }
    }
}
