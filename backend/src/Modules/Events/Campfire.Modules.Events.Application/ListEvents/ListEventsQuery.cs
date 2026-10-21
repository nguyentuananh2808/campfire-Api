using Campfire.Modules.Events.Application.Common;
using Campfire.Modules.Events.Application.Models;
using MediatR;

namespace Campfire.Modules.Events.Application.ListEvents;

public sealed record ListEventsQuery(Guid GroupId, Guid UserId, int Limit) : IRequest<EventResult<IReadOnlyList<EventSummary>>>;
