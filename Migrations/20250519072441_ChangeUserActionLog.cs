using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chuyendoiso.Migrations
{
    /// <inheritdoc />
    public partial class ChangeUserActionLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActionLogs_Auth_UserId",
                table: "ActionLogs");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "ActionLogs",
                newName: "RelatedUserId");

            migrationBuilder.RenameIndex(
                name: "IX_ActionLogs_UserId",
                table: "ActionLogs",
                newName: "IX_ActionLogs_RelatedUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ActionLogs_Auth_RelatedUserId",
                table: "ActionLogs",
                column: "RelatedUserId",
                principalTable: "Auth",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActionLogs_Auth_RelatedUserId",
                table: "ActionLogs");

            migrationBuilder.RenameColumn(
                name: "RelatedUserId",
                table: "ActionLogs",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_ActionLogs_RelatedUserId",
                table: "ActionLogs",
                newName: "IX_ActionLogs_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ActionLogs_Auth_UserId",
                table: "ActionLogs",
                column: "UserId",
                principalTable: "Auth",
                principalColumn: "Id");
        }
    }
}
