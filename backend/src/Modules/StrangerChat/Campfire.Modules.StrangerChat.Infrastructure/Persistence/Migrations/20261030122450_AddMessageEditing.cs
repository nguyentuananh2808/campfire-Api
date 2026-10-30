using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Campfire.Modules.StrangerChat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMessageEditing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EditedAtUtc",
                schema: "stranger_chat",
                table: "messages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Version",
                schema: "stranger_chat",
                table: "messages",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EditedAtUtc",
                schema: "stranger_chat",
                table: "messages");

            migrationBuilder.DropColumn(
                name: "Version",
                schema: "stranger_chat",
                table: "messages");
        }
    }
}
