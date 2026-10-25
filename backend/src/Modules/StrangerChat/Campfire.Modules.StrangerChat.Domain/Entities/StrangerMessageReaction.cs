namespace Campfire.Modules.StrangerChat.Domain.Entities;

public sealed class StrangerMessageReaction
{
    private StrangerMessageReaction() { }

    private StrangerMessageReaction(Guid messageId, Guid sessionId, string reaction, DateTimeOffset reactedAtUtc)
    {
        MessageId = messageId;
        SessionId = sessionId;
        Change(reaction, reactedAtUtc);
    }

    public Guid MessageId { get; private set; }
    public Guid SessionId { get; private set; }
    public string Reaction { get; private set; } = string.Empty;
    public DateTimeOffset ReactedAtUtc { get; private set; }

    public static StrangerMessageReaction Create(Guid messageId, Guid sessionId, string reaction, DateTimeOffset reactedAtUtc) =>
        new(messageId, sessionId, reaction, reactedAtUtc);

    public void Change(string reaction, DateTimeOffset reactedAtUtc)
    {
        if (!StrangerReactionCatalog.Contains(reaction))
            throw new ArgumentException("Reaction code is not supported.", nameof(reaction));
        Reaction = reaction;
        ReactedAtUtc = reactedAtUtc;
    }
}
