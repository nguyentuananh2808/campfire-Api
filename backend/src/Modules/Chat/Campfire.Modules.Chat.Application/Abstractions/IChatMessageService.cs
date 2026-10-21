using Campfire.Modules.Chat.Application.Common;
using Campfire.Modules.Chat.Application.Models;

namespace Campfire.Modules.Chat.Application.Abstractions;

public interface IChatMessageService
{
    Task<ChatResult<SendMessageResult>> SendAsync(Guid groupId, Guid userId, Guid clientMessageId, string kind, string content, CancellationToken cancellationToken);
    Task<ChatResult<ChatMessagePage>> GetAsync(Guid groupId, Guid userId, string? before, string? after, int limit, CancellationToken cancellationToken);
}
