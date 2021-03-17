using Microsoft.EntityFrameworkCore.Migrations;

namespace HackerRank.Migrations
{
    public partial class UpdatedAchievement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Level",
                table: "Achievement");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "Achievement",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
