using Campfire.Modules.Groups.Application.Common;
using Campfire.Modules.Groups.Application.Models;
using MediatR;

namespace Campfire.Modules.Groups.Application.CreateInvitation;

public sealed record CreateInvitationCommand(
    Guid GroupId,
    Guid ActorUserId,
    int ExpiresInHours,
    string TraceId) : IRequest<GroupResult<InvitationCreated>>;
