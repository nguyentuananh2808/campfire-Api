using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Campfire.Modules.Chat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddImageMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Messages_Kind",
                schema: "chat",
                table: "Messages");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Messages_Kind",
                schema: "chat",
                table: "Messages",
                sql: "\"Kind\" IN ('Text', 'Sticker', 'Image')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Messages_Kind",
                schema: "chat",
                table: "Messages");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Messages_Kind",
                schema: "chat",
                table: "Messages",
                sql: "\"Kind\" IN ('Text', 'Sticker')");
        }
    }
}
