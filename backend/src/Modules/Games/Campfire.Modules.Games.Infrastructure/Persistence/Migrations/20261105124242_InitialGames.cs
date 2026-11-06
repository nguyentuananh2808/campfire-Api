using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Campfire.Modules.Games.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialGames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "games");

            migrationBuilder.CreateTable(
                name: "CaroGames",
                schema: "games",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    XPlayerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OPlayerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentPlayerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    WinnerUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Board = table.Column<string>(type: "character(9)", fixedLength: true, maxLength: 9, nullable: false),
                    Status = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    Version = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaroGames", x => x.Id);
                    table.CheckConstraint("CK_CaroGames_Board", "length(\"Board\") = 9 AND \"Board\" ~ '^[XO-]{9}$'");
                    table.CheckConstraint("CK_CaroGames_Players", "\"XPlayerUserId\" <> \"OPlayerUserId\"");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CaroGames_GroupId",
                schema: "games",
                table: "CaroGames",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_CaroGames_OPlayerUserId_UpdatedAtUtc",
                schema: "games",
                table: "CaroGames",
                columns: new[] { "OPlayerUserId", "UpdatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_CaroGames_XPlayerUserId_UpdatedAtUtc",
                schema: "games",
                table: "CaroGames",
                columns: new[] { "XPlayerUserId", "UpdatedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CaroGames",
                schema: "games");
        }
    }
}
