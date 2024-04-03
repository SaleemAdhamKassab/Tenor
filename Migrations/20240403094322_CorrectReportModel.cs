using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tenor.Migrations
{
    /// <inheritdoc />
    public partial class CorrectReportModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MeasureHavings_reportMeasures_ReportMeasureId",
                table: "MeasureHavings");

            migrationBuilder.DropForeignKey(
                name: "FK_ReportFilters_DimensionLevels_DimensionLevelID",
                table: "ReportFilters");

            migrationBuilder.DropForeignKey(
                name: "FK_ReportFilters_Reports_ReportID",
                table: "ReportFilters");

            migrationBuilder.DropForeignKey(
                name: "FK_ReportLevels_DimensionLevels_DimensionLevelID",
                table: "ReportLevels");

            migrationBuilder.DropForeignKey(
                name: "FK_ReportLevels_Reports_ReportID",
                table: "ReportLevels");

            migrationBuilder.DropForeignKey(
                name: "FK_reportMeasures_Operations_OperationID",
                table: "reportMeasures");

            migrationBuilder.DropForeignKey(
                name: "FK_reportMeasures_Reports_ReportID",
                table: "reportMeasures");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Devices_DeviceID",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Reports_ChildID",
                table: "Reports");

            migrationBuilder.DropPrimaryKey(
                name: "PK_reportMeasures",
                table: "reportMeasures");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "MeasureHavings");

            migrationBuilder.RenameTable(
                name: "reportMeasures",
                newName: "ReportMeasures");

            migrationBuilder.RenameColumn(
                name: "DeviceID",
                table: "Reports",
                newName: "DeviceId");

            migrationBuilder.RenameColumn(
                name: "ChildID",
                table: "Reports",
                newName: "ChildId");

            migrationBuilder.RenameIndex(
                name: "IX_Reports_DeviceID",
                table: "Reports",
                newName: "IX_Reports_DeviceId");

            migrationBuilder.RenameIndex(
                name: "IX_Reports_ChildID",
                table: "Reports",
                newName: "IX_Reports_ChildId");

            migrationBuilder.RenameColumn(
                name: "ReportID",
                table: "ReportMeasures",
                newName: "ReportId");

            migrationBuilder.RenameColumn(
                name: "OperationID",
                table: "ReportMeasures",
                newName: "OperationId");

            migrationBuilder.RenameIndex(
                name: "IX_reportMeasures_ReportID",
                table: "ReportMeasures",
                newName: "IX_ReportMeasures_ReportId");

            migrationBuilder.RenameIndex(
                name: "IX_reportMeasures_OperationID",
                table: "ReportMeasures",
                newName: "IX_ReportMeasures_OperationId");

            migrationBuilder.RenameColumn(
                name: "ReportID",
                table: "ReportLevels",
                newName: "ReportId");

            migrationBuilder.RenameColumn(
                name: "DimensionLevelID",
                table: "ReportLevels",
                newName: "DimensionLevelId");

            migrationBuilder.RenameIndex(
                name: "IX_ReportLevels_ReportID",
                table: "ReportLevels",
                newName: "IX_ReportLevels_ReportId");

            migrationBuilder.RenameIndex(
                name: "IX_ReportLevels_DimensionLevelID",
                table: "ReportLevels",
                newName: "IX_ReportLevels_DimensionLevelId");

            migrationBuilder.RenameColumn(
                name: "ReportID",
                table: "ReportFilters",
                newName: "ReportId");

            migrationBuilder.RenameColumn(
                name: "DimensionLevelID",
                table: "ReportFilters",
                newName: "DimensionLevelId");

            migrationBuilder.RenameIndex(
                name: "IX_ReportFilters_ReportID",
                table: "ReportFilters",
                newName: "IX_ReportFilters_ReportId");

            migrationBuilder.RenameIndex(
                name: "IX_ReportFilters_DimensionLevelID",
                table: "ReportFilters",
                newName: "IX_ReportFilters_DimensionLevelId");

            migrationBuilder.RenameColumn(
                name: "MeasureID",
                table: "MeasureHavings",
                newName: "MeasureId");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "MeasureHavings",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "LogicalOperator",
                table: "MeasureHavings",
                newName: "OperatorId");

            migrationBuilder.AddColumn<int>(
                name: "LogicOpt",
                table: "MeasureHavings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Value",
                table: "MeasureHavings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReportMeasures",
                table: "ReportMeasures",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_MeasureHavings_OperatorId",
                table: "MeasureHavings",
                column: "OperatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_MeasureHavings_Operators_OperatorId",
                table: "MeasureHavings",
                column: "OperatorId",
                principalTable: "Operators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MeasureHavings_ReportMeasures_ReportMeasureId",
                table: "MeasureHavings",
                column: "ReportMeasureId",
                principalTable: "ReportMeasures",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReportFilters_DimensionLevels_DimensionLevelId",
                table: "ReportFilters",
                column: "DimensionLevelId",
                principalTable: "DimensionLevels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReportFilters_Reports_ReportId",
                table: "ReportFilters",
                column: "ReportId",
                principalTable: "Reports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReportLevels_DimensionLevels_DimensionLevelId",
                table: "ReportLevels",
                column: "DimensionLevelId",
                principalTable: "DimensionLevels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReportLevels_Reports_ReportId",
                table: "ReportLevels",
                column: "ReportId",
                principalTable: "Reports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReportMeasures_Operations_OperationId",
                table: "ReportMeasures",
                column: "OperationId",
                principalTable: "Operations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReportMeasures_Reports_ReportId",
                table: "ReportMeasures",
                column: "ReportId",
                principalTable: "Reports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Devices_DeviceId",
                table: "Reports",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Reports_ChildId",
                table: "Reports",
                column: "ChildId",
                principalTable: "Reports",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MeasureHavings_Operators_OperatorId",
                table: "MeasureHavings");

            migrationBuilder.DropForeignKey(
                name: "FK_MeasureHavings_ReportMeasures_ReportMeasureId",
                table: "MeasureHavings");

            migrationBuilder.DropForeignKey(
                name: "FK_ReportFilters_DimensionLevels_DimensionLevelId",
                table: "ReportFilters");

            migrationBuilder.DropForeignKey(
                name: "FK_ReportFilters_Reports_ReportId",
                table: "ReportFilters");

            migrationBuilder.DropForeignKey(
                name: "FK_ReportLevels_DimensionLevels_DimensionLevelId",
                table: "ReportLevels");

            migrationBuilder.DropForeignKey(
                name: "FK_ReportLevels_Reports_ReportId",
                table: "ReportLevels");

            migrationBuilder.DropForeignKey(
                name: "FK_ReportMeasures_Operations_OperationId",
                table: "ReportMeasures");

            migrationBuilder.DropForeignKey(
                name: "FK_ReportMeasures_Reports_ReportId",
                table: "ReportMeasures");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Devices_DeviceId",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Reports_ChildId",
                table: "Reports");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReportMeasures",
                table: "ReportMeasures");

            migrationBuilder.DropIndex(
                name: "IX_MeasureHavings_OperatorId",
                table: "MeasureHavings");

            migrationBuilder.DropColumn(
                name: "LogicOpt",
                table: "MeasureHavings");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "MeasureHavings");

            migrationBuilder.RenameTable(
                name: "ReportMeasures",
                newName: "reportMeasures");

            migrationBuilder.RenameColumn(
                name: "DeviceId",
                table: "Reports",
                newName: "DeviceID");

            migrationBuilder.RenameColumn(
                name: "ChildId",
                table: "Reports",
                newName: "ChildID");

            migrationBuilder.RenameIndex(
                name: "IX_Reports_DeviceId",
                table: "Reports",
                newName: "IX_Reports_DeviceID");

            migrationBuilder.RenameIndex(
                name: "IX_Reports_ChildId",
                table: "Reports",
                newName: "IX_Reports_ChildID");

            migrationBuilder.RenameColumn(
                name: "ReportId",
                table: "reportMeasures",
                newName: "ReportID");

            migrationBuilder.RenameColumn(
                name: "OperationId",
                table: "reportMeasures",
                newName: "OperationID");

            migrationBuilder.RenameIndex(
                name: "IX_ReportMeasures_ReportId",
                table: "reportMeasures",
                newName: "IX_reportMeasures_ReportID");

            migrationBuilder.RenameIndex(
                name: "IX_ReportMeasures_OperationId",
                table: "reportMeasures",
                newName: "IX_reportMeasures_OperationID");

            migrationBuilder.RenameColumn(
                name: "ReportId",
                table: "ReportLevels",
                newName: "ReportID");

            migrationBuilder.RenameColumn(
                name: "DimensionLevelId",
                table: "ReportLevels",
                newName: "DimensionLevelID");

            migrationBuilder.RenameIndex(
                name: "IX_ReportLevels_ReportId",
                table: "ReportLevels",
                newName: "IX_ReportLevels_ReportID");

            migrationBuilder.RenameIndex(
                name: "IX_ReportLevels_DimensionLevelId",
                table: "ReportLevels",
                newName: "IX_ReportLevels_DimensionLevelID");

            migrationBuilder.RenameColumn(
                name: "ReportId",
                table: "ReportFilters",
                newName: "ReportID");

            migrationBuilder.RenameColumn(
                name: "DimensionLevelId",
                table: "ReportFilters",
                newName: "DimensionLevelID");

            migrationBuilder.RenameIndex(
                name: "IX_ReportFilters_ReportId",
                table: "ReportFilters",
                newName: "IX_ReportFilters_ReportID");

            migrationBuilder.RenameIndex(
                name: "IX_ReportFilters_DimensionLevelId",
                table: "ReportFilters",
                newName: "IX_ReportFilters_DimensionLevelID");

            migrationBuilder.RenameColumn(
                name: "MeasureId",
                table: "MeasureHavings",
                newName: "MeasureID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "MeasureHavings",
                newName: "ID");

            migrationBuilder.RenameColumn(
                name: "OperatorId",
                table: "MeasureHavings",
                newName: "LogicalOperator");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "MeasureHavings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_reportMeasures",
                table: "reportMeasures",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MeasureHavings_reportMeasures_ReportMeasureId",
                table: "MeasureHavings",
                column: "ReportMeasureId",
                principalTable: "reportMeasures",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReportFilters_DimensionLevels_DimensionLevelID",
                table: "ReportFilters",
                column: "DimensionLevelID",
                principalTable: "DimensionLevels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReportFilters_Reports_ReportID",
                table: "ReportFilters",
                column: "ReportID",
                principalTable: "Reports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReportLevels_DimensionLevels_DimensionLevelID",
                table: "ReportLevels",
                column: "DimensionLevelID",
                principalTable: "DimensionLevels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReportLevels_Reports_ReportID",
                table: "ReportLevels",
                column: "ReportID",
                principalTable: "Reports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_reportMeasures_Operations_OperationID",
                table: "reportMeasures",
                column: "OperationID",
                principalTable: "Operations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_reportMeasures_Reports_ReportID",
                table: "reportMeasures",
                column: "ReportID",
                principalTable: "Reports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Devices_DeviceID",
                table: "Reports",
                column: "DeviceID",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Reports_ChildID",
                table: "Reports",
                column: "ChildID",
                principalTable: "Reports",
                principalColumn: "Id");
        }
    }
}
