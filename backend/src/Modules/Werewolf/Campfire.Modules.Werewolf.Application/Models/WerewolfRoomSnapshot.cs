namespace Campfire.Modules.Werewolf.Application.Models;

public sealed record WerewolfRoomSnapshot(Guid Id, string Code, string Name, string Visibility, string Status,
    Guid HostUserId, int MinimumPlayers, int MaximumPlayers, bool DeadVoiceEnabled, Guid Version,
    Guid? GameId, string? Phase, int Round, int PhaseSequence, DateTimeOffset? PhaseEndsAtUtc, string? Winner,
    IReadOnlyList<WerewolfPlayerView> Players, PrivateRoleView? MyRole, IReadOnlyList<GameEventView> Events,
    bool CanManage, bool CanStart, bool CanAct, bool CanVote, int VotesSubmitted, int EligibleVoters);
