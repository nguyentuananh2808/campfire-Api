using Campfire.Modules.Groups.Contracts;

namespace Campfire.Modules.Groups.Infrastructure.Services;

public sealed class GroupMembershipRevocationPublisher(
    IEnumerable<IGroupMembershipRevocationListener> listeners) : IGroupMembershipRevocationPublisher
{
    public async Task PublishAsync(Guid groupId, Guid userId, CancellationToken cancellationToken)
    {
        foreach (var listener in listeners)
        {
            await listener.RevokeAsync(groupId, userId, cancellationToken);
        }
    }
}
