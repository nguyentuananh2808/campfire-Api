namespace Campfire.Modules.Chat.Presentation.Models;

public sealed record EditMessageRequest(string Content, Guid ExpectedVersion);
