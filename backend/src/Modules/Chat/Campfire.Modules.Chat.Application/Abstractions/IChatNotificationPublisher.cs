using Campfire.Modules.Chat.Contracts;

namespace Campfire.Modules.Chat.Application.Abstractions;

public interface IChatNotificationPublisher
{
    Task PublishCreatedAsync(ChatMessageDto message, CancellationToken cancellationToken);
    Task PublishChangedAsync(Guid groupId, Guid messageId, CancellationToken cancellationToken);
}
