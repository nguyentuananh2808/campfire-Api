using Campfire.Modules.Events.Application.Abstractions;
using Campfire.Modules.Events.Application.Common;
using Campfire.Modules.Events.Application.Models;
using MediatR;

namespace Campfire.Modules.Events.Application.ListEvents;

public sealed class ListEventsQueryHandler(IEventService service) : IRequestHandler<ListEventsQuery, EventResult<IReadOnlyList<EventSummary>>>
{
    public Task<EventResult<IReadOnlyList<EventSummary>>> Handle(ListEventsQuery request, CancellationToken cancellationToken) =>
        service.ListAsync(request.GroupId, request.UserId, request.Limit, cancellationToken);
}
