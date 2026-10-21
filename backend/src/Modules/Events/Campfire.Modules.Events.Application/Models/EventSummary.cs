namespace Campfire.Modules.Events.Application.Models;

public sealed record EventSummary(Guid Id, Guid GroupId, Guid CreatorUserId, string Title,
    DateTimeOffset StartsAtUtc, string Location, string Status, Guid Version,
    int GoingCount, int NotGoingCount, string? MyResponse);
