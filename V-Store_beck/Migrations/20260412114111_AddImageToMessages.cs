using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V_Store_beck.Migrations
{
    /// <inheritdoc />
    public partial class AddImageToMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageFileName",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageFileName",
                table: "GlobalMessages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "GlobalMessages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageFileName",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "ImageFileName",
                table: "GlobalMessages");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "GlobalMessages");
        }
    }
}
