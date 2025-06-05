using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chuyendoiso.Migrations
{
    /// <inheritdoc />
    public partial class UpdateReviewAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsUpdatedByUnit",
                table: "ReviewAssignment",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsUpdatedByUnit",
                table: "ReviewAssignment");
        }
    }
}
