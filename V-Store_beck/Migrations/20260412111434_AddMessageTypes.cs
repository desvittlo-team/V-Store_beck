using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V_Store_beck.Migrations
{
    /// <inheritdoc />
    public partial class AddMessageTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "Messages",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ItemId",
                table: "Messages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ItemId",
                table: "Messages",
                column: "ItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_InventoryItems_ItemId",
                table: "Messages",
                column: "ItemId",
                principalTable: "InventoryItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_InventoryItems_ItemId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_ItemId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "Amount",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "ItemId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Messages");
        }
    }
}
