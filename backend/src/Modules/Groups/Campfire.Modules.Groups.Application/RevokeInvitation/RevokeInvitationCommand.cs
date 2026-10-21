using Campfire.Modules.Groups.Application.Common;
using MediatR;

namespace Campfire.Modules.Groups.Application.RevokeInvitation;

public sealed record RevokeInvitationCommand(
    Guid GroupId,
    Guid InvitationId,
    Guid ActorUserId,
    string TraceId) : IRequest<GroupResult<bool>>;
