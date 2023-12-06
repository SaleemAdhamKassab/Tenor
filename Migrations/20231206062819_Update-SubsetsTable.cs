using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tenor.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSubsetsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DimensionTable",
                table: "Subsets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FactDimensionReference",
                table: "Subsets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GranularityPeriod",
                table: "Subsets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JoinExpression",
                table: "Subsets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "LoadPriorety",
                table: "Subsets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StartChar",
                table: "Subsets",
                type: "nvarchar(1)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SummaryType",
                table: "Subsets",
                type: "nvarchar(max)",
                nullable: true);

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DimensionTable",
                table: "Subsets");

            migrationBuilder.DropColumn(
                name: "FactDimensionReference",
                table: "Subsets");

            migrationBuilder.DropColumn(
                name: "GranularityPeriod",
                table: "Subsets");

            migrationBuilder.DropColumn(
                name: "JoinExpression",
                table: "Subsets");

            migrationBuilder.DropColumn(
                name: "LoadPriorety",
                table: "Subsets");

            migrationBuilder.DropColumn(
                name: "StartChar",
                table: "Subsets");

            migrationBuilder.DropColumn(
                name: "SummaryType",
                table: "Subsets");

            migrationBuilder.DropColumn(
                name: "Technology",
                table: "Subsets");

            migrationBuilder.DropColumn(
                name: "TechnologyId",
                table: "Subsets");
        }
    }
}
