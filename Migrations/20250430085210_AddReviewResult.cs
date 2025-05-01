using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace chuyendoiso.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinalReviewResult",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReviewAssignmentId = table.Column<int>(type: "integer", nullable: false),
                    FinalScore = table.Column<float>(type: "real", nullable: true),
                    FinalComment = table.Column<string>(type: "text", nullable: true),
                    FinalAttachmentPath = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinalReviewResult", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinalReviewResult_ReviewAssignment_ReviewAssignmentId",
                        column: x => x.ReviewAssignmentId,
                        principalTable: "ReviewAssignment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReviewResult",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReviewAssignmentId = table.Column<int>(type: "integer", nullable: false),
                    Score = table.Column<float>(type: "real", nullable: true),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    AttachmentPath = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewResult", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReviewResult_ReviewAssignment_ReviewAssignmentId",
                        column: x => x.ReviewAssignmentId,
                        principalTable: "ReviewAssignment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinalReviewResult_ReviewAssignmentId",
                table: "FinalReviewResult",
                column: "ReviewAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewResult_ReviewAssignmentId",
                table: "ReviewResult",
                column: "ReviewAssignmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinalReviewResult");

            migrationBuilder.DropTable(
                name: "ReviewResult");
        }
    }
}
