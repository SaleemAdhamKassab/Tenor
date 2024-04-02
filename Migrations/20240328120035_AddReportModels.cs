using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tenor.Migrations
{
    /// <inheritdoc />
    public partial class AddReportModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ChangedBy",
                table: "Reports",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ChangedDate",
                table: "Reports",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "ChildID",
                table: "Reports",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Reports",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Reports",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "DeviceID",
                table: "Reports",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "Reports",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ReportFilters",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LogicalOperator = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReportID = table.Column<int>(type: "int", nullable: false),
                    DimensionLevelID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportFilters", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ReportFilters_DimensionLevels_DimensionLevelID",
                        column: x => x.DimensionLevelID,
                        principalTable: "DimensionLevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReportFilters_Reports_ReportID",
                        column: x => x.ReportID,
                        principalTable: "Reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportLevels",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    SortDirection = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReportID = table.Column<int>(type: "int", nullable: false),
                    DimensionLevelID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportLevels", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ReportLevels_DimensionLevels_DimensionLevelID",
                        column: x => x.DimensionLevelID,
                        principalTable: "DimensionLevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReportLevels_Reports_ReportID",
                        column: x => x.ReportID,
                        principalTable: "Reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reportMeasures",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReportID = table.Column<int>(type: "int", nullable: false),
                    OperationID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reportMeasures", x => x.ID);
                    table.ForeignKey(
                        name: "FK_reportMeasures_Operations_OperationID",
                        column: x => x.OperationID,
                        principalTable: "Operations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_reportMeasures_Reports_ReportID",
                        column: x => x.ReportID,
                        principalTable: "Reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MeasureHavings",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LogicalOperator = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MeasureID = table.Column<int>(type: "int", nullable: false),
                    ReportMeasureID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeasureHavings", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MeasureHavings_reportMeasures_ReportMeasureID",
                        column: x => x.ReportMeasureID,
                        principalTable: "reportMeasures",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ChildID",
                table: "Reports",
                column: "ChildID");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_DeviceID",
                table: "Reports",
                column: "DeviceID");

            migrationBuilder.CreateIndex(
                name: "IX_MeasureHavings_ReportMeasureID",
                table: "MeasureHavings",
                column: "ReportMeasureID");

            migrationBuilder.CreateIndex(
                name: "IX_ReportFilters_DimensionLevelID",
                table: "ReportFilters",
                column: "DimensionLevelID");

            migrationBuilder.CreateIndex(
                name: "IX_ReportFilters_ReportID",
                table: "ReportFilters",
                column: "ReportID");

            migrationBuilder.CreateIndex(
                name: "IX_ReportLevels_DimensionLevelID",
                table: "ReportLevels",
                column: "DimensionLevelID");

            migrationBuilder.CreateIndex(
                name: "IX_ReportLevels_ReportID",
                table: "ReportLevels",
                column: "ReportID");

            migrationBuilder.CreateIndex(
                name: "IX_reportMeasures_OperationID",
                table: "reportMeasures",
                column: "OperationID");

            migrationBuilder.CreateIndex(
                name: "IX_reportMeasures_ReportID",
                table: "reportMeasures",
                column: "ReportID");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Devices_DeviceID",
                table: "Reports",
                column: "DeviceID",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Reports_ChildID",
                table: "Reports",
                column: "ChildID",
                principalTable: "Reports",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Devices_DeviceID",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Reports_ChildID",
                table: "Reports");

            migrationBuilder.DropTable(
                name: "MeasureHavings");

            migrationBuilder.DropTable(
                name: "ReportFilters");

            migrationBuilder.DropTable(
                name: "ReportLevels");

            migrationBuilder.DropTable(
                name: "reportMeasures");

            migrationBuilder.DropIndex(
                name: "IX_Reports_ChildID",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_DeviceID",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ChangedBy",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ChangedDate",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ChildID",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "DeviceID",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "Reports");
        }
    }
}
