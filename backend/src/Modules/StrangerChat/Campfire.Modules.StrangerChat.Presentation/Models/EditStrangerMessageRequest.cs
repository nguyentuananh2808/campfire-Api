namespace Campfire.Modules.StrangerChat.Presentation.Models;

public sealed record EditStrangerMessageRequest(string Content, Guid ExpectedVersion);
