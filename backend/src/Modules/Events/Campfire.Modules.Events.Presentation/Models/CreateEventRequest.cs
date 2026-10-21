namespace Campfire.Modules.Events.Presentation.Models;

public sealed record CreateEventRequest(string Title, string Description, DateTimeOffset StartsAtUtc, string Location);
