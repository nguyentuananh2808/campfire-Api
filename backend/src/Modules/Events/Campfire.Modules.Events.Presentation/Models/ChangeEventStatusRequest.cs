namespace Campfire.Modules.Events.Presentation.Models;

public sealed record ChangeEventStatusRequest(string Status, Guid ExpectedVersion);
