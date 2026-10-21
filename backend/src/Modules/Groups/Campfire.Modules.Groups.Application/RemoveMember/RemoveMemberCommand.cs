using Campfire.Modules.Groups.Application.Common;
using MediatR;

namespace Campfire.Modules.Groups.Application.RemoveMember;

public sealed record RemoveMemberCommand(
    Guid GroupId,
    Guid MemberUserId,
    Guid ActorUserId,
    string TraceId) : IRequest<GroupResult<bool>>;
