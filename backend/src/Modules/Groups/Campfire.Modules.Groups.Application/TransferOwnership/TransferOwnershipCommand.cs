using Campfire.Modules.Groups.Application.Common;
using Campfire.Modules.Groups.Application.Models;
using MediatR;

namespace Campfire.Modules.Groups.Application.TransferOwnership;

public sealed record TransferOwnershipCommand(
    Guid GroupId,
    Guid NewOwnerUserId,
    Guid ExpectedVersion,
    Guid ActorUserId,
    string TraceId) : IRequest<GroupResult<GroupSummary>>;
