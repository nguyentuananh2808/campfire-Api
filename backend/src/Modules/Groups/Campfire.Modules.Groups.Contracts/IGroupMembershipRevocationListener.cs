namespace Campfire.Modules.Groups.Contracts;

public interface IGroupMembershipRevocationListener
{
    Task RevokeAsync(Guid groupId, Guid userId, CancellationToken cancellationToken);
}
