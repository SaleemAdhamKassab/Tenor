using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tenor.Migrations
{
    /// <inheritdoc />
    public partial class RenameMainSetsToDevices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subsets_MainSets_MainSetId",
                table: "Subsets");

            migrationBuilder.DropTable(
                name: "MainSets");

            migrationBuilder.RenameColumn(
                name: "MainSetId",
                table: "Subsets",
                newName: "DeviceId");

            migrationBuilder.RenameIndex(
                name: "IX_Subsets_MainSetId",
                table: "Subsets",
                newName: "IX_Subsets_DeviceId");

            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Devices_Devices_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Devices",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Devices_ParentId",
                table: "Devices",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Subsets_Devices_DeviceId",
                table: "Subsets",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subsets_Devices_DeviceId",
                table: "Subsets");

            migrationBuilder.DropTable(
                name: "Devices");

            migrationBuilder.RenameColumn(
                name: "DeviceId",
                table: "Subsets",
                newName: "MainSetId");

            migrationBuilder.RenameIndex(
                name: "IX_Subsets_DeviceId",
                table: "Subsets",
                newName: "IX_Subsets_MainSetId");

            migrationBuilder.CreateTable(
                name: "MainSets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SupplierId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MainSets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MainSets_MainSets_ParentId",
                        column: x => x.ParentId,
                        principalTable: "MainSets",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MainSets_ParentId",
                table: "MainSets",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Subsets_MainSets_MainSetId",
                table: "Subsets",
                column: "MainSetId",
                principalTable: "MainSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
