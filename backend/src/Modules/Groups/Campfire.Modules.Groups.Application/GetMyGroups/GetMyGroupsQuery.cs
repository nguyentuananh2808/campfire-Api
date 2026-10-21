using Campfire.Modules.Groups.Application.Models;
using MediatR;

namespace Campfire.Modules.Groups.Application.GetMyGroups;

public sealed record GetMyGroupsQuery(Guid UserId) : IRequest<IReadOnlyList<GroupSummary>>;
