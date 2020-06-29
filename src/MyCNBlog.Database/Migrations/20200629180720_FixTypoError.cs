using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MyCNBlog.Database.Migrations
{
    public partial class FixTypoError : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RegiserDate",
                table: "BlogUser");

            migrationBuilder.AddColumn<DateTime>(
                name: "RegisterDate",
                table: "BlogUser",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RegisterDate",
                table: "BlogUser");

            migrationBuilder.AddColumn<DateTime>(
                name: "RegiserDate",
                table: "BlogUser",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
