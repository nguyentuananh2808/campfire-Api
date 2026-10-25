namespace Campfire.Modules.Chat.Presentation.Models;

public sealed record SendMessageRequest(Guid ClientMessageId, string Content, string? Kind = null, Guid? ReplyToMessageId = null);
