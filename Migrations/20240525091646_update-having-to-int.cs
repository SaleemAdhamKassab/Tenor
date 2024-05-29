using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tenor.Migrations
{
    /// <inheritdoc />
    public partial class updatehavingtoint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dimensions_Devices_DeviceId",
                table: "Dimensions");

            migrationBuilder.DropForeignKey(
                name: "FK_MeasureHavings_Operators_OperatorId",
                table: "MeasureHavings");

            migrationBuilder.AlterColumn<int>(
                name: "Value",
                table: "MeasureHavings",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "OperatorId",
                table: "MeasureHavings",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "DeviceId",
                table: "Dimensions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Dimensions_Devices_DeviceId",
                table: "Dimensions",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MeasureHavings_Operators_OperatorId",
                table: "MeasureHavings",
                column: "OperatorId",
                principalTable: "Operators",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dimensions_Devices_DeviceId",
                table: "Dimensions");

            migrationBuilder.DropForeignKey(
                name: "FK_MeasureHavings_Operators_OperatorId",
                table: "MeasureHavings");

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "MeasureHavings",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "OperatorId",
                table: "MeasureHavings",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DeviceId",
                table: "Dimensions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Dimensions_Devices_DeviceId",
                table: "Dimensions",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MeasureHavings_Operators_OperatorId",
                table: "MeasureHavings",
                column: "OperatorId",
                principalTable: "Operators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
