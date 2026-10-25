using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Campfire.Modules.StrangerChat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CompleteChatInteractions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Kind",
                schema: "stranger_chat",
                table: "messages",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "Text");

            migrationBuilder.AddColumn<Guid>(
                name: "ReplyToMessageId",
                schema: "stranger_chat",
                table: "messages",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "message_reactions",
                schema: "stranger_chat",
                columns: table => new
                {
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reaction = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    ReactedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_message_reactions", x => new { x.MessageId, x.SessionId });
                    table.CheckConstraint("CK_message_reactions_Reaction", "\"Reaction\" IN ('Like', 'Love', 'Laugh', 'Wow', 'Sad')");
                    table.ForeignKey(
                        name: "FK_message_reactions_messages_MessageId",
                        column: x => x.MessageId,
                        principalSchema: "stranger_chat",
                        principalTable: "messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_messages_ReplyToMessageId",
                schema: "stranger_chat",
                table: "messages",
                column: "ReplyToMessageId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_messages_Kind",
                schema: "stranger_chat",
                table: "messages",
                sql: "\"Kind\" IN ('Text', 'Sticker')");

            migrationBuilder.CreateIndex(
                name: "IX_message_reactions_MessageId_Reaction",
                schema: "stranger_chat",
                table: "message_reactions",
                columns: new[] { "MessageId", "Reaction" });

            migrationBuilder.AddForeignKey(
                name: "FK_messages_messages_ReplyToMessageId",
                schema: "stranger_chat",
                table: "messages",
                column: "ReplyToMessageId",
                principalSchema: "stranger_chat",
                principalTable: "messages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_messages_messages_ReplyToMessageId",
                schema: "stranger_chat",
                table: "messages");

            migrationBuilder.DropTable(
                name: "message_reactions",
                schema: "stranger_chat");

            migrationBuilder.DropIndex(
                name: "IX_messages_ReplyToMessageId",
                schema: "stranger_chat",
                table: "messages");

            migrationBuilder.DropCheckConstraint(
                name: "CK_messages_Kind",
                schema: "stranger_chat",
                table: "messages");

            migrationBuilder.DropColumn(
                name: "Kind",
                schema: "stranger_chat",
                table: "messages");

            migrationBuilder.DropColumn(
                name: "ReplyToMessageId",
                schema: "stranger_chat",
                table: "messages");
        }
    }
}
