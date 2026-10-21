using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Campfire.Modules.Chat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddChatMessageKinds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Kind",
                schema: "chat",
                table: "Messages",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "Text");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Messages_Kind",
                schema: "chat",
                table: "Messages",
                sql: "\"Kind\" IN ('Text', 'Sticker')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Messages_Kind",
                schema: "chat",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "Kind",
                schema: "chat",
                table: "Messages");
        }
    }
}
