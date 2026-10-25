namespace Campfire.Modules.StrangerChat.Presentation.Models; public sealed record SendStrangerMessageRequest(Guid ClientMessageId, string Content, string? Kind = null, Guid? ReplyToMessageId = null);
