using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tenor.Migrations
{
    /// <inheritdoc />
    public partial class SetCascadetoNullOperation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropForeignKey(
            name: "FK_Kpis_Operations_OperationId",
            table: "Kpis");

            migrationBuilder.AddForeignKey(
               name: "FK_Kpis_Operations_OperationId",
               table: "Kpis",
               column: "OperationId",
               principalTable: "Operations",
               principalColumn: "Id",
               onDelete: ReferentialAction.NoAction);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
