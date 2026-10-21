using Campfire.Modules.Groups.Application.Common;
using Campfire.Modules.Groups.Application.Models;
using MediatR;

namespace Campfire.Modules.Groups.Application.CreateGroup;

public sealed record CreateGroupCommand(Guid UserId, string Name, string TraceId)
    : IRequest<GroupResult<GroupSummary>>;
