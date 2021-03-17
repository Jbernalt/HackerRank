using Microsoft.EntityFrameworkCore.Migrations;

namespace HackerRank.Data.Migrations
{
    public partial class UserTransactionUserIdFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserTransaction_AspNetUsers_UserId",
                table: "UserTransaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserTransaction",
                table: "UserTransaction");

            migrationBuilder.DropIndex(
                name: "IX_UserTransaction_UserId",
                table: "UserTransaction");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "UserTransaction");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserTransaction",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserTransaction",
                table: "UserTransaction",
                columns: new[] { "UserId", "TransactionId", "FetchDate" });

            migrationBuilder.AddForeignKey(
                name: "FK_UserTransaction_AspNetUsers_UserId",
                table: "UserTransaction",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserTransaction_AspNetUsers_UserId",
                table: "UserTransaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserTransaction",
                table: "UserTransaction");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserTransaction",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "UserTransaction",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserTransaction",
                table: "UserTransaction",
                columns: new[] { "Id", "TransactionId", "FetchDate" });

            migrationBuilder.CreateIndex(
                name: "IX_UserTransaction_UserId",
                table: "UserTransaction",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserTransaction_AspNetUsers_UserId",
                table: "UserTransaction",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
