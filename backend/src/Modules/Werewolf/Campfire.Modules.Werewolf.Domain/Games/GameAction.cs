namespace Campfire.Modules.Werewolf.Domain.Games;

public sealed class GameAction
{
    private GameAction() { }
    public GameAction(Guid gameId, int phaseSequence, Guid actorUserId, Guid clientActionId,
        GameActionKind kind, Guid? targetUserId, DateTimeOffset createdAtUtc)
    {
        Id = Guid.NewGuid(); GameId = gameId; PhaseSequence = phaseSequence; ActorUserId = actorUserId;
        ClientActionId = clientActionId; Kind = kind; TargetUserId = targetUserId; CreatedAtUtc = createdAtUtc;
    }
    public Guid Id { get; private set; }
    public Guid GameId { get; private set; }
    public int PhaseSequence { get; private set; }
    public Guid ActorUserId { get; private set; }
    public Guid ClientActionId { get; private set; }
    public GameActionKind Kind { get; private set; }
    public Guid? TargetUserId { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
}
