using Campfire.Modules.Groups.Application.Abstractions;
using Campfire.Modules.Groups.Application.Common;
using Campfire.Modules.Groups.Application.Models;
using MediatR;

namespace Campfire.Modules.Groups.Application.PreviewInvitation;

public sealed class PreviewInvitationCommandHandler(IGroupInvitationService service)
    : IRequestHandler<PreviewInvitationCommand, GroupResult<InvitationPreview>>
{
    public Task<GroupResult<InvitationPreview>> Handle(
        PreviewInvitationCommand request,
        CancellationToken cancellationToken) =>
        service.PreviewAsync(request.Token, request.UserId, cancellationToken);
}
