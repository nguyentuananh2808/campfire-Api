using Campfire.Modules.StrangerChat.Application.Common;
using Campfire.Modules.StrangerChat.Application.Models;
namespace Campfire.Modules.StrangerChat.Application.Abstractions;
public interface IStrangerChatService
{
    Task<StrangerResult<CreatedSession>> CreateSessionAsync(string nickname, string gender, string genderPreference, bool confirmsAdult, CancellationToken cancellationToken);
    Task<StrangerResult<GuestSessionModel>> GetSessionAsync(string rawToken, CancellationToken cancellationToken);
    Task<StrangerResult<bool>> DeleteSessionAsync(string rawToken, CancellationToken cancellationToken);
    Task<StrangerResult<MatchmakingModel>> QueueAsync(string rawToken, CancellationToken cancellationToken);
    Task<StrangerResult<bool>> CancelQueueAsync(string rawToken, CancellationToken cancellationToken);
    Task<StrangerResult<ConversationModel>> GetConversationAsync(string rawToken, CancellationToken cancellationToken);
    Task<StrangerResult<MessagePage>> GetMessagesAsync(string rawToken, Guid conversationId, int limit, CancellationToken cancellationToken);
    Task<StrangerResult<MessageModel>> SendMessageAsync(string rawToken, Guid conversationId, Guid clientMessageId, string kind, string content, Guid? replyToMessageId, CancellationToken cancellationToken);
    Task<StrangerResult<MessageModel>> SetReactionAsync(string rawToken, Guid conversationId, Guid messageId, string? reaction, CancellationToken cancellationToken);
    Task<StrangerResult<MessageModel>> EditMessageAsync(string rawToken, Guid conversationId, Guid messageId, string content, Guid expectedVersion, CancellationToken cancellationToken);
    Task<StrangerResult<bool>> EndAsync(string rawToken, Guid conversationId, bool block, CancellationToken cancellationToken);
    Task<StrangerResult<bool>> ReportAsync(string rawToken, Guid conversationId, string reason, string? note, CancellationToken cancellationToken);
    Task<Guid?> ResolveActiveConversationAsync(string rawToken, CancellationToken cancellationToken);
}
