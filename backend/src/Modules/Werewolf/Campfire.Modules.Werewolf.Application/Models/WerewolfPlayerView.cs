namespace Campfire.Modules.Werewolf.Application.Models;

public sealed record WerewolfPlayerView(Guid UserId, string DisplayName, int Seat, bool IsHost, bool IsReady,
    bool IsConnected, bool IsAlive, bool IsSpeaking, bool IsHostMuted, string Kind);
