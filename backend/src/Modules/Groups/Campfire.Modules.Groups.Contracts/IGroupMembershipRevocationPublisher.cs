namespace Campfire.Modules.Groups.Contracts;

public interface IGroupMembershipRevocationPublisher
{
    Task PublishAsync(Guid groupId, Guid userId, CancellationToken cancellationToken);
}
