using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chuyendoiso.Migrations
{
    /// <inheritdoc />
    public partial class AddUserActionLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "ActionLogs",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActionLogs_UserId",
                table: "ActionLogs",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ActionLogs_Auth_UserId",
                table: "ActionLogs",
                column: "UserId",
                principalTable: "Auth",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActionLogs_Auth_UserId",
                table: "ActionLogs");

            migrationBuilder.DropIndex(
                name: "IX_ActionLogs_UserId",
                table: "ActionLogs");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ActionLogs");
        }
    }
}
