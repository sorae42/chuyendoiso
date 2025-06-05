using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chuyendoiso.Migrations
{
    /// <inheritdoc />
    public partial class AddRejectionAndDeclineFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.AddColumn<bool>(
                name: "IsRejected",
                table: "FinalReviewResult",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RejectAttachmentPath",
                table: "FinalReviewResult",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectReason",
                table: "FinalReviewResult",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedAt",
                table: "FinalReviewResult",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropColumn(
                name: "IsRejected",
                table: "FinalReviewResult");

            migrationBuilder.DropColumn(
                name: "RejectAttachmentPath",
                table: "FinalReviewResult");

            migrationBuilder.DropColumn(
                name: "RejectReason",
                table: "FinalReviewResult");

            migrationBuilder.DropColumn(
                name: "RejectedAt",
                table: "FinalReviewResult");
        }
    }
}
