using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tenor.Migrations
{
    /// <inheritdoc />
    public partial class AddRelationBetweenKpiAndDevice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeviceId",
                table: "Kpis",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Kpis_DeviceId",
                table: "Kpis",
                column: "DeviceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Kpis_Devices_DeviceId",
                table: "Kpis",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Kpis_Devices_DeviceId",
                table: "Kpis");

            migrationBuilder.DropIndex(
                name: "IX_Kpis_DeviceId",
                table: "Kpis");

            migrationBuilder.DropColumn(
                name: "DeviceId",
                table: "Kpis");
        }
    }
}
