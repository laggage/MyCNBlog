using Microsoft.EntityFrameworkCore.Migrations;

namespace MyCNBlog.Database.Migrations
{
    public partial class MdTb_AddViewCount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "Post",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "Post");
        }
    }
}
