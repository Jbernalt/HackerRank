using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HackerRank.Migrations
{
    public partial class NewImageStoring : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageBinaryData",
                table: "Achievement");

            migrationBuilder.AddColumn<string>(
                name: "Image",
                table: "Achievement",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Image",
                table: "Achievement");

            migrationBuilder.AddColumn<byte[]>(
                name: "ImageBinaryData",
                table: "Achievement",
                type: "varbinary(max)",
                nullable: true);
        }
    }
}
