using Campfire.Modules.StrangerChat.Application.Models;
namespace Campfire.Modules.StrangerChat.Application.Abstractions;
public interface IStrangerNotificationPublisher
{
    Task MatchFoundAsync(Guid conversationId, CancellationToken cancellationToken);
    Task MessageCreatedAsync(Guid conversationId, MessageModel message, CancellationToken cancellationToken);
    Task ConversationEndedAsync(Guid conversationId, CancellationToken cancellationToken);
}
