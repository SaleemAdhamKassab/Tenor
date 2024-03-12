using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tenor.Migrations
{
    /// <inheritdoc />
    public partial class byAmrAddIsMandatory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupTenantRoles_Tenants_TenantId",
                table: "GroupTenantRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTenantRoles_Tenants_TenantId",
                table: "UserTenantRoles");

            migrationBuilder.AlterColumn<int>(
                name: "TenantId",
                table: "UserTenantRoles",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "TenantId",
                table: "GroupTenantRoles",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<bool>(
                name: "IsMandatory",
                table: "ExtraFields",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupTenantRoles_Tenants_TenantId",
                table: "GroupTenantRoles",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserTenantRoles_Tenants_TenantId",
                table: "UserTenantRoles",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupTenantRoles_Tenants_TenantId",
                table: "GroupTenantRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTenantRoles_Tenants_TenantId",
                table: "UserTenantRoles");

            migrationBuilder.DropColumn(
                name: "IsMandatory",
                table: "ExtraFields");

            migrationBuilder.AlterColumn<int>(
                name: "TenantId",
                table: "UserTenantRoles",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TenantId",
                table: "GroupTenantRoles",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupTenantRoles_Tenants_TenantId",
                table: "GroupTenantRoles",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTenantRoles_Tenants_TenantId",
                table: "UserTenantRoles",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
