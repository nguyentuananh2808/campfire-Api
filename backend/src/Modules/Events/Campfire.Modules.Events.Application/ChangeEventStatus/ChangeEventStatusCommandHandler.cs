using Campfire.Modules.Events.Application.Abstractions;
using Campfire.Modules.Events.Application.Common;
using Campfire.Modules.Events.Application.Models;
using MediatR;

namespace Campfire.Modules.Events.Application.ChangeEventStatus;

public sealed class ChangeEventStatusCommandHandler(IEventService service)
    : IRequestHandler<ChangeEventStatusCommand, EventResult<EventDetail>>
{
    public Task<EventResult<EventDetail>> Handle(ChangeEventStatusCommand request, CancellationToken cancellationToken) =>
        service.ChangeStatusAsync(request.GroupId, request.EventId, request.UserId,
            request.ExpectedVersion, request.Status, request.TraceId, cancellationToken);
}
