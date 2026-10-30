namespace Campfire.Modules.StrangerChat.Domain.Entities;

public sealed class StrangerMessage
{
    private StrangerMessage() { }
    private StrangerMessage(Guid conversationId, Guid senderSessionId, Guid clientMessageId, StrangerMessageKind kind, string content, Guid? replyToMessageId, DateTimeOffset now)
    { Id = Guid.NewGuid(); ConversationId = conversationId; SenderSessionId = senderSessionId; ClientMessageId = clientMessageId; Kind = kind; Content = Validate(kind, content); ReplyToMessageId = replyToMessageId; SentAtUtc = now; Version = Guid.NewGuid(); }
    public Guid Id { get; private set; }
    public Guid ConversationId { get; private set; }
    public Guid SenderSessionId { get; private set; }
    public Guid ClientMessageId { get; private set; }
    public StrangerMessageKind Kind { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public Guid? ReplyToMessageId { get; private set; }
    public DateTimeOffset SentAtUtc { get; private set; }
    public DateTimeOffset? EditedAtUtc { get; private set; }
    public Guid Version { get; private set; }
    public void Edit(string content, DateTimeOffset editedAtUtc)
    {
        if (Kind is not StrangerMessageKind.Text) throw new InvalidOperationException("Only text messages can be edited.");
        Content = Validate(StrangerMessageKind.Text, content).Trim();
        EditedAtUtc = editedAtUtc;
        Version = Guid.NewGuid();
    }
    public static StrangerMessage Create(Guid conversationId, Guid senderSessionId, Guid clientMessageId, string content, DateTimeOffset now)
    {
        return new StrangerMessage(conversationId, senderSessionId, clientMessageId, StrangerMessageKind.Text, content, null, now);
    }

    public static StrangerMessage Create(Guid conversationId, Guid senderSessionId, Guid clientMessageId, StrangerMessageKind kind, string content, Guid? replyToMessageId, DateTimeOffset now) =>
        new(conversationId, senderSessionId, clientMessageId, kind, content, replyToMessageId, now);

    private static string Validate(StrangerMessageKind kind, string content)
    {
        ArgumentNullException.ThrowIfNull(content);
        if (kind is StrangerMessageKind.Text)
        {
            if (string.IsNullOrWhiteSpace(content) || content.Length > 1000)
                throw new ArgumentException("Message must contain at most 1,000 characters.");
            return content.Trim();
        }

        if (kind is StrangerMessageKind.Sticker && StrangerStickerCatalog.Contains(content)) return content;
        throw new ArgumentException("Sticker code is not supported.");
    }
}
