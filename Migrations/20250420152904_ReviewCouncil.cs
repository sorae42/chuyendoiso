using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace chuyendoiso.Migrations
{
    /// <inheritdoc />
    public partial class ReviewCouncil : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReviewCouncil",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewCouncil", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReviewCouncil_Auth_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Auth",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reviewer",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AuthId = table.Column<int>(type: "integer", nullable: false),
                    ReviewCouncilId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviewer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reviewer_Auth_AuthId",
                        column: x => x.AuthId,
                        principalTable: "Auth",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reviewer_ReviewCouncil_ReviewCouncilId",
                        column: x => x.ReviewCouncilId,
                        principalTable: "ReviewCouncil",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReviewAssignment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReviewerId = table.Column<int>(type: "integer", nullable: false),
                    UnitId = table.Column<int>(type: "integer", nullable: false),
                    SubCriteriaId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewAssignment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReviewAssignment_Reviewer_ReviewerId",
                        column: x => x.ReviewerId,
                        principalTable: "Reviewer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReviewAssignment_SubCriteria_SubCriteriaId",
                        column: x => x.SubCriteriaId,
                        principalTable: "SubCriteria",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReviewAssignment_Unit_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Unit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReviewAssignment_ReviewerId",
                table: "ReviewAssignment",
                column: "ReviewerId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewAssignment_SubCriteriaId",
                table: "ReviewAssignment",
                column: "SubCriteriaId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewAssignment_UnitId",
                table: "ReviewAssignment",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewCouncil_CreatedById",
                table: "ReviewCouncil",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Reviewer_AuthId",
                table: "Reviewer",
                column: "AuthId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviewer_ReviewCouncilId",
                table: "Reviewer",
                column: "ReviewCouncilId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReviewAssignment");

            migrationBuilder.DropTable(
                name: "Reviewer");

            migrationBuilder.DropTable(
                name: "ReviewCouncil");
        }
    }
}
