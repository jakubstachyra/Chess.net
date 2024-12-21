using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RankingUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RankingsUsers_AspNetUsers_UserId",
                table: "RankingsUsers");

            migrationBuilder.DropColumn(
                name: "UserID",
                table: "RankingsUsers");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "RankingsUsers",
                newName: "UserID");

            migrationBuilder.RenameIndex(
                name: "IX_RankingsUsers_UserId",
                table: "RankingsUsers",
                newName: "IX_RankingsUsers_UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_RankingsUsers_AspNetUsers_UserID",
                table: "RankingsUsers",
                column: "UserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RankingsUsers_AspNetUsers_UserID",
                table: "RankingsUsers");

            migrationBuilder.RenameColumn(
                name: "UserID",
                table: "RankingsUsers",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_RankingsUsers_UserID",
                table: "RankingsUsers",
                newName: "IX_RankingsUsers_UserId");

            migrationBuilder.AddColumn<int>(
                name: "UserID",
                table: "RankingsUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_RankingsUsers_AspNetUsers_UserId",
                table: "RankingsUsers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
