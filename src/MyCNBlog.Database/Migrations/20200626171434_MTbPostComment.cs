using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MyCNBlog.Database.Migrations
{
    public partial class MTbPostComment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PostComment_Post_ReplayedPostId",
                table: "PostComment");

            migrationBuilder.DropForeignKey(
                name: "FK_PostComment_BlogUser_ReplayedUserId",
                table: "PostComment");

            migrationBuilder.DropIndex(
                name: "IX_PostComment_ReplayedPostId",
                table: "PostComment");

            migrationBuilder.DropIndex(
                name: "IX_PostComment_ReplayedUserId",
                table: "PostComment");

            migrationBuilder.DropColumn(
                name: "ReplayedPostId",
                table: "PostComment");

            migrationBuilder.DropColumn(
                name: "ReplayedUserId",
                table: "PostComment");

            migrationBuilder.AddColumn<DateTime>(
                name: "PostedTime",
                table: "PostComment",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "RepliedPostId",
                table: "PostComment",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RepliedUserId",
                table: "PostComment",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PostComment_RepliedPostId",
                table: "PostComment",
                column: "RepliedPostId");

            migrationBuilder.CreateIndex(
                name: "IX_PostComment_RepliedUserId",
                table: "PostComment",
                column: "RepliedUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PostComment_Post_RepliedPostId",
                table: "PostComment",
                column: "RepliedPostId",
                principalTable: "Post",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PostComment_BlogUser_RepliedUserId",
                table: "PostComment",
                column: "RepliedUserId",
                principalTable: "BlogUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PostComment_Post_RepliedPostId",
                table: "PostComment");

            migrationBuilder.DropForeignKey(
                name: "FK_PostComment_BlogUser_RepliedUserId",
                table: "PostComment");

            migrationBuilder.DropIndex(
                name: "IX_PostComment_RepliedPostId",
                table: "PostComment");

            migrationBuilder.DropIndex(
                name: "IX_PostComment_RepliedUserId",
                table: "PostComment");

            migrationBuilder.DropColumn(
                name: "PostedTime",
                table: "PostComment");

            migrationBuilder.DropColumn(
                name: "RepliedPostId",
                table: "PostComment");

            migrationBuilder.DropColumn(
                name: "RepliedUserId",
                table: "PostComment");

            migrationBuilder.AddColumn<int>(
                name: "ReplayedPostId",
                table: "PostComment",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReplayedUserId",
                table: "PostComment",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PostComment_ReplayedPostId",
                table: "PostComment",
                column: "ReplayedPostId");

            migrationBuilder.CreateIndex(
                name: "IX_PostComment_ReplayedUserId",
                table: "PostComment",
                column: "ReplayedUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PostComment_Post_ReplayedPostId",
                table: "PostComment",
                column: "ReplayedPostId",
                principalTable: "Post",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PostComment_BlogUser_ReplayedUserId",
                table: "PostComment",
                column: "ReplayedUserId",
                principalTable: "BlogUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
