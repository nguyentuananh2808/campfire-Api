using Campfire.Modules.Groups.Application.Common;
using Campfire.Modules.Groups.Application.Models;
using MediatR;

namespace Campfire.Modules.Groups.Application.JoinGroup;

public sealed record JoinGroupCommand(string Token, Guid UserId, string TraceId)
    : IRequest<GroupResult<GroupSummary>>;
