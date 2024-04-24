using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tenor.Migrations
{
    /// <inheritdoc />
    public partial class EditReportLevelRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReportLevels_DimensionLevels_DimensionLevelId",
                table: "ReportLevels");

            migrationBuilder.RenameColumn(
                name: "DimensionLevelId",
                table: "ReportLevels",
                newName: "LevelId");

            migrationBuilder.RenameIndex(
                name: "IX_ReportLevels_DimensionLevelId",
                table: "ReportLevels",
                newName: "IX_ReportLevels_LevelId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReportLevels_Levels_LevelId",
                table: "ReportLevels",
                column: "LevelId",
                principalTable: "Levels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReportLevels_Levels_LevelId",
                table: "ReportLevels");

            migrationBuilder.RenameColumn(
                name: "LevelId",
                table: "ReportLevels",
                newName: "DimensionLevelId");

            migrationBuilder.RenameIndex(
                name: "IX_ReportLevels_LevelId",
                table: "ReportLevels",
                newName: "IX_ReportLevels_DimensionLevelId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReportLevels_DimensionLevels_DimensionLevelId",
                table: "ReportLevels",
                column: "DimensionLevelId",
                principalTable: "DimensionLevels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
