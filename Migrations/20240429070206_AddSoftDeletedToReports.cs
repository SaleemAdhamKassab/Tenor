using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tenor.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeletedToReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Reports",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Reports");
        }
    }
}
