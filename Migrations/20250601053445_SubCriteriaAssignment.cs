using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace chuyendoiso.Migrations
{
    /// <inheritdoc />
    public partial class SubCriteriaAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SubCriteriaAssignment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SubCriteriaId = table.Column<int>(type: "integer", nullable: false),
                    EvaluationPeriodId = table.Column<int>(type: "integer", nullable: false),
                    UnitId = table.Column<int>(type: "integer", nullable: false),
                    Score = table.Column<float>(type: "real", nullable: true),
                    EvaluatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    EvidenceInfo = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubCriteriaAssignment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubCriteriaAssignment_EvaluationPeriod_EvaluationPeriodId",
                        column: x => x.EvaluationPeriodId,
                        principalTable: "EvaluationPeriod",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubCriteriaAssignment_SubCriteria_SubCriteriaId",
                        column: x => x.SubCriteriaId,
                        principalTable: "SubCriteria",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubCriteriaAssignment_Unit_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Unit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubCriteriaAssignment_EvaluationPeriodId",
                table: "SubCriteriaAssignment",
                column: "EvaluationPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_SubCriteriaAssignment_SubCriteriaId",
                table: "SubCriteriaAssignment",
                column: "SubCriteriaId");

            migrationBuilder.CreateIndex(
                name: "IX_SubCriteriaAssignment_UnitId",
                table: "SubCriteriaAssignment",
                column: "UnitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubCriteriaAssignment");
        }
    }
}
