using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tenor.Migrations
{
    /// <inheritdoc />
    public partial class EditReportFilterWithLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReportFilters_DimensionLevels_DimensionLevelId",
                table: "ReportFilters");

            migrationBuilder.AlterColumn<int>(
                name: "DimensionLevelId",
                table: "ReportFilters",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "LevelId",
                table: "ReportFilters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ReportFilters_LevelId",
                table: "ReportFilters",
                column: "LevelId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReportFilters_DimensionLevels_DimensionLevelId",
                table: "ReportFilters",
                column: "DimensionLevelId",
                principalTable: "DimensionLevels",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReportFilters_Levels_LevelId",
                table: "ReportFilters",
                column: "LevelId",
                principalTable: "Levels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReportFilters_DimensionLevels_DimensionLevelId",
                table: "ReportFilters");

            migrationBuilder.DropForeignKey(
                name: "FK_ReportFilters_Levels_LevelId",
                table: "ReportFilters");

            migrationBuilder.DropIndex(
                name: "IX_ReportFilters_LevelId",
                table: "ReportFilters");

            migrationBuilder.DropColumn(
                name: "LevelId",
                table: "ReportFilters");

            migrationBuilder.AlterColumn<int>(
                name: "DimensionLevelId",
                table: "ReportFilters",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ReportFilters_DimensionLevels_DimensionLevelId",
                table: "ReportFilters",
                column: "DimensionLevelId",
                principalTable: "DimensionLevels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
