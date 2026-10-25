using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Campfire.Modules.Chat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CompleteChatInteractions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ReplyToMessageId",
                schema: "chat",
                table: "Messages",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MessageReactions",
                schema: "chat",
                columns: table => new
                {
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reaction = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    ReactedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageReactions", x => new { x.MessageId, x.UserId });
                    table.CheckConstraint("CK_MessageReactions_Reaction", "\"Reaction\" IN ('Like', 'Love', 'Laugh', 'Wow', 'Sad')");
                    table.ForeignKey(
                        name: "FK_MessageReactions_Messages_MessageId",
                        column: x => x.MessageId,
                        principalSchema: "chat",
                        principalTable: "Messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ReplyToMessageId",
                schema: "chat",
                table: "Messages",
                column: "ReplyToMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageReactions_MessageId_Reaction",
                schema: "chat",
                table: "MessageReactions",
                columns: new[] { "MessageId", "Reaction" });

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Messages_ReplyToMessageId",
                schema: "chat",
                table: "Messages",
                column: "ReplyToMessageId",
                principalSchema: "chat",
                principalTable: "Messages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Messages_ReplyToMessageId",
                schema: "chat",
                table: "Messages");

            migrationBuilder.DropTable(
                name: "MessageReactions",
                schema: "chat");

            migrationBuilder.DropIndex(
                name: "IX_Messages_ReplyToMessageId",
                schema: "chat",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "ReplyToMessageId",
                schema: "chat",
                table: "Messages");
        }
    }
}
