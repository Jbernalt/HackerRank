using Microsoft.EntityFrameworkCore.Migrations;

namespace HackerRank.Migrations
{
    public partial class inttodouble : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MonthlyRating",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<double>(
                name: "DailyRating",
                table: "UserStats",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MonthlyRating",
                table: "UserStats",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AlterColumn<double>(
                name: "GroupRating",
                table: "Group",
                type: "float",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DailyRating",
                table: "UserStats");

            migrationBuilder.DropColumn(
                name: "MonthlyRating",
                table: "UserStats");

            migrationBuilder.AlterColumn<int>(
                name: "GroupRating",
                table: "Group",
                type: "int",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AddColumn<double>(
                name: "MonthlyRating",
                table: "AspNetUsers",
                type: "float",
                nullable: true);
        }
    }
}
