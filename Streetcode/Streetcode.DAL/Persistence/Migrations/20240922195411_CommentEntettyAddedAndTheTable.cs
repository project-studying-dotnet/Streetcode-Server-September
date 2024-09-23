using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Streetcode.DAL.Persistence.Migrations
{
    public partial class CommentEntettyAddedAndTheTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Title",
                schema: "timeline",
                table: "timeline_items",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "timeline",
                table: "timeline_items",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(600)",
                oldMaxLength: 600,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "comments",
                schema: "streetcode",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommentContent = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    StreetcodeId = table.Column<int>(type: "int", nullable: false),
                    ParentCommentId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_comments_streetcodes_StreetcodeId",
                        column: x => x.StreetcodeId,
                        principalSchema: "streetcode",
                        principalTable: "streetcodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_comments_StreetcodeId",
                schema: "streetcode",
                table: "comments",
                column: "StreetcodeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "comments",
                schema: "streetcode");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                schema: "timeline",
                table: "timeline_items",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "timeline",
                table: "timeline_items",
                type: "nvarchar(600)",
                maxLength: 600,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
