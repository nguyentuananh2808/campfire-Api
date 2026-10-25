using Campfire.Modules.Chat.Application.Abstractions;
using Campfire.Modules.Chat.Contracts;
using Microsoft.AspNetCore.SignalR;

namespace Campfire.Modules.Chat.Infrastructure.Realtime;

public sealed class SignalRChatNotificationPublisher(IHubContext<ChatHub> hubContext) : IChatNotificationPublisher
{
    public Task PublishCreatedAsync(ChatMessageDto message, CancellationToken cancellationToken) =>
        hubContext.Clients.Group(ChatHub.GroupName(message.GroupId)).SendAsync("MessageCreated", message, cancellationToken);

    public Task PublishChangedAsync(Guid groupId, Guid messageId, CancellationToken cancellationToken) =>
        hubContext.Clients.Group(ChatHub.GroupName(groupId)).SendAsync("MessageChanged", messageId, cancellationToken);
}
