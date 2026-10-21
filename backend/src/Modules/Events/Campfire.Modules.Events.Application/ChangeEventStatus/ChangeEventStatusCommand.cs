using Campfire.Modules.Events.Application.Common;
using Campfire.Modules.Events.Application.Models;
using MediatR;

namespace Campfire.Modules.Events.Application.ChangeEventStatus;

public sealed record ChangeEventStatusCommand(Guid GroupId, Guid EventId, Guid UserId,
    Guid ExpectedVersion, string Status, string TraceId) : IRequest<EventResult<EventDetail>>;
