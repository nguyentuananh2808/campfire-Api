using Campfire.Modules.Groups.Application.Common;
using Campfire.Modules.Groups.Application.Models;

namespace Campfire.Modules.Groups.Application.Abstractions;

public interface IGroupInvitationService
{
    Task<GroupResult<InvitationCreated>> CreateAsync(
        Guid groupId,
        Guid actorUserId,
        int expiresInHours,
        string traceId,
        CancellationToken cancellationToken);

    Task<GroupResult<IReadOnlyList<InvitationView>>> GetForGroupAsync(
        Guid groupId,
        Guid actorUserId,
        CancellationToken cancellationToken);

    Task<GroupResult<bool>> RevokeAsync(
        Guid groupId,
        Guid invitationId,
        Guid actorUserId,
        string traceId,
        CancellationToken cancellationToken);

    Task<GroupResult<InvitationPreview>> PreviewAsync(
        string token,
        Guid userId,
        CancellationToken cancellationToken);

    Task<GroupResult<GroupSummary>> JoinAsync(
        string token,
        Guid userId,
        string traceId,
        CancellationToken cancellationToken);
}
