using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chuyendoiso.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUnit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Unit");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Unit");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Unit",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Unit",
                type: "text",
                nullable: true);
        }
    }
}
