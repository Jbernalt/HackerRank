using Microsoft.EntityFrameworkCore.Migrations;

namespace HackerRank.Migrations
{
    public partial class UpdatedUserAndAchievementModels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserStatsId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfActions",
                table: "Achievement",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "UserStats",
                columns: table => new
                {
                    UserStatsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TotalCommits = table.Column<int>(type: "int", nullable: false),
                    TotalMergeRequests = table.Column<int>(type: "int", nullable: false),
                    TotalIssuesSolved = table.Column<int>(type: "int", nullable: false),
                    TotalIssuesCreated = table.Column<int>(type: "int", nullable: false),
                    TotalComments = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserStats", x => x.UserStatsId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_UserStatsId",
                table: "AspNetUsers",
                column: "UserStatsId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_UserStats_UserStatsId",
                table: "AspNetUsers",
                column: "UserStatsId",
                principalTable: "UserStats",
                principalColumn: "UserStatsId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_UserStats_UserStatsId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "UserStats");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_UserStatsId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UserStatsId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "NumberOfActions",
                table: "Achievement");
        }
    }
}
