using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace chuyendoiso.Migrations
{
    /// <inheritdoc />
    public partial class EvaluationPeriod_Unit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Unit",
                table: "Auth");

            migrationBuilder.AddColumn<int>(
                name: "EvaluationPeriodId",
                table: "ParentCriteria",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UnitId",
                table: "Auth",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EvaluationPeriod",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluationPeriod", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Unit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Unit", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EvaluationUnit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EvaluationPeriodId = table.Column<int>(type: "integer", nullable: false),
                    UnitId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluationUnit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EvaluationUnit_EvaluationPeriod_EvaluationPeriodId",
                        column: x => x.EvaluationPeriodId,
                        principalTable: "EvaluationPeriod",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EvaluationUnit_Unit_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Unit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParentCriteria_EvaluationPeriodId",
                table: "ParentCriteria",
                column: "EvaluationPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_Auth_UnitId",
                table: "Auth",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationUnit_EvaluationPeriodId",
                table: "EvaluationUnit",
                column: "EvaluationPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationUnit_UnitId",
                table: "EvaluationUnit",
                column: "UnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Auth_Unit_UnitId",
                table: "Auth",
                column: "UnitId",
                principalTable: "Unit",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ParentCriteria_EvaluationPeriod_EvaluationPeriodId",
                table: "ParentCriteria",
                column: "EvaluationPeriodId",
                principalTable: "EvaluationPeriod",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Auth_Unit_UnitId",
                table: "Auth");

            migrationBuilder.DropForeignKey(
                name: "FK_ParentCriteria_EvaluationPeriod_EvaluationPeriodId",
                table: "ParentCriteria");

            migrationBuilder.DropTable(
                name: "EvaluationUnit");

            migrationBuilder.DropTable(
                name: "EvaluationPeriod");

            migrationBuilder.DropTable(
                name: "Unit");

            migrationBuilder.DropIndex(
                name: "IX_ParentCriteria_EvaluationPeriodId",
                table: "ParentCriteria");

            migrationBuilder.DropIndex(
                name: "IX_Auth_UnitId",
                table: "Auth");

            migrationBuilder.DropColumn(
                name: "EvaluationPeriodId",
                table: "ParentCriteria");

            migrationBuilder.DropColumn(
                name: "UnitId",
                table: "Auth");

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "Auth",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
