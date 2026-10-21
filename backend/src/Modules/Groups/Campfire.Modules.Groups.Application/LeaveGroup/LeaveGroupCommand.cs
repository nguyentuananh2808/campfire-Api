using Campfire.Modules.Groups.Application.Common;
using MediatR;

namespace Campfire.Modules.Groups.Application.LeaveGroup;

public sealed record LeaveGroupCommand(Guid GroupId, Guid UserId, string TraceId)
    : IRequest<GroupResult<bool>>;
