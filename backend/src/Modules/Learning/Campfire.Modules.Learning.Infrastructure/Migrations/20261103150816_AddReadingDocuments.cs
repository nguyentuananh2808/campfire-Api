using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Campfire.Modules.Learning.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReadingDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "reading_documents",
                schema: "learning",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Content = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: false),
                    CharacterCount = table.Column<int>(type: "integer", nullable: false),
                    ChineseCharacterCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reading_documents", x => x.Id);
                    table.CheckConstraint("CK_reading_documents_CharacterCount", "\"CharacterCount\" BETWEEN 1 AND 10000");
                    table.CheckConstraint("CK_reading_documents_ChineseCharacterCount", "\"ChineseCharacterCount\" >= 0");
                });

            migrationBuilder.CreateIndex(
                name: "IX_reading_documents_UserId_CreatedAtUtc",
                schema: "learning",
                table: "reading_documents",
                columns: new[] { "UserId", "CreatedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "reading_documents",
                schema: "learning");
        }
    }
}
