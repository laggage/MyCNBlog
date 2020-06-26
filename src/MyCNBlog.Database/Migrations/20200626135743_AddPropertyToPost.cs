using Microsoft.EntityFrameworkCore.Migrations;

namespace MyCNBlog.Database.Migrations
{
    public partial class AddPropertyToPost : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlogId",
                table: "BlogUser");

            migrationBuilder.AddColumn<bool>(
                name: "IsTopMost",
                table: "Post",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TopMostOrder",
                table: "Post",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTopMost",
                table: "Post");

            migrationBuilder.DropColumn(
                name: "TopMostOrder",
                table: "Post");

            migrationBuilder.AddColumn<int>(
                name: "BlogId",
                table: "BlogUser",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
