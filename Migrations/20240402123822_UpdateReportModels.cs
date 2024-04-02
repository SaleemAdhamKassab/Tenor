using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tenor.Migrations
{
    /// <inheritdoc />
    public partial class UpdateReportModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExtraFields_Devices_DeviceId",
                table: "ExtraFields");

            migrationBuilder.DropForeignKey(
                name: "FK_MeasureHavings_reportMeasures_ReportMeasureID",
                table: "MeasureHavings");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "reportMeasures",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "ReportLevels",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "ReportFilters",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ReportMeasureID",
                table: "MeasureHavings",
                newName: "ReportMeasureId");

            migrationBuilder.RenameIndex(
                name: "IX_MeasureHavings_ReportMeasureID",
                table: "MeasureHavings",
                newName: "IX_MeasureHavings_ReportMeasureId");

            migrationBuilder.AlterColumn<int>(
                name: "SortDirection",
                table: "ReportLevels",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "DeviceId",
                table: "ExtraFields",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_ExtraFields_Devices_DeviceId",
                table: "ExtraFields",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MeasureHavings_reportMeasures_ReportMeasureId",
                table: "MeasureHavings",
                column: "ReportMeasureId",
                principalTable: "reportMeasures",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExtraFields_Devices_DeviceId",
                table: "ExtraFields");

            migrationBuilder.DropForeignKey(
                name: "FK_MeasureHavings_reportMeasures_ReportMeasureId",
                table: "MeasureHavings");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "reportMeasures",
                newName: "ID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "ReportLevels",
                newName: "ID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "ReportFilters",
                newName: "ID");

            migrationBuilder.RenameColumn(
                name: "ReportMeasureId",
                table: "MeasureHavings",
                newName: "ReportMeasureID");

            migrationBuilder.RenameIndex(
                name: "IX_MeasureHavings_ReportMeasureId",
                table: "MeasureHavings",
                newName: "IX_MeasureHavings_ReportMeasureID");

            migrationBuilder.AlterColumn<string>(
                name: "SortDirection",
                table: "ReportLevels",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "DeviceId",
                table: "ExtraFields",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ExtraFields_Devices_DeviceId",
                table: "ExtraFields",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MeasureHavings_reportMeasures_ReportMeasureID",
                table: "MeasureHavings",
                column: "ReportMeasureID",
                principalTable: "reportMeasures",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
