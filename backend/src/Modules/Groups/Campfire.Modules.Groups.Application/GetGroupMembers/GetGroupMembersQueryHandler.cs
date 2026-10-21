using Campfire.Modules.Groups.Application.Abstractions;
using Campfire.Modules.Groups.Application.Common;
using Campfire.Modules.Groups.Application.Models;
using MediatR;

namespace Campfire.Modules.Groups.Application.GetGroupMembers;

public sealed class GetGroupMembersQueryHandler(IGroupManagementService service)
    : IRequestHandler<GetGroupMembersQuery, GroupResult<IReadOnlyList<GroupMemberView>>>
{
    public Task<GroupResult<IReadOnlyList<GroupMemberView>>> Handle(
        GetGroupMembersQuery request,
        CancellationToken cancellationToken) =>
        service.GetMembersAsync(request.GroupId, request.UserId, cancellationToken);
}
