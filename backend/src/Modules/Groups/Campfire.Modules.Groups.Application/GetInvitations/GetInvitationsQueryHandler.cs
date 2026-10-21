using Campfire.Modules.Groups.Application.Abstractions;
using Campfire.Modules.Groups.Application.Common;
using Campfire.Modules.Groups.Application.Models;
using MediatR;

namespace Campfire.Modules.Groups.Application.GetInvitations;

public sealed class GetInvitationsQueryHandler(IGroupInvitationService service)
    : IRequestHandler<GetInvitationsQuery, GroupResult<IReadOnlyList<InvitationView>>>
{
    public Task<GroupResult<IReadOnlyList<InvitationView>>> Handle(
        GetInvitationsQuery request,
        CancellationToken cancellationToken) =>
        service.GetForGroupAsync(request.GroupId, request.ActorUserId, cancellationToken);
}
