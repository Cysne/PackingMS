using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PackingService.Api.Migrations
{
    /// <inheritdoc />
    public partial class AjusteModelo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderBoxes",
                table: "OrderBoxes");

            migrationBuilder.DropIndex(
                name: "IX_OrderBoxes_OrderId",
                table: "OrderBoxes");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "OrderBoxes");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Products",
                newName: "ProductId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Orders",
                newName: "OrderId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "OrderItems",
                newName: "OrderItemId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Boxes",
                newName: "BoxId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderBoxes",
                table: "OrderBoxes",
                columns: new[] { "OrderId", "BoxId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderBoxes",
                table: "OrderBoxes");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "Products",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "OrderId",
                table: "Orders",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "OrderItemId",
                table: "OrderItems",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "BoxId",
                table: "Boxes",
                newName: "Id");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "OrderBoxes",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderBoxes",
                table: "OrderBoxes",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_OrderBoxes_OrderId",
                table: "OrderBoxes",
                column: "OrderId");
        }
    }
}
