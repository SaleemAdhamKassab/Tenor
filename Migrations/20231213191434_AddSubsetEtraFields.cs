using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tenor.Migrations
{
    /// <inheritdoc />
    public partial class AddSubsetEtraFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
                
            migrationBuilder.CreateTable(
                name: "SubsetFields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FieldId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubsetFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubsetFields_ExtraFields_FieldId",
                        column: x => x.FieldId,
                        principalTable: "ExtraFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubsetFieldValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubsetId = table.Column<int>(type: "int", nullable: false),
                    SubsetFieldId = table.Column<int>(type: "int", nullable: false),
                    FieldValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubsetFieldValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubsetFieldValues_SubsetFields_SubsetFieldId",
                        column: x => x.SubsetFieldId,
                        principalTable: "SubsetFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubsetFieldValues_Subsets_SubsetId",
                        column: x => x.SubsetId,
                        principalTable: "Subsets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubsetFields_FieldId",
                table: "SubsetFields",
                column: "FieldId");

            migrationBuilder.CreateIndex(
                name: "IX_SubsetFieldValues_SubsetFieldId",
                table: "SubsetFieldValues",
                column: "SubsetFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_SubsetFieldValues_SubsetId",
                table: "SubsetFieldValues",
                column: "SubsetId");

           
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
          

            migrationBuilder.DropTable(
                name: "SubsetFieldValues");

            migrationBuilder.DropTable(
                name: "SubsetFields");
        
        }
    }
}
