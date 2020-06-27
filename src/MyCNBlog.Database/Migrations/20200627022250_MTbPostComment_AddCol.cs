using Microsoft.EntityFrameworkCore.Migrations;

namespace MyCNBlog.Database.Migrations
{
    public partial class MTbPostComment_AddCol : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RepliedCommentId",
                table: "PostComment",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PostComment_RepliedCommentId",
                table: "PostComment",
                column: "RepliedCommentId");

            migrationBuilder.AddForeignKey(
                name: "FK_PostComment_PostComment_RepliedCommentId",
                table: "PostComment",
                column: "RepliedCommentId",
                principalTable: "PostComment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PostComment_PostComment_RepliedCommentId",
                table: "PostComment");

            migrationBuilder.DropIndex(
                name: "IX_PostComment_RepliedCommentId",
                table: "PostComment");

            migrationBuilder.DropColumn(
                name: "RepliedCommentId",
                table: "PostComment");
        }
    }
}
