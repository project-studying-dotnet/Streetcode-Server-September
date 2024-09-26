using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Streetcode.DAL.Persistence.Migrations
{
    public partial class Commententitychanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        { 
            migrationBuilder.AddColumn<int>(
                name: "ParentCommentId",
                schema: "streetcode",
                table: "comments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_comments_ParentCommentId",
                schema: "streetcode",
                table: "comments",
                column: "ParentCommentId");

            migrationBuilder.AddForeignKey(
                name: "FK_comments_comments_ParentCommentId",
                schema: "streetcode",
                table: "comments",
                column: "ParentCommentId",
                principalSchema: "streetcode",
                principalTable: "comments",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_comments_comments_ParentCommentId",
                schema: "streetcode",
                table: "comments");

            migrationBuilder.DropIndex(
                name: "IX_comments_ParentCommentId",
                schema: "streetcode",
                table: "comments");

            migrationBuilder.DropColumn(
                name: "ParentCommentId",
                schema: "streetcode",
                table: "comments");          
        }
    }
}
