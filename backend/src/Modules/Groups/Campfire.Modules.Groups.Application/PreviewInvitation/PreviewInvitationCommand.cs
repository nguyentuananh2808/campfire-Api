using Campfire.Modules.Groups.Application.Common;
using Campfire.Modules.Groups.Application.Models;
using MediatR;

namespace Campfire.Modules.Groups.Application.PreviewInvitation;

public sealed record PreviewInvitationCommand(string Token, Guid UserId)
    : IRequest<GroupResult<InvitationPreview>>;
