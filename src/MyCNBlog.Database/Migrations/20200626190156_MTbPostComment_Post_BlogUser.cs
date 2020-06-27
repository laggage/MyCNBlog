using Microsoft.EntityFrameworkCore.Migrations;

namespace MyCNBlog.Database.Migrations
{
    public partial class MTbPostComment_Post_BlogUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PostComment_UserId",
                table: "PostComment",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PostComment_BlogUser_UserId",
                table: "PostComment",
                column: "UserId",
                principalTable: "BlogUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PostComment_BlogUser_UserId",
                table: "PostComment");

            migrationBuilder.DropIndex(
                name: "IX_PostComment_UserId",
                table: "PostComment");
        }
    }
}
