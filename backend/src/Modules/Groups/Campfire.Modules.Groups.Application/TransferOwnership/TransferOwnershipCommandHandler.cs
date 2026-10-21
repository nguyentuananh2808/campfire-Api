using Campfire.Modules.Groups.Application.Abstractions;
using Campfire.Modules.Groups.Application.Common;
using Campfire.Modules.Groups.Application.Models;
using MediatR;

namespace Campfire.Modules.Groups.Application.TransferOwnership;

public sealed class TransferOwnershipCommandHandler(IGroupManagementService service)
    : IRequestHandler<TransferOwnershipCommand, GroupResult<GroupSummary>>
{
    public Task<GroupResult<GroupSummary>> Handle(
        TransferOwnershipCommand request,
        CancellationToken cancellationToken) =>
        service.TransferOwnershipAsync(
            request.GroupId,
            request.NewOwnerUserId,
            request.ExpectedVersion,
            request.ActorUserId,
            request.TraceId,
            cancellationToken);
}
