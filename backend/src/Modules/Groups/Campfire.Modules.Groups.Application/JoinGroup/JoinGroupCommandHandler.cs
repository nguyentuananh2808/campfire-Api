using Campfire.Modules.Groups.Application.Abstractions;
using Campfire.Modules.Groups.Application.Common;
using Campfire.Modules.Groups.Application.Models;
using MediatR;

namespace Campfire.Modules.Groups.Application.JoinGroup;

public sealed class JoinGroupCommandHandler(IGroupInvitationService service)
    : IRequestHandler<JoinGroupCommand, GroupResult<GroupSummary>>
{
    public Task<GroupResult<GroupSummary>> Handle(
        JoinGroupCommand request,
        CancellationToken cancellationToken) =>
        service.JoinAsync(request.Token, request.UserId, request.TraceId, cancellationToken);
}
