using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tenor.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TenorMetaTenants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenorMetaTenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TenorMetaMainSets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenorMetaMainSets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenorMetaMainSets_TenorMetaTenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "TenorMetaTenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "TenorMetaSubsets",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TableName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RefTableName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Schema = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaxDataDate = table.Column<int>(type: "int", nullable: false),
                    IsLoad = table.Column<bool>(type: "bit", nullable: false),
                    DataTS = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IndexTS = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DbLink = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    MainSetId = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenorMetaSubsets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenorMetaSubsets_TenorMetaMainSets_MainSetId",
                        column: x => x.MainSetId,
                        principalTable: "TenorMetaMainSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_TenorMetaSubsets_TenorMetaTenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "TenorMetaTenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenorMetaCounters",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ColumnName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RefColumnName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Aggregation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    SubsetId = table.Column<long>(type: "bigint", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenorMetaCounters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenorMetaCounters_TenorMetaSubsets_SubsetId",
                        column: x => x.SubsetId,
                        principalTable: "TenorMetaSubsets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_TenorMetaCounters_TenorMetaTenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "TenorMetaTenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TenorMetaCounters_SubsetId",
                table: "TenorMetaCounters",
                column: "SubsetId");

            migrationBuilder.CreateIndex(
                name: "IX_TenorMetaCounters_TenantId",
                table: "TenorMetaCounters",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TenorMetaMainSets_TenantId",
                table: "TenorMetaMainSets",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TenorMetaSubsets_MainSetId",
                table: "TenorMetaSubsets",
                column: "MainSetId");

            migrationBuilder.CreateIndex(
                name: "IX_TenorMetaSubsets_TenantId",
                table: "TenorMetaSubsets",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TenorMetaCounters");

            migrationBuilder.DropTable(
                name: "TenorMetaSubsets");

            migrationBuilder.DropTable(
                name: "TenorMetaMainSets");

            migrationBuilder.DropTable(
                name: "TenorMetaTenants");
        }
    }
}
