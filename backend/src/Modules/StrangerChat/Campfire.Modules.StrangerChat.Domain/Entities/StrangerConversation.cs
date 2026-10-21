namespace Campfire.Modules.StrangerChat.Domain.Entities;

public sealed class StrangerConversation
{
    private StrangerConversation() { }
    private StrangerConversation(Guid first, Guid second, DateTimeOffset now) { Id = Guid.NewGuid(); FirstSessionId = first; SecondSessionId = second; StartedAtUtc = now; Version = Guid.NewGuid(); }
    public Guid Id { get; private set; }
    public Guid FirstSessionId { get; private set; }
    public Guid SecondSessionId { get; private set; }
    public DateTimeOffset StartedAtUtc { get; private set; }
    public DateTimeOffset? EndedAtUtc { get; private set; }
    public Guid Version { get; private set; }
    public bool IsActive => EndedAtUtc is null;
    public bool Contains(Guid sessionId) => FirstSessionId == sessionId || SecondSessionId == sessionId;
    public Guid PartnerOf(Guid sessionId) => FirstSessionId == sessionId ? SecondSessionId : FirstSessionId;
    public static StrangerConversation Create(Guid first, Guid second, DateTimeOffset now)
    {
        if (first == second) throw new ArgumentException("A session cannot match itself.");
        return new StrangerConversation(first, second, now);
    }
    public void End(DateTimeOffset now) { if (EndedAtUtc is null) { EndedAtUtc = now; Version = Guid.NewGuid(); } }
}
