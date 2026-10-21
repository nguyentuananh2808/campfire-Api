namespace Campfire.Modules.StrangerChat.Application.Models;
public sealed record MessageModel(Guid Id, Guid ConversationId, Guid ClientMessageId, string Content, DateTimeOffset SentAtUtc, bool IsMine);
