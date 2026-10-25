namespace Campfire.Modules.Chat.Contracts;

public sealed record ChatReplyPreviewDto(
    Guid MessageId,
    string SenderDisplayName,
    string Kind,
    string Content);
