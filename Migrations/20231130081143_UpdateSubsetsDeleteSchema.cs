using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tenor.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSubsetsDeleteSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Schema",
                table: "Subsets");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Schema",
                table: "Subsets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
