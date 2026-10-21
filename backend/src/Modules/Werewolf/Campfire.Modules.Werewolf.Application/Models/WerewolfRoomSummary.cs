namespace Campfire.Modules.Werewolf.Application.Models;

public sealed record WerewolfRoomSummary(Guid Id, string Code, string Name, string Visibility, string Status,
    int PlayerCount, int MaximumPlayers, DateTimeOffset CreatedAtUtc);
