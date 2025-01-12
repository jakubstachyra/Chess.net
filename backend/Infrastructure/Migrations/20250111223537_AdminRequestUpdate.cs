using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdminRequestUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdminRequests_AspNetUsers_UserID",
                table: "AdminRequests");

            migrationBuilder.DropIndex(
                name: "IX_AdminRequests_UserID",
                table: "AdminRequests");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AdminRequests_UserID",
                table: "AdminRequests",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_AdminRequests_AspNetUsers_UserID",
                table: "AdminRequests",
                column: "UserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
