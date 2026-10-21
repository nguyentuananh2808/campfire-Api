using Campfire.Modules.Groups.Application.Common;
using Campfire.Modules.Groups.Application.Models;
using MediatR;

namespace Campfire.Modules.Groups.Application.GetInvitations;

public sealed record GetInvitationsQuery(Guid GroupId, Guid ActorUserId)
    : IRequest<GroupResult<IReadOnlyList<InvitationView>>>;
