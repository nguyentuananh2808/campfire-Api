using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Campfire.Modules.Chat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddChatReadStates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReadStates",
                schema: "chat",
                columns: table => new
                {
                    GroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastReadAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReadStates", x => new { x.GroupId, x.UserId });
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReadStates_UserId",
                schema: "chat",
                table: "ReadStates",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReadStates",
                schema: "chat");
        }
    }
}
