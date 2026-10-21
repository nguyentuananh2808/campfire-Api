using Campfire.Modules.Groups.Application.Abstractions;
using Campfire.Modules.Groups.Application.Common;
using MediatR;

namespace Campfire.Modules.Groups.Application.RevokeInvitation;

public sealed class RevokeInvitationCommandHandler(IGroupInvitationService service)
    : IRequestHandler<RevokeInvitationCommand, GroupResult<bool>>
{
    public Task<GroupResult<bool>> Handle(
        RevokeInvitationCommand request,
        CancellationToken cancellationToken) =>
        service.RevokeAsync(
            request.GroupId,
            request.InvitationId,
            request.ActorUserId,
            request.TraceId,
            cancellationToken);
}
