using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chuyendoiso.Migrations
{
    /// <inheritdoc />
    public partial class AddTwoFactorFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTwoFactorEnabled",
                table: "Auth",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OtpCode",
                table: "Auth",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OtpExpires",
                table: "Auth",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TrustedDeviceToken",
                table: "Auth",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TrustedUntil",
                table: "Auth",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTwoFactorEnabled",
                table: "Auth");

            migrationBuilder.DropColumn(
                name: "OtpCode",
                table: "Auth");

            migrationBuilder.DropColumn(
                name: "OtpExpires",
                table: "Auth");

            migrationBuilder.DropColumn(
                name: "TrustedDeviceToken",
                table: "Auth");

            migrationBuilder.DropColumn(
                name: "TrustedUntil",
                table: "Auth");
        }
    }
}
