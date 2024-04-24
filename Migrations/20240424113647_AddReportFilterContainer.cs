using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tenor.Migrations
{
    /// <inheritdoc />
    public partial class AddReportFilterContainer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReportFilters_Reports_ReportId",
                table: "ReportFilters");

            migrationBuilder.RenameColumn(
                name: "ReportId",
                table: "ReportFilters",
                newName: "FilterContainerId");

            migrationBuilder.RenameIndex(
                name: "IX_ReportFilters_ReportId",
                table: "ReportFilters",
                newName: "IX_ReportFilters_FilterContainerId");

            migrationBuilder.CreateTable(
                name: "ReportFilterContainers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportId = table.Column<int>(type: "int", nullable: false),
                    LogicalOperator = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportFilterContainers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportFilterContainers_Reports_ReportId",
                        column: x => x.ReportId,
                        principalTable: "Reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReportFilterContainers_ReportId",
                table: "ReportFilterContainers",
                column: "ReportId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReportFilters_ReportFilterContainers_FilterContainerId",
                table: "ReportFilters",
                column: "FilterContainerId",
                principalTable: "ReportFilterContainers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReportFilters_ReportFilterContainers_FilterContainerId",
                table: "ReportFilters");

            migrationBuilder.DropTable(
                name: "ReportFilterContainers");

            migrationBuilder.RenameColumn(
                name: "FilterContainerId",
                table: "ReportFilters",
                newName: "ReportId");

            migrationBuilder.RenameIndex(
                name: "IX_ReportFilters_FilterContainerId",
                table: "ReportFilters",
                newName: "IX_ReportFilters_ReportId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReportFilters_Reports_ReportId",
                table: "ReportFilters",
                column: "ReportId",
                principalTable: "Reports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
