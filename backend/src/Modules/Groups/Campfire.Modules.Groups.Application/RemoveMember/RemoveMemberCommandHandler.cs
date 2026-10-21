using Campfire.Modules.Groups.Application.Abstractions;
using Campfire.Modules.Groups.Application.Common;
using MediatR;

namespace Campfire.Modules.Groups.Application.RemoveMember;

public sealed class RemoveMemberCommandHandler(IGroupManagementService service)
    : IRequestHandler<RemoveMemberCommand, GroupResult<bool>>
{
    public Task<GroupResult<bool>> Handle(
        RemoveMemberCommand request,
        CancellationToken cancellationToken) =>
        service.RemoveMemberAsync(
            request.GroupId,
            request.MemberUserId,
            request.ActorUserId,
            request.TraceId,
            cancellationToken);
}
