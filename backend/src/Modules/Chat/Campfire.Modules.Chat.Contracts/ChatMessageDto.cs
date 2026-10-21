namespace Campfire.Modules.Chat.Contracts;

public sealed record ChatMessageDto(
    Guid Id,
    Guid GroupId,
    Guid SenderUserId,
    string SenderDisplayName,
    Guid ClientMessageId,
    string Kind,
    string Content,
    DateTimeOffset SentAtUtc);
