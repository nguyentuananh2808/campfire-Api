namespace Campfire.Modules.StrangerChat.Domain.Entities;

public sealed class StrangerMessage
{
    private StrangerMessage() { }
    private StrangerMessage(Guid conversationId, Guid senderSessionId, Guid clientMessageId, string content, DateTimeOffset now)
    { Id = Guid.NewGuid(); ConversationId = conversationId; SenderSessionId = senderSessionId; ClientMessageId = clientMessageId; Content = content.Trim(); SentAtUtc = now; }
    public Guid Id { get; private set; }
    public Guid ConversationId { get; private set; }
    public Guid SenderSessionId { get; private set; }
    public Guid ClientMessageId { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public DateTimeOffset SentAtUtc { get; private set; }
    public static StrangerMessage Create(Guid conversationId, Guid senderSessionId, Guid clientMessageId, string content, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(content) || content.Length > 1000) throw new ArgumentException("Message must contain at most 1,000 characters.");
        return new StrangerMessage(conversationId, senderSessionId, clientMessageId, content, now);
    }
}
