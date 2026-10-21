using Campfire.Modules.Groups.Application.Abstractions;
using Campfire.Modules.Groups.Application.Common;
using Campfire.Modules.Groups.Application.Models;
using MediatR;

namespace Campfire.Modules.Groups.Application.RenameGroup;

public sealed class RenameGroupCommandHandler(IGroupManagementService service)
    : IRequestHandler<RenameGroupCommand, GroupResult<GroupSummary>>
{
    public Task<GroupResult<GroupSummary>> Handle(
        RenameGroupCommand request,
        CancellationToken cancellationToken) =>
        service.RenameAsync(
            request.GroupId,
            request.UserId,
            request.Name.Trim(),
            request.TraceId,
            cancellationToken);
}
