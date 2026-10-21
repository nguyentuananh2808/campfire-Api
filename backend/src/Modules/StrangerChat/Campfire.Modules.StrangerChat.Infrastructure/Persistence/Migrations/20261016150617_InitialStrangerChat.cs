using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Campfire.Modules.StrangerChat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialStrangerChat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "stranger_chat");

            migrationBuilder.CreateTable(
                name: "blocks",
                schema: "stranger_chat",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BlockerSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    BlockedSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_blocks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "conversations",
                schema: "stranger_chat",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SecondSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Version = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_conversations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "guest_sessions",
                schema: "stranger_chat",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Nickname = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastSeenAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guest_sessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "matchmaking_tickets",
                schema: "stranger_chat",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    QueuedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_matchmaking_tickets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "messages",
                schema: "stranger_chat",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SenderSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientMessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    SentAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_messages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "reports",
                schema: "stranger_chat",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReporterSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportedSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RetainUntilUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reports", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_blocks_BlockerSessionId_BlockedSessionId",
                schema: "stranger_chat",
                table: "blocks",
                columns: new[] { "BlockerSessionId", "BlockedSessionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_blocks_ExpiresAtUtc",
                schema: "stranger_chat",
                table: "blocks",
                column: "ExpiresAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_conversations_FirstSessionId_EndedAtUtc",
                schema: "stranger_chat",
                table: "conversations",
                columns: new[] { "FirstSessionId", "EndedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_conversations_SecondSessionId_EndedAtUtc",
                schema: "stranger_chat",
                table: "conversations",
                columns: new[] { "SecondSessionId", "EndedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_guest_sessions_TokenHash",
                schema: "stranger_chat",
                table: "guest_sessions",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_matchmaking_tickets_QueuedAtUtc_ExpiresAtUtc",
                schema: "stranger_chat",
                table: "matchmaking_tickets",
                columns: new[] { "QueuedAtUtc", "ExpiresAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_matchmaking_tickets_SessionId",
                schema: "stranger_chat",
                table: "matchmaking_tickets",
                column: "SessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_messages_ConversationId_SenderSessionId_ClientMessageId",
                schema: "stranger_chat",
                table: "messages",
                columns: new[] { "ConversationId", "SenderSessionId", "ClientMessageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_messages_ConversationId_SentAtUtc_Id",
                schema: "stranger_chat",
                table: "messages",
                columns: new[] { "ConversationId", "SentAtUtc", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_reports_RetainUntilUtc",
                schema: "stranger_chat",
                table: "reports",
                column: "RetainUntilUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "blocks",
                schema: "stranger_chat");

            migrationBuilder.DropTable(
                name: "conversations",
                schema: "stranger_chat");

            migrationBuilder.DropTable(
                name: "guest_sessions",
                schema: "stranger_chat");

            migrationBuilder.DropTable(
                name: "matchmaking_tickets",
                schema: "stranger_chat");

            migrationBuilder.DropTable(
                name: "messages",
                schema: "stranger_chat");

            migrationBuilder.DropTable(
                name: "reports",
                schema: "stranger_chat");
        }
    }
}
