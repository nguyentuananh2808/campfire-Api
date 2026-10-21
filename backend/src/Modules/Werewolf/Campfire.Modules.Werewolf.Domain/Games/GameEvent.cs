namespace Campfire.Modules.Werewolf.Domain.Games;

public sealed class GameEvent
{
    private GameEvent() { }
    public GameEvent(Guid gameId, int phaseSequence, string kind, string message, Guid? recipientUserId, DateTimeOffset occurredAtUtc)
    { Id = Guid.NewGuid(); GameId = gameId; PhaseSequence = phaseSequence; Kind = kind; Message = message; RecipientUserId = recipientUserId; OccurredAtUtc = occurredAtUtc; }
    public Guid Id { get; private set; }
    public Guid GameId { get; private set; }
    public int PhaseSequence { get; private set; }
    public string Kind { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public Guid? RecipientUserId { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }
}
