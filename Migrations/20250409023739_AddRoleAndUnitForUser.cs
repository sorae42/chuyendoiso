using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chuyendoiso.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleAndUnitForUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EvaluatedAt",
                table: "SubCriteria",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnitEvaluate",
                table: "SubCriteria",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Auth",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "Auth",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EvaluatedAt",
                table: "SubCriteria");

            migrationBuilder.DropColumn(
                name: "UnitEvaluate",
                table: "SubCriteria");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "Auth");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "Auth");
        }
    }
}
