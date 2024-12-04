using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RepositoryPattern : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Games_GameMode_GameModeID",
                table: "Games");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "Moves",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "GameModeID",
                table: "Games",
                newName: "GameModeId");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "Games",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Games_GameModeID",
                table: "Games",
                newName: "IX_Games_GameModeId");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "GameMode",
                newName: "Id");

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_Games_GameMode_GameModeId",
                table: "Games",
                column: "GameModeId",
                principalTable: "GameMode",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Games_GameMode_GameModeId",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Moves",
                newName: "ID");

            migrationBuilder.RenameColumn(
                name: "GameModeId",
                table: "Games",
                newName: "GameModeID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Games",
                newName: "ID");

            migrationBuilder.RenameIndex(
                name: "IX_Games_GameModeId",
                table: "Games",
                newName: "IX_Games_GameModeID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "GameMode",
                newName: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Games_GameMode_GameModeID",
                table: "Games",
                column: "GameModeID",
                principalTable: "GameMode",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
