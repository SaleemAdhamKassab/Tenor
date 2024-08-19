using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tenor.Migrations
{
    /// <inheritdoc />
    public partial class Add_Set_To_Subset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SetId",
                table: "Subsets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SetName",
                table: "Subsets",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SetId",
                table: "Subsets");

            migrationBuilder.DropColumn(
                name: "SetName",
                table: "Subsets");
        }
    }
}
