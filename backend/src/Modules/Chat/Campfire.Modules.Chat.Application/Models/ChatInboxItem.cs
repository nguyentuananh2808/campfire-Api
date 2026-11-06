namespace Campfire.Modules.Chat.Application.Models;

public sealed record ChatInboxItem(
    Guid GroupId,
    Guid? LastMessageId,
    string? LastMessageSenderDisplayName,
    string? LastMessageKind,
    string? LastMessagePreview,
    DateTimeOffset? LastMessageAtUtc,
    int UnreadCount);
