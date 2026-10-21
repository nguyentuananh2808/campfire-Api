namespace Campfire.Modules.StrangerChat.Domain.Entities;

public sealed class StrangerBlock
{
    private StrangerBlock() { }
    public StrangerBlock(Guid blockerSessionId, Guid blockedSessionId, DateTimeOffset now) { Id = Guid.NewGuid(); BlockerSessionId = blockerSessionId; BlockedSessionId = blockedSessionId; CreatedAtUtc = now; ExpiresAtUtc = now.AddDays(30); }
    public Guid Id { get; private set; }
    public Guid BlockerSessionId { get; private set; }
    public Guid BlockedSessionId { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset ExpiresAtUtc { get; private set; }
}
