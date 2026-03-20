using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V_Store_beck.Migrations
{
    /// <inheritdoc />
    public partial class AddScreenshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Author",
                table: "Screenshots");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Screenshots");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Screenshots",
                newName: "FileName");

            migrationBuilder.RenameColumn(
                name: "Rating",
                table: "Screenshots",
                newName: "UserId");

            migrationBuilder.AddColumn<string>(
                name: "Caption",
                table: "Screenshots",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Likes",
                table: "Screenshots",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Screenshots_UserId",
                table: "Screenshots",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Screenshots_Users_UserId",
                table: "Screenshots",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Screenshots_Users_UserId",
                table: "Screenshots");

            migrationBuilder.DropIndex(
                name: "IX_Screenshots_UserId",
                table: "Screenshots");

            migrationBuilder.DropColumn(
                name: "Caption",
                table: "Screenshots");

            migrationBuilder.DropColumn(
                name: "Likes",
                table: "Screenshots");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Screenshots",
                newName: "Rating");

            migrationBuilder.RenameColumn(
                name: "FileName",
                table: "Screenshots",
                newName: "Title");

            migrationBuilder.AddColumn<string>(
                name: "Author",
                table: "Screenshots",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Screenshots",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
