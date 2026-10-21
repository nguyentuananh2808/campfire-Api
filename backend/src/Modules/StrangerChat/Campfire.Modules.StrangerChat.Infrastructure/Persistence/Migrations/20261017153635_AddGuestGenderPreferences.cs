using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Campfire.Modules.StrangerChat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGuestGenderPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Gender",
                schema: "stranger_chat",
                table: "guest_sessions",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "Other");

            migrationBuilder.AddColumn<string>(
                name: "GenderPreference",
                schema: "stranger_chat",
                table: "guest_sessions",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "Any");

            migrationBuilder.CreateIndex(
                name: "IX_guest_sessions_Gender_GenderPreference",
                schema: "stranger_chat",
                table: "guest_sessions",
                columns: new[] { "Gender", "GenderPreference" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_guest_sessions_Gender",
                schema: "stranger_chat",
                table: "guest_sessions",
                sql: "\"Gender\" IN ('Male', 'Female', 'Other')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_guest_sessions_GenderPreference",
                schema: "stranger_chat",
                table: "guest_sessions",
                sql: "\"GenderPreference\" IN ('Any', 'Male', 'Female', 'Other')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_guest_sessions_Gender_GenderPreference",
                schema: "stranger_chat",
                table: "guest_sessions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_guest_sessions_Gender",
                schema: "stranger_chat",
                table: "guest_sessions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_guest_sessions_GenderPreference",
                schema: "stranger_chat",
                table: "guest_sessions");

            migrationBuilder.DropColumn(
                name: "Gender",
                schema: "stranger_chat",
                table: "guest_sessions");

            migrationBuilder.DropColumn(
                name: "GenderPreference",
                schema: "stranger_chat",
                table: "guest_sessions");
        }
    }
}
