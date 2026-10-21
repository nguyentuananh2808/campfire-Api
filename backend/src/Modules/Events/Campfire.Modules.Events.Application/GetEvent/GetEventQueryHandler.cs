using Campfire.Modules.Events.Application.Abstractions;
using Campfire.Modules.Events.Application.Common;
using Campfire.Modules.Events.Application.Models;
using MediatR;

namespace Campfire.Modules.Events.Application.GetEvent;

public sealed class GetEventQueryHandler(IEventService service) : IRequestHandler<GetEventQuery, EventResult<EventDetail>>
{
    public Task<EventResult<EventDetail>> Handle(GetEventQuery request, CancellationToken cancellationToken) =>
        service.GetAsync(request.GroupId, request.EventId, request.UserId, cancellationToken);
}
