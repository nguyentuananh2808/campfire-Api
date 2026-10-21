using Campfire.Modules.Events.Application.Abstractions;
using Campfire.Modules.Events.Application.Common;
using Campfire.Modules.Events.Application.Models;
using MediatR;

namespace Campfire.Modules.Events.Application.SetRsvp;

public sealed class SetRsvpCommandHandler(IEventService service) : IRequestHandler<SetRsvpCommand, EventResult<EventDetail>>
{
    public Task<EventResult<EventDetail>> Handle(SetRsvpCommand request, CancellationToken cancellationToken) =>
        service.SetRsvpAsync(request.GroupId, request.EventId, request.UserId, request.Response, cancellationToken);
}
