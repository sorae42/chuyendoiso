using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chuyendoiso.Migrations
{
    /// <inheritdoc />
    public partial class AddDeclineFieldsToReviewAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeclineAttachmentPath",
                table: "ReviewResult");

            migrationBuilder.DropColumn(
                name: "DeclineReason",
                table: "ReviewResult");

            migrationBuilder.DropColumn(
                name: "DeclinedAt",
                table: "ReviewResult");

            migrationBuilder.DropColumn(
                name: "IsDeclined",
                table: "ReviewResult");

            migrationBuilder.AddColumn<string>(
                name: "DeclineAttachmentPath",
                table: "ReviewAssignment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeclineReason",
                table: "ReviewAssignment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeclinedAt",
                table: "ReviewAssignment",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeclined",
                table: "ReviewAssignment",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeclineAttachmentPath",
                table: "ReviewAssignment");

            migrationBuilder.DropColumn(
                name: "DeclineReason",
                table: "ReviewAssignment");

            migrationBuilder.DropColumn(
                name: "DeclinedAt",
                table: "ReviewAssignment");

            migrationBuilder.DropColumn(
                name: "IsDeclined",
                table: "ReviewAssignment");

            migrationBuilder.AddColumn<string>(
                name: "DeclineAttachmentPath",
                table: "ReviewResult",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeclineReason",
                table: "ReviewResult",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeclinedAt",
                table: "ReviewResult",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeclined",
                table: "ReviewResult",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
