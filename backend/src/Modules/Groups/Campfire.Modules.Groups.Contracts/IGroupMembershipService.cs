namespace Campfire.Modules.Groups.Contracts;

public interface IGroupMembershipService
{
    Task<IReadOnlyList<Guid>> GetGroupIdsAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<GroupAccess> GetAccessAsync(
        Guid groupId,
        Guid userId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<GroupMemberAccess>> GetMembersAsync(
        Guid groupId,
        CancellationToken cancellationToken);
}
