using Campfire.Modules.Groups.Application.Common;
using Campfire.Modules.Groups.Application.Models;
using MediatR;

namespace Campfire.Modules.Groups.Application.GetGroupMembers;

public sealed record GetGroupMembersQuery(Guid GroupId, Guid UserId)
    : IRequest<GroupResult<IReadOnlyList<GroupMemberView>>>;
