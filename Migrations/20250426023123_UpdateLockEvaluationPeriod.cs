using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chuyendoiso.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLockEvaluationPeriod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LockAttachment",
                table: "EvaluationPeriod",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LockReason",
                table: "EvaluationPeriod",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LockAttachment",
                table: "EvaluationPeriod");

            migrationBuilder.DropColumn(
                name: "LockReason",
                table: "EvaluationPeriod");
        }
    }
}
