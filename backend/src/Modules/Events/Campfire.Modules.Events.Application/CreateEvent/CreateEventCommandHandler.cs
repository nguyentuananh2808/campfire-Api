using Campfire.Modules.Events.Application.Abstractions;
using Campfire.Modules.Events.Application.Common;
using Campfire.Modules.Events.Application.Models;
using MediatR;

namespace Campfire.Modules.Events.Application.CreateEvent;

public sealed class CreateEventCommandHandler(IEventService service) : IRequestHandler<CreateEventCommand, EventResult<EventDetail>>
{
    public Task<EventResult<EventDetail>> Handle(CreateEventCommand request, CancellationToken cancellationToken) =>
        service.CreateAsync(request.GroupId, request.UserId, request.Title, request.Description,
            request.StartsAtUtc, request.Location, cancellationToken);
}
