using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tenor.Migrations
{
    /// <inheritdoc />
    public partial class AddLevelsEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Trees");

            migrationBuilder.DropTable(
                name: "DeviceLevels");

            migrationBuilder.AddColumn<string>(
                name: "DataType",
                table: "Levels",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsFilter",
                table: "Levels",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsLevel",
                table: "Levels",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Dimensions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TableName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SchemaName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeviceId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dimensions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dimensions_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DimensionJoiners",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PkName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FkName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DimensionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DimensionJoiners", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DimensionJoiners_Dimensions_DimensionId",
                        column: x => x.DimensionId,
                        principalTable: "Dimensions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DimensionLevels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DimensionId = table.Column<int>(type: "int", nullable: false),
                    LevelId = table.Column<int>(type: "int", nullable: false),
                    ColumnName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DimensionLevels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DimensionLevels_DimensionLevels_ParentId",
                        column: x => x.ParentId,
                        principalTable: "DimensionLevels",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DimensionLevels_Dimensions_DimensionId",
                        column: x => x.DimensionId,
                        principalTable: "Dimensions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DimensionLevels_Levels_LevelId",
                        column: x => x.LevelId,
                        principalTable: "Levels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DimensionJoiners_DimensionId",
                table: "DimensionJoiners",
                column: "DimensionId");

            migrationBuilder.CreateIndex(
                name: "IX_DimensionLevels_DimensionId",
                table: "DimensionLevels",
                column: "DimensionId");

            migrationBuilder.CreateIndex(
                name: "IX_DimensionLevels_LevelId",
                table: "DimensionLevels",
                column: "LevelId");

            migrationBuilder.CreateIndex(
                name: "IX_DimensionLevels_ParentId",
                table: "DimensionLevels",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Dimensions_DeviceId",
                table: "Dimensions",
                column: "DeviceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DimensionJoiners");

            migrationBuilder.DropTable(
                name: "DimensionLevels");

            migrationBuilder.DropTable(
                name: "Dimensions");

            migrationBuilder.DropColumn(
                name: "DataType",
                table: "Levels");

            migrationBuilder.DropColumn(
                name: "IsFilter",
                table: "Levels");

            migrationBuilder.DropColumn(
                name: "IsLevel",
                table: "Levels");

            migrationBuilder.CreateTable(
                name: "DeviceLevels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceId = table.Column<int>(type: "int", nullable: false),
                    LevelId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceLevels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceLevels_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeviceLevels_Levels_LevelId",
                        column: x => x.LevelId,
                        principalTable: "Levels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Trees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceLevelId = table.Column<int>(type: "int", nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trees_DeviceLevels_DeviceLevelId",
                        column: x => x.DeviceLevelId,
                        principalTable: "DeviceLevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Trees_Trees_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Trees",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceLevels_DeviceId",
                table: "DeviceLevels",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceLevels_LevelId",
                table: "DeviceLevels",
                column: "LevelId");

            migrationBuilder.CreateIndex(
                name: "IX_Trees_DeviceLevelId",
                table: "Trees",
                column: "DeviceLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_Trees_ParentId",
                table: "Trees",
                column: "ParentId");
        }
    }
}
