using Campfire.Modules.Events.Application.Common;
using Campfire.Modules.Events.Application.Models;
using MediatR;

namespace Campfire.Modules.Events.Application.GetEvent;

public sealed record GetEventQuery(Guid GroupId, Guid EventId, Guid UserId) : IRequest<EventResult<EventDetail>>;
