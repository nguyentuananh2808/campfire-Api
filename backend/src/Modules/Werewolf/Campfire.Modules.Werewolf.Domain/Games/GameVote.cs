namespace Campfire.Modules.Werewolf.Domain.Games;

public sealed class GameVote
{
    private GameVote() { }
    public GameVote(Guid gameId, int phaseSequence, Guid voterUserId, Guid clientActionId,
        Guid? targetUserId, DateTimeOffset createdAtUtc)
    { GameId = gameId; PhaseSequence = phaseSequence; VoterUserId = voterUserId; ClientActionId = clientActionId; TargetUserId = targetUserId; CreatedAtUtc = createdAtUtc; }
    public Guid GameId { get; private set; }
    public int PhaseSequence { get; private set; }
    public Guid VoterUserId { get; private set; }
    public Guid ClientActionId { get; private set; }
    public Guid? TargetUserId { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
}
