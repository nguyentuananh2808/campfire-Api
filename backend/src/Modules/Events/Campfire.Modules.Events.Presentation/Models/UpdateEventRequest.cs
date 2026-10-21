namespace Campfire.Modules.Events.Presentation.Models;

public sealed record UpdateEventRequest(string Title, string Description, DateTimeOffset StartsAtUtc,
    string Location, Guid ExpectedVersion);
