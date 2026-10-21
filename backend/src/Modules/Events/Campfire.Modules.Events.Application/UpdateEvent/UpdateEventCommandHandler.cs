using Campfire.Modules.Events.Application.Abstractions;
using Campfire.Modules.Events.Application.Common;
using Campfire.Modules.Events.Application.Models;
using MediatR;

namespace Campfire.Modules.Events.Application.UpdateEvent;

public sealed class UpdateEventCommandHandler(IEventService service) : IRequestHandler<UpdateEventCommand, EventResult<EventDetail>>
{
    public Task<EventResult<EventDetail>> Handle(UpdateEventCommand request, CancellationToken cancellationToken) =>
        service.UpdateAsync(request.GroupId, request.EventId, request.UserId, request.ExpectedVersion,
            request.Title, request.Description, request.StartsAtUtc, request.Location, cancellationToken);
}
