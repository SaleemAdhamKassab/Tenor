using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tenor.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCountersTable2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Kpis_Devices_DeviceId",
                table: "Kpis");

            migrationBuilder.DropColumn(
                name: "Technology",
                table: "Subsets");

            migrationBuilder.DropColumn(
                name: "TechnologyId",
                table: "Subsets");

            migrationBuilder.DropColumn(
                name: "GroupCategoryId",
                table: "Counters");

            migrationBuilder.DropColumn(
                name: "GroupDeviceTypeId",
                table: "Counters");

            migrationBuilder.DropColumn(
                name: "GroupLevelId",
                table: "Counters");

            migrationBuilder.DropColumn(
                name: "TechnologyId",
                table: "Counters");

            migrationBuilder.AlterColumn<int>(
                name: "DeviceId",
                table: "Kpis",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Kpis_Devices_DeviceId",
                table: "Kpis",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Kpis_Devices_DeviceId",
                table: "Kpis");

            migrationBuilder.AddColumn<string>(
                name: "Technology",
                table: "Subsets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TechnologyId",
                table: "Subsets",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DeviceId",
                table: "Kpis",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GroupCategoryId",
                table: "Counters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GroupDeviceTypeId",
                table: "Counters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GroupLevelId",
                table: "Counters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TechnologyId",
                table: "Counters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Kpis_Devices_DeviceId",
                table: "Kpis",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
