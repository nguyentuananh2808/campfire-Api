using Campfire.Modules.Events.Application.Common;
using Campfire.Modules.Events.Application.Models;
using MediatR;

namespace Campfire.Modules.Events.Application.UpdateEvent;

public sealed record UpdateEventCommand(Guid GroupId, Guid EventId, Guid UserId, Guid ExpectedVersion,
    string Title, string Description, DateTimeOffset StartsAtUtc, string Location) : IRequest<EventResult<EventDetail>>;
