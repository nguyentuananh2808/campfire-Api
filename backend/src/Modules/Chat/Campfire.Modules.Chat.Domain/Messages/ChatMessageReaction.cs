namespace Campfire.Modules.Chat.Domain.Messages;

public sealed class ChatMessageReaction
{
    private ChatMessageReaction() { }

    private ChatMessageReaction(Guid messageId, Guid userId, string reaction, DateTimeOffset reactedAtUtc)
    {
        MessageId = messageId;
        UserId = userId;
        SetReaction(reaction, reactedAtUtc);
    }

    public Guid MessageId { get; private set; }
    public Guid UserId { get; private set; }
    public string Reaction { get; private set; } = string.Empty;
    public DateTimeOffset ReactedAtUtc { get; private set; }

    public static ChatMessageReaction Create(Guid messageId, Guid userId, string reaction, DateTimeOffset reactedAtUtc) =>
        new(messageId, userId, reaction, reactedAtUtc);

    public void Change(string reaction, DateTimeOffset reactedAtUtc) => SetReaction(reaction, reactedAtUtc);

    private void SetReaction(string reaction, DateTimeOffset reactedAtUtc)
    {
        if (!ChatReactionCatalog.Contains(reaction))
            throw new ArgumentException("Reaction code is not supported.", nameof(reaction));

        Reaction = reaction;
        ReactedAtUtc = reactedAtUtc;
    }
}
