namespace Campfire.Modules.Chat.Domain.Messages;

public sealed class ChatMessage
{
    private ChatMessage() { }

    private ChatMessage(
        Guid groupId,
        Guid senderUserId,
        Guid clientMessageId,
        ChatMessageKind kind,
        string content,
        DateTimeOffset sentAtUtc)
    {
        Id = Guid.NewGuid();
        GroupId = groupId;
        SenderUserId = senderUserId;
        ClientMessageId = clientMessageId;
        Kind = kind;
        Content = ValidateContent(kind, content);
        SentAtUtc = sentAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid GroupId { get; private set; }
    public Guid SenderUserId { get; private set; }
    public Guid ClientMessageId { get; private set; }
    public ChatMessageKind Kind { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public DateTimeOffset SentAtUtc { get; private set; }

    public static ChatMessage Create(Guid groupId, Guid senderUserId, Guid clientMessageId, string content, DateTimeOffset sentAtUtc) =>
        new(groupId, senderUserId, clientMessageId, ChatMessageKind.Text, content, sentAtUtc);

    public static ChatMessage Create(
        Guid groupId,
        Guid senderUserId,
        Guid clientMessageId,
        ChatMessageKind kind,
        string content,
        DateTimeOffset sentAtUtc) =>
        new(groupId, senderUserId, clientMessageId, kind, content, sentAtUtc);

    private static string ValidateContent(ChatMessageKind kind, string content)
    {
        ArgumentNullException.ThrowIfNull(content);
        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind));
        }

        if (kind is ChatMessageKind.Text && (string.IsNullOrWhiteSpace(content) || content.Length > 2000))
        {
            throw new ArgumentException("Message content must contain text and be at most 2,000 characters.", nameof(content));
        }

        if (kind is ChatMessageKind.Sticker && !ChatStickerCatalog.Contains(content))
        {
            throw new ArgumentException("Sticker code is not supported.", nameof(content));
        }

        return content;
    }
}
