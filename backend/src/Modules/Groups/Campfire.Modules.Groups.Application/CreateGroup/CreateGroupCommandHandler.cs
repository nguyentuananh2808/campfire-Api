using Campfire.Modules.Groups.Application.Abstractions;
using Campfire.Modules.Groups.Application.Common;
using Campfire.Modules.Groups.Application.Models;
using MediatR;

namespace Campfire.Modules.Groups.Application.CreateGroup;

public sealed class CreateGroupCommandHandler(IGroupManagementService service)
    : IRequestHandler<CreateGroupCommand, GroupResult<GroupSummary>>
{
    public Task<GroupResult<GroupSummary>> Handle(
        CreateGroupCommand request,
        CancellationToken cancellationToken) =>
        service.CreateAsync(request.UserId, request.Name.Trim(), request.TraceId, cancellationToken);
}
