using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace chuyendoiso.Migrations
{
    /// <inheritdoc />
    public partial class Criteria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TargetGroup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TargetGroup", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ParentCriteria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    MaxScore = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    TargetGroupId = table.Column<int>(type: "integer", nullable: false),
                    EvidenceInfo = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParentCriteria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParentCriteria_TargetGroup_TargetGroupId",
                        column: x => x.TargetGroupId,
                        principalTable: "TargetGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubCriteria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    MaxScore = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ParentCriteriaId = table.Column<int>(type: "integer", nullable: false),
                    EvidenceInfo = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubCriteria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubCriteria_ParentCriteria_ParentCriteriaId",
                        column: x => x.ParentCriteriaId,
                        principalTable: "ParentCriteria",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParentCriteria_TargetGroupId",
                table: "ParentCriteria",
                column: "TargetGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_SubCriteria_ParentCriteriaId",
                table: "SubCriteria",
                column: "ParentCriteriaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubCriteria");

            migrationBuilder.DropTable(
                name: "ParentCriteria");

            migrationBuilder.DropTable(
                name: "TargetGroup");
        }
    }
}
