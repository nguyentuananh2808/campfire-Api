using Campfire.Modules.Werewolf.Application.Common;
using Campfire.Modules.Werewolf.Application.Models;

namespace Campfire.Modules.Werewolf.Application.Abstractions;

public interface IWerewolfService
{
    Task<WerewolfResult<IReadOnlyList<WerewolfRoomSummary>>> ListPublicAsync(Guid userId, CancellationToken cancellationToken);
    Task<WerewolfResult<WerewolfRoomSnapshot>> CreateAsync(Guid userId, string name, string visibility, int minimumPlayers, int maximumPlayers, bool deadVoiceEnabled, CancellationToken cancellationToken);
    Task<WerewolfResult<WerewolfRoomSnapshot>> JoinAsync(Guid userId, string code, bool spectator, CancellationToken cancellationToken);
    Task<WerewolfResult<WerewolfRoomSnapshot>> GetAsync(Guid roomId, Guid userId, CancellationToken cancellationToken);
    Task<WerewolfResult<WerewolfRoomSnapshot>> UpdateAsync(Guid roomId, Guid userId, Guid expectedVersion, string name, string visibility, int minimumPlayers, int maximumPlayers, bool deadVoiceEnabled, CancellationToken cancellationToken);
    Task<WerewolfResult<WerewolfRoomSnapshot>> SetReadyAsync(Guid roomId, Guid userId, bool ready, CancellationToken cancellationToken);
    Task<WerewolfResult<WerewolfRoomSnapshot>> StartAsync(Guid roomId, Guid userId, Guid expectedVersion, CancellationToken cancellationToken);
    Task<WerewolfResult<WerewolfRoomSnapshot>> SubmitActionAsync(Guid gameId, Guid userId, Guid clientActionId, string kind, Guid? targetUserId, CancellationToken cancellationToken);
    Task<WerewolfResult<WerewolfRoomSnapshot>> SubmitVoteAsync(Guid gameId, Guid userId, Guid clientActionId, Guid? targetUserId, CancellationToken cancellationToken);
    Task<WerewolfResult<bool>> LeaveAsync(Guid roomId, Guid userId, CancellationToken cancellationToken);
    Task<WerewolfResult<VoiceAccessView>> IssueVoiceAccessAsync(Guid roomId, Guid userId, CancellationToken cancellationToken);
    Task<WerewolfResult<bool>> SetConnectionAsync(Guid roomId, Guid userId, bool connected, CancellationToken cancellationToken);
    Task<WerewolfResult<WerewolfRoomSnapshot>> ModerateAsync(Guid roomId, Guid userId, Guid targetUserId, string action, CancellationToken cancellationToken);
    Task AdvanceDueGamesAsync(CancellationToken cancellationToken);
}
