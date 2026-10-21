using Campfire.Modules.Groups.Application.Abstractions;
using Campfire.Modules.Groups.Application.Models;
using MediatR;

namespace Campfire.Modules.Groups.Application.GetMyGroups;

public sealed class GetMyGroupsQueryHandler(IGroupManagementService service)
    : IRequestHandler<GetMyGroupsQuery, IReadOnlyList<GroupSummary>>
{
    public Task<IReadOnlyList<GroupSummary>> Handle(
        GetMyGroupsQuery request,
        CancellationToken cancellationToken) =>
        service.GetMineAsync(request.UserId, cancellationToken);
}
