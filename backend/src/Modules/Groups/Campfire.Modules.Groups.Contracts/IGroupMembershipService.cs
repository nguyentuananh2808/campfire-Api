namespace Campfire.Modules.Groups.Contracts;

public interface IGroupMembershipService
{
    Task<GroupAccess> GetAccessAsync(
        Guid groupId,
        Guid userId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<GroupMemberAccess>> GetMembersAsync(
        Guid groupId,
        CancellationToken cancellationToken);
}
