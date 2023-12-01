using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tenor.Migrations
{
    /// <inheritdoc />
    public partial class AddExtraFileldEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Dashboards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dashboards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExtraFields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtraFields", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CounterFields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FieldId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CounterFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CounterFields_ExtraFields_FieldId",
                        column: x => x.FieldId,
                        principalTable: "ExtraFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DashboardFields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FieldId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DashboardFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DashboardFields_ExtraFields_FieldId",
                        column: x => x.FieldId,
                        principalTable: "ExtraFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KpiFields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FieldId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KpiFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KpiFields_ExtraFields_FieldId",
                        column: x => x.FieldId,
                        principalTable: "ExtraFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportFields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FieldId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportFields_ExtraFields_FieldId",
                        column: x => x.FieldId,
                        principalTable: "ExtraFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CounterFieldValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CounterId = table.Column<int>(type: "int", nullable: false),
                    CounterFieldId = table.Column<int>(type: "int", nullable: false),
                    FieldValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CounterFieldValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CounterFieldValues_CounterFields_CounterFieldId",
                        column: x => x.CounterFieldId,
                        principalTable: "CounterFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CounterFieldValues_Counters_CounterId",
                        column: x => x.CounterId,
                        principalTable: "Counters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DashboardFieldValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DashboardId = table.Column<int>(type: "int", nullable: false),
                    DashboardFieldId = table.Column<int>(type: "int", nullable: false),
                    FieldValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DashboardFieldValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DashboardFieldValues_DashboardFields_DashboardFieldId",
                        column: x => x.DashboardFieldId,
                        principalTable: "DashboardFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DashboardFieldValues_Dashboards_DashboardId",
                        column: x => x.DashboardId,
                        principalTable: "Dashboards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KpiFieldValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KpiId = table.Column<int>(type: "int", nullable: false),
                    KpiFieldId = table.Column<int>(type: "int", nullable: false),
                    FieldValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KpiFieldValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KpiFieldValues_KpiFields_KpiFieldId",
                        column: x => x.KpiFieldId,
                        principalTable: "KpiFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KpiFieldValues_Kpis_KpiId",
                        column: x => x.KpiId,
                        principalTable: "Kpis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportFieldValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportId = table.Column<int>(type: "int", nullable: false),
                    ReportFieldId = table.Column<int>(type: "int", nullable: false),
                    FieldValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportFieldValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportFieldValues_ReportFields_ReportFieldId",
                        column: x => x.ReportFieldId,
                        principalTable: "ReportFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReportFieldValues_Reports_ReportId",
                        column: x => x.ReportId,
                        principalTable: "Reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CounterFields_FieldId",
                table: "CounterFields",
                column: "FieldId");

            migrationBuilder.CreateIndex(
                name: "IX_CounterFieldValues_CounterFieldId",
                table: "CounterFieldValues",
                column: "CounterFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_CounterFieldValues_CounterId",
                table: "CounterFieldValues",
                column: "CounterId");

            migrationBuilder.CreateIndex(
                name: "IX_DashboardFields_FieldId",
                table: "DashboardFields",
                column: "FieldId");

            migrationBuilder.CreateIndex(
                name: "IX_DashboardFieldValues_DashboardFieldId",
                table: "DashboardFieldValues",
                column: "DashboardFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_DashboardFieldValues_DashboardId",
                table: "DashboardFieldValues",
                column: "DashboardId");

            migrationBuilder.CreateIndex(
                name: "IX_ExtraFields_Name",
                table: "ExtraFields",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KpiFields_FieldId",
                table: "KpiFields",
                column: "FieldId");

            migrationBuilder.CreateIndex(
                name: "IX_KpiFieldValues_KpiFieldId",
                table: "KpiFieldValues",
                column: "KpiFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_KpiFieldValues_KpiId",
                table: "KpiFieldValues",
                column: "KpiId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportFields_FieldId",
                table: "ReportFields",
                column: "FieldId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportFieldValues_ReportFieldId",
                table: "ReportFieldValues",
                column: "ReportFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportFieldValues_ReportId",
                table: "ReportFieldValues",
                column: "ReportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CounterFieldValues");

            migrationBuilder.DropTable(
                name: "DashboardFieldValues");

            migrationBuilder.DropTable(
                name: "KpiFieldValues");

            migrationBuilder.DropTable(
                name: "ReportFieldValues");

            migrationBuilder.DropTable(
                name: "CounterFields");

            migrationBuilder.DropTable(
                name: "DashboardFields");

            migrationBuilder.DropTable(
                name: "Dashboards");

            migrationBuilder.DropTable(
                name: "KpiFields");

            migrationBuilder.DropTable(
                name: "ReportFields");

            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropTable(
                name: "ExtraFields");
        }
    }
}
