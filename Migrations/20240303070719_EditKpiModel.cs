using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tenor.Migrations
{
    /// <inheritdoc />
    public partial class EditKpiModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Kpis",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationDate",
                table: "Kpis",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "Kpis",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Kpis");

            migrationBuilder.DropColumn(
                name: "CreationDate",
                table: "Kpis");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "Kpis");
        }
    }
}
