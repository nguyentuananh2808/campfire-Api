using Campfire.Modules.Events.Application.Common;
using Campfire.Modules.Events.Application.Models;
using MediatR;

namespace Campfire.Modules.Events.Application.SetRsvp;

public sealed record SetRsvpCommand(Guid GroupId, Guid EventId, Guid UserId, string Response)
    : IRequest<EventResult<EventDetail>>;
