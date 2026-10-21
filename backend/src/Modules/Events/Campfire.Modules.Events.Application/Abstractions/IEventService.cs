using Campfire.Modules.Events.Application.Common;
using Campfire.Modules.Events.Application.Models;

namespace Campfire.Modules.Events.Application.Abstractions;

public interface IEventService
{
    Task<EventResult<EventDetail>> CreateAsync(Guid groupId, Guid userId, string title, string description,
        DateTimeOffset startsAtUtc, string location, CancellationToken cancellationToken);
    Task<EventResult<IReadOnlyList<EventSummary>>> ListAsync(Guid groupId, Guid userId, int limit, CancellationToken cancellationToken);
    Task<EventResult<EventDetail>> GetAsync(Guid groupId, Guid eventId, Guid userId, CancellationToken cancellationToken);
    Task<EventResult<EventDetail>> UpdateAsync(Guid groupId, Guid eventId, Guid userId, Guid expectedVersion,
        string title, string description, DateTimeOffset startsAtUtc, string location, CancellationToken cancellationToken);
    Task<EventResult<EventDetail>> ChangeStatusAsync(Guid groupId, Guid eventId, Guid userId, Guid expectedVersion,
        string status, string traceId, CancellationToken cancellationToken);
    Task<EventResult<EventDetail>> SetRsvpAsync(Guid groupId, Guid eventId, Guid userId, string response, CancellationToken cancellationToken);
}
