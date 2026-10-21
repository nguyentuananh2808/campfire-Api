using Campfire.Modules.Werewolf.Application.Models;

namespace Campfire.Modules.Werewolf.Application.Abstractions;

public interface IWerewolfVoiceGateway
{
    bool IsConfigured { get; }
    VoiceAccessView IssueAccess(string roomName, Guid userId, string displayName, string channel, bool canPublish, bool canSubscribe, DateTimeOffset now);
    Task ClosePhaseRoomsAsync(Guid roomId, Guid gameId, int phaseSequence, CancellationToken cancellationToken);
}
