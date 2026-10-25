namespace Campfire.Modules.StrangerChat.Application.Models;
public sealed record MessageModel(
    Guid Id,
    Guid ConversationId,
    Guid ClientMessageId,
    string Kind,
    string Content,
    DateTimeOffset SentAtUtc,
    bool IsMine,
    MessageReplyModel? ReplyTo,
    IReadOnlyList<MessageReactionModel> Reactions,
    string? MyReaction);
