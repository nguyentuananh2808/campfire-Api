using Campfire.Modules.Groups.Application.Common;
using Campfire.Modules.Groups.Application.Models;

namespace Campfire.Modules.Groups.Application.Abstractions;

public interface IGroupManagementService
{
    Task<GroupResult<GroupSummary>> CreateAsync(
        Guid userId,
        string name,
        string traceId,
        CancellationToken cancellationToken);

    Task<GroupResult<GroupSummary>> RenameAsync(
        Guid groupId,
        Guid userId,
        string name,
        string traceId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<GroupSummary>> GetMineAsync(Guid userId, CancellationToken cancellationToken);

    Task<GroupResult<IReadOnlyList<GroupMemberView>>> GetMembersAsync(
        Guid groupId,
        Guid userId,
        CancellationToken cancellationToken);

    Task<GroupResult<bool>> RemoveMemberAsync(
        Guid groupId,
        Guid memberUserId,
        Guid actorUserId,
        string traceId,
        CancellationToken cancellationToken);

    Task<GroupResult<bool>> LeaveAsync(
        Guid groupId,
        Guid userId,
        string traceId,
        CancellationToken cancellationToken);

    Task<GroupResult<GroupSummary>> TransferOwnershipAsync(
        Guid groupId,
        Guid newOwnerUserId,
        Guid expectedVersion,
        Guid actorUserId,
        string traceId,
        CancellationToken cancellationToken);
}
