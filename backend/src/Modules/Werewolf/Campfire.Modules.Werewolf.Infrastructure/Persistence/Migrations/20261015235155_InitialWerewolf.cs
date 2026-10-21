using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Campfire.Modules.Werewolf.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialWerewolf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "werewolf");

            migrationBuilder.CreateTable(
                name: "Rooms",
                schema: "werewolf",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HostUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    Name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Visibility = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    MinimumPlayers = table.Column<int>(type: "integer", nullable: false),
                    MaximumPlayers = table.Column<int>(type: "integer", nullable: false),
                    DeadVoiceEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Games",
                schema: "werewolf",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    Phase = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    Round = table.Column<int>(type: "integer", nullable: false),
                    PhaseSequence = table.Column<int>(type: "integer", nullable: false),
                    PhaseEndsAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<Guid>(type: "uuid", nullable: false),
                    Winner = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    StartedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FinishedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Games_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalSchema: "werewolf",
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoomPlayers",
                schema: "werewolf",
                columns: table => new
                {
                    RoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    IsReady = table.Column<bool>(type: "boolean", nullable: false),
                    IsConnected = table.Column<bool>(type: "boolean", nullable: false),
                    IsHostMuted = table.Column<bool>(type: "boolean", nullable: false),
                    JoinedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastSeenAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomPlayers", x => new { x.RoomId, x.UserId });
                    table.ForeignKey(
                        name: "FK_RoomPlayers_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalSchema: "werewolf",
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GameActions",
                schema: "werewolf",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GameId = table.Column<Guid>(type: "uuid", nullable: false),
                    PhaseSequence = table.Column<int>(type: "integer", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientActionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TargetUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameActions_Games_GameId",
                        column: x => x.GameId,
                        principalSchema: "werewolf",
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GameEvents",
                schema: "werewolf",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GameId = table.Column<Guid>(type: "uuid", nullable: false),
                    PhaseSequence = table.Column<int>(type: "integer", nullable: false),
                    Kind = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    RecipientUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameEvents_Games_GameId",
                        column: x => x.GameId,
                        principalSchema: "werewolf",
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GamePlayers",
                schema: "werewolf",
                columns: table => new
                {
                    GameId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Seat = table.Column<int>(type: "integer", nullable: false),
                    IsAlive = table.Column<bool>(type: "boolean", nullable: false),
                    HasHealPotion = table.Column<bool>(type: "boolean", nullable: false),
                    HasPoisonPotion = table.Column<bool>(type: "boolean", nullable: false),
                    LastProtectedUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GamePlayers", x => new { x.GameId, x.UserId });
                    table.ForeignKey(
                        name: "FK_GamePlayers_Games_GameId",
                        column: x => x.GameId,
                        principalSchema: "werewolf",
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GameVotes",
                schema: "werewolf",
                columns: table => new
                {
                    GameId = table.Column<Guid>(type: "uuid", nullable: false),
                    PhaseSequence = table.Column<int>(type: "integer", nullable: false),
                    VoterUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientActionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameVotes", x => new { x.GameId, x.PhaseSequence, x.VoterUserId });
                    table.ForeignKey(
                        name: "FK_GameVotes_Games_GameId",
                        column: x => x.GameId,
                        principalSchema: "werewolf",
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameActions_GameId_ActorUserId_ClientActionId",
                schema: "werewolf",
                table: "GameActions",
                columns: new[] { "GameId", "ActorUserId", "ClientActionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameActions_GameId_PhaseSequence_ActorUserId",
                schema: "werewolf",
                table: "GameActions",
                columns: new[] { "GameId", "PhaseSequence", "ActorUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameEvents_GameId_OccurredAtUtc_Id",
                schema: "werewolf",
                table: "GameEvents",
                columns: new[] { "GameId", "OccurredAtUtc", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_GamePlayers_GameId_Seat",
                schema: "werewolf",
                table: "GamePlayers",
                columns: new[] { "GameId", "Seat" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Games_Phase_PhaseEndsAtUtc",
                schema: "werewolf",
                table: "Games",
                columns: new[] { "Phase", "PhaseEndsAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Games_RoomId",
                schema: "werewolf",
                table: "Games",
                column: "RoomId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameVotes_GameId_VoterUserId_ClientActionId",
                schema: "werewolf",
                table: "GameVotes",
                columns: new[] { "GameId", "VoterUserId", "ClientActionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoomPlayers_UserId",
                schema: "werewolf",
                table: "RoomPlayers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_Code",
                schema: "werewolf",
                table: "Rooms",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_Visibility_Status_CreatedAtUtc",
                schema: "werewolf",
                table: "Rooms",
                columns: new[] { "Visibility", "Status", "CreatedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameActions",
                schema: "werewolf");

            migrationBuilder.DropTable(
                name: "GameEvents",
                schema: "werewolf");

            migrationBuilder.DropTable(
                name: "GamePlayers",
                schema: "werewolf");

            migrationBuilder.DropTable(
                name: "GameVotes",
                schema: "werewolf");

            migrationBuilder.DropTable(
                name: "RoomPlayers",
                schema: "werewolf");

            migrationBuilder.DropTable(
                name: "Games",
                schema: "werewolf");

            migrationBuilder.DropTable(
                name: "Rooms",
                schema: "werewolf");
        }
    }
}
