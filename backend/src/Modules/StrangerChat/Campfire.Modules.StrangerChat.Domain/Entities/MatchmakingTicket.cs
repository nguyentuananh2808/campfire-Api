namespace Campfire.Modules.StrangerChat.Domain.Entities;

public sealed class MatchmakingTicket
{
    private MatchmakingTicket() { }
    private MatchmakingTicket(Guid sessionId, DateTimeOffset now) { Id = Guid.NewGuid(); SessionId = sessionId; QueuedAtUtc = now; ExpiresAtUtc = now.AddMinutes(2); }
    public Guid Id { get; private set; }
    public Guid SessionId { get; private set; }
    public DateTimeOffset QueuedAtUtc { get; private set; }
    public DateTimeOffset ExpiresAtUtc { get; private set; }
    public static MatchmakingTicket Create(Guid sessionId, DateTimeOffset now) => new(sessionId, now);
}
