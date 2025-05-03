using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chuyendoiso.Migrations
{
    /// <inheritdoc />
    public partial class NullInParentCriteria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParentCriteria_EvaluationPeriod_EvaluationPeriodId",
                table: "ParentCriteria");

            migrationBuilder.DropForeignKey(
                name: "FK_ParentCriteria_TargetGroup_TargetGroupId",
                table: "ParentCriteria");

            migrationBuilder.AlterColumn<int>(
                name: "TargetGroupId",
                table: "ParentCriteria",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "EvaluationPeriodId",
                table: "ParentCriteria",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_ParentCriteria_EvaluationPeriod_EvaluationPeriodId",
                table: "ParentCriteria",
                column: "EvaluationPeriodId",
                principalTable: "EvaluationPeriod",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ParentCriteria_TargetGroup_TargetGroupId",
                table: "ParentCriteria",
                column: "TargetGroupId",
                principalTable: "TargetGroup",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParentCriteria_EvaluationPeriod_EvaluationPeriodId",
                table: "ParentCriteria");

            migrationBuilder.DropForeignKey(
                name: "FK_ParentCriteria_TargetGroup_TargetGroupId",
                table: "ParentCriteria");

            migrationBuilder.AlterColumn<int>(
                name: "TargetGroupId",
                table: "ParentCriteria",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EvaluationPeriodId",
                table: "ParentCriteria",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ParentCriteria_EvaluationPeriod_EvaluationPeriodId",
                table: "ParentCriteria",
                column: "EvaluationPeriodId",
                principalTable: "EvaluationPeriod",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ParentCriteria_TargetGroup_TargetGroupId",
                table: "ParentCriteria",
                column: "TargetGroupId",
                principalTable: "TargetGroup",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
