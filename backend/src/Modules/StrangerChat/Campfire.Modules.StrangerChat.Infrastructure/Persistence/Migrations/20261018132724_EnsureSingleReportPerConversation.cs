using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Campfire.Modules.StrangerChat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnsureSingleReportPerConversation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "UX_reports_conversation_reporter",
                schema: "stranger_chat",
                table: "reports",
                columns: new[] { "ConversationId", "ReporterSessionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_reports_conversation_reporter",
                schema: "stranger_chat",
                table: "reports");
        }
    }
}
