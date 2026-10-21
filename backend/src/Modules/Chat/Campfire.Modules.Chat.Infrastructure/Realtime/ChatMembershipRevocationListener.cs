using Campfire.Modules.Groups.Contracts;
using Microsoft.AspNetCore.SignalR;

namespace Campfire.Modules.Chat.Infrastructure.Realtime;

public sealed class ChatMembershipRevocationListener(ChatConnectionRegistry registry, IHubContext<ChatHub> hubContext)
    : IGroupMembershipRevocationListener
{
    public async Task RevokeAsync(Guid groupId, Guid userId, CancellationToken cancellationToken)
    {
        foreach (var connectionId in registry.Find(userId, groupId))
        {
            await hubContext.Groups.RemoveFromGroupAsync(connectionId, ChatHub.GroupName(groupId), cancellationToken);
            registry.Remove(connectionId, userId, groupId);
        }
    }
}
