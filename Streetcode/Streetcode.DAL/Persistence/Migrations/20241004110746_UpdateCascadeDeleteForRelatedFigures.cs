using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Streetcode.DAL.Persistence.Migrations
{
    public partial class UpdateCascadeDeleteForRelatedFigures : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_related_figures_streetcodes_ObserverId",
                schema: "streetcode",
                table: "related_figures");

            migrationBuilder.AddForeignKey(
                name: "FK_related_figures_streetcodes_ObserverId",
                schema: "streetcode",
                table: "related_figures",
                column: "ObserverId",
                principalSchema: "streetcode",
                principalTable: "streetcodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_related_figures_streetcodes_ObserverId",
                schema: "streetcode",
                table: "related_figures");

            migrationBuilder.AddForeignKey(
                name: "FK_related_figures_streetcodes_ObserverId",
                schema: "streetcode",
                table: "related_figures",
                column: "ObserverId",
                principalSchema: "streetcode",
                principalTable: "streetcodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
