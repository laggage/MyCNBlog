using Microsoft.EntityFrameworkCore.Migrations;

namespace MyCNBlog.Database.Migrations
{
    public partial class MTbPostComment_Post_BlogUser_FK : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PostComment_BlogUser_RepliedUserId",
                table: "PostComment");

            migrationBuilder.AddForeignKey(
                name: "FK_PostComment_BlogUser_RepliedUserId",
                table: "PostComment",
                column: "RepliedUserId",
                principalTable: "BlogUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PostComment_BlogUser_RepliedUserId",
                table: "PostComment");

            migrationBuilder.AddForeignKey(
                name: "FK_PostComment_BlogUser_RepliedUserId",
                table: "PostComment",
                column: "RepliedUserId",
                principalTable: "BlogUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
