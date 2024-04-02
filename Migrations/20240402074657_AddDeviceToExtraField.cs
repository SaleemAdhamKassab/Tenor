using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tenor.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceToExtraField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeviceId",
                table: "ExtraFields",
                type: "int",
                nullable: true
               );

            migrationBuilder.CreateIndex(
                name: "IX_ExtraFields_DeviceId",
                table: "ExtraFields",
                column: "DeviceId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExtraFields_Devices_DeviceId",
                table: "ExtraFields",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExtraFields_Devices_DeviceId",
                table: "ExtraFields");

            migrationBuilder.DropIndex(
                name: "IX_ExtraFields_DeviceId",
                table: "ExtraFields");

            migrationBuilder.DropColumn(
                name: "DeviceId",
                table: "ExtraFields");
        }
    }
}
