using Campfire.Modules.StrangerChat.Application.Abstractions; using Campfire.Modules.StrangerChat.Application.Models; using Microsoft.AspNetCore.SignalR;
namespace Campfire.Modules.StrangerChat.Infrastructure.Realtime;
public sealed class SignalRStrangerNotificationPublisher(IHubContext<StrangerChatHub> hub) : IStrangerNotificationPublisher
{
    public Task MatchFoundAsync(Guid id, CancellationToken ct) => hub.Clients.Group(StrangerChatHub.Group(id)).SendAsync("MatchFound", id, ct);
    public Task MessageCreatedAsync(Guid id, MessageModel message, CancellationToken ct) => hub.Clients.Group(StrangerChatHub.Group(id)).SendAsync("MessageCreated", message, ct);
    public Task MessageChangedAsync(Guid id, Guid messageId, CancellationToken ct) => hub.Clients.Group(StrangerChatHub.Group(id)).SendAsync("MessageChanged", messageId, ct);
    public Task ConversationEndedAsync(Guid id, CancellationToken ct) => hub.Clients.Group(StrangerChatHub.Group(id)).SendAsync("ConversationEnded", id, ct);
}
