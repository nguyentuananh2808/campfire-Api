using Campfire.Modules.Events.Application.Common;
using Campfire.Modules.Events.Application.Models;
using MediatR;

namespace Campfire.Modules.Events.Application.CreateEvent;

public sealed record CreateEventCommand(Guid GroupId, Guid UserId, string Title, string Description,
    DateTimeOffset StartsAtUtc, string Location) : IRequest<EventResult<EventDetail>>;
