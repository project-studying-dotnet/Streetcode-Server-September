using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Streetcode.DAL.Persistence.Migrations
{
    public partial class SortOrderInFactsIsUniqueNow : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_facts_SortOrder",
                schema: "streetcode",
                table: "facts",
                column: "SortOrder",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_facts_SortOrder",
                schema: "streetcode",
                table: "facts");
        }
    }
}
