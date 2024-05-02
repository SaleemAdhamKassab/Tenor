using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tenor.Migrations
{
    /// <inheritdoc />
    public partial class EditReportFilterWithLevel2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReportFilters_DimensionLevels_DimensionLevelId",
                table: "ReportFilters");

            migrationBuilder.DropIndex(
                name: "IX_ReportFilters_DimensionLevelId",
                table: "ReportFilters");

            migrationBuilder.DropColumn(
                name: "DimensionLevelId",
                table: "ReportFilters");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DimensionLevelId",
                table: "ReportFilters",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReportFilters_DimensionLevelId",
                table: "ReportFilters",
                column: "DimensionLevelId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReportFilters_DimensionLevels_DimensionLevelId",
                table: "ReportFilters",
                column: "DimensionLevelId",
                principalTable: "DimensionLevels",
                principalColumn: "Id");
        }
    }
}
