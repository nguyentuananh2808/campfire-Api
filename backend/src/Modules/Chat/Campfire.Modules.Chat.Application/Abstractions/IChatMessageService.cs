using Campfire.Modules.Chat.Application.Common;
using Campfire.Modules.Chat.Application.Models;
using Campfire.Modules.Chat.Contracts;

namespace Campfire.Modules.Chat.Application.Abstractions;

public interface IChatMessageService
{
    Task<ChatResult<SendMessageResult>> SendAsync(Guid groupId, Guid userId, Guid clientMessageId, string kind, string content, Guid? replyToMessageId, CancellationToken cancellationToken);
    Task<ChatResult<ChatMessagePage>> GetAsync(Guid groupId, Guid userId, string? before, string? after, int limit, CancellationToken cancellationToken);
    Task<ChatResult<ChatMessageDto>> SetReactionAsync(Guid groupId, Guid messageId, Guid userId, string? reaction, CancellationToken cancellationToken);
}
