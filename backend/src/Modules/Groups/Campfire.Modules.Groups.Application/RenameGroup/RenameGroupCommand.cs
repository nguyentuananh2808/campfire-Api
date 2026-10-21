using Campfire.Modules.Groups.Application.Common;
using Campfire.Modules.Groups.Application.Models;
using MediatR;

namespace Campfire.Modules.Groups.Application.RenameGroup;

public sealed record RenameGroupCommand(
    Guid GroupId,
    Guid UserId,
    string Name,
    string TraceId) : IRequest<GroupResult<GroupSummary>>;
