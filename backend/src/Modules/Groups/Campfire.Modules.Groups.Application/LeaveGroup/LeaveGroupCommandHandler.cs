using Campfire.Modules.Groups.Application.Abstractions;
using Campfire.Modules.Groups.Application.Common;
using MediatR;

namespace Campfire.Modules.Groups.Application.LeaveGroup;

public sealed class LeaveGroupCommandHandler(IGroupManagementService service)
    : IRequestHandler<LeaveGroupCommand, GroupResult<bool>>
{
    public Task<GroupResult<bool>> Handle(
        LeaveGroupCommand request,
        CancellationToken cancellationToken) =>
        service.LeaveAsync(request.GroupId, request.UserId, request.TraceId, cancellationToken);
}
