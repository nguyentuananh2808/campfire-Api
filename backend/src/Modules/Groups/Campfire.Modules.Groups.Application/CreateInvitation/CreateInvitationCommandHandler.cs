using Campfire.Modules.Groups.Application.Abstractions;
using Campfire.Modules.Groups.Application.Common;
using Campfire.Modules.Groups.Application.Models;
using MediatR;

namespace Campfire.Modules.Groups.Application.CreateInvitation;

public sealed class CreateInvitationCommandHandler(IGroupInvitationService service)
    : IRequestHandler<CreateInvitationCommand, GroupResult<InvitationCreated>>
{
    public Task<GroupResult<InvitationCreated>> Handle(
        CreateInvitationCommand request,
        CancellationToken cancellationToken) =>
        service.CreateAsync(
            request.GroupId,
            request.ActorUserId,
            request.ExpiresInHours,
            request.TraceId,
            cancellationToken);
}
