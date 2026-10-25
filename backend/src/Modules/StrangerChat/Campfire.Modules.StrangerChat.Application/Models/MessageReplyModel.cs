namespace Campfire.Modules.StrangerChat.Application.Models;

public sealed record MessageReplyModel(Guid MessageId, bool IsMine, string Kind, string Content);
