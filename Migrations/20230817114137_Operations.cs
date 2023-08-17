using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tenor.Migrations
{
    /// <inheritdoc />
    public partial class Operations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TenorMetaFunctions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ArgumentsCount = table.Column<int>(type: "int", nullable: false),
                    IsBool = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenorMetaFunctions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TenorMetaOperators",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenorMetaOperators", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TenorMetaKPIs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OperationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenorMetaKPIs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TenorMetaOperations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CounterId = table.Column<int>(type: "int", nullable: true),
                    KpiId = table.Column<int>(type: "int", nullable: true),
                    FunctionId = table.Column<int>(type: "int", nullable: true),
                    OperatorId = table.Column<int>(type: "int", nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenorMetaOperations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenorMetaOperations_TenorMetaCounters_CounterId",
                        column: x => x.CounterId,
                        principalTable: "TenorMetaCounters",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TenorMetaOperations_TenorMetaFunctions_FunctionId",
                        column: x => x.FunctionId,
                        principalTable: "TenorMetaFunctions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TenorMetaOperations_TenorMetaKPIs_KpiId",
                        column: x => x.KpiId,
                        principalTable: "TenorMetaKPIs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TenorMetaOperations_TenorMetaOperations_ParentId",
                        column: x => x.ParentId,
                        principalTable: "TenorMetaOperations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TenorMetaOperations_TenorMetaOperators_OperatorId",
                        column: x => x.OperatorId,
                        principalTable: "TenorMetaOperators",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TenorMetaKPIs_OperationId",
                table: "TenorMetaKPIs",
                column: "OperationId");

            migrationBuilder.CreateIndex(
                name: "IX_TenorMetaOperations_CounterId",
                table: "TenorMetaOperations",
                column: "CounterId");

            migrationBuilder.CreateIndex(
                name: "IX_TenorMetaOperations_FunctionId",
                table: "TenorMetaOperations",
                column: "FunctionId");

            migrationBuilder.CreateIndex(
                name: "IX_TenorMetaOperations_KpiId",
                table: "TenorMetaOperations",
                column: "KpiId");

            migrationBuilder.CreateIndex(
                name: "IX_TenorMetaOperations_OperatorId",
                table: "TenorMetaOperations",
                column: "OperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_TenorMetaOperations_ParentId",
                table: "TenorMetaOperations",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_TenorMetaKPIs_TenorMetaOperations_OperationId",
                table: "TenorMetaKPIs",
                column: "OperationId",
                principalTable: "TenorMetaOperations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TenorMetaKPIs_TenorMetaOperations_OperationId",
                table: "TenorMetaKPIs");

            migrationBuilder.DropTable(
                name: "TenorMetaOperations");

            migrationBuilder.DropTable(
                name: "TenorMetaFunctions");

            migrationBuilder.DropTable(
                name: "TenorMetaKPIs");

            migrationBuilder.DropTable(
                name: "TenorMetaOperators");
        }
    }
}
