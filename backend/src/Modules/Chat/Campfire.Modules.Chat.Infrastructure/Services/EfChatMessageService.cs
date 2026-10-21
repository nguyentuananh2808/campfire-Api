using Campfire.Modules.Chat.Application.Abstractions;
using Campfire.Modules.Chat.Application.Common;
using Campfire.Modules.Chat.Application.Models;
using Campfire.Modules.Chat.Contracts;
using Campfire.Modules.Chat.Domain.Messages;
using Campfire.Modules.Chat.Infrastructure.Persistence;
using Campfire.Modules.Groups.Contracts;
using Campfire.Modules.Identity.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Campfire.Modules.Chat.Infrastructure.Services;

public sealed class EfChatMessageService(
    ChatDbContext dbContext,
    IGroupMembershipService membershipService,
    IUserProfileReader profileReader,
    IChatNotificationPublisher notificationPublisher,
    TimeProvider timeProvider,
    ILogger<EfChatMessageService> logger) : IChatMessageService
{
    public async Task<ChatResult<SendMessageResult>> SendAsync(
        Guid groupId,
        Guid userId,
        Guid clientMessageId,
        string kind,
        string content,
        CancellationToken cancellationToken)
    {
        if (!(await membershipService.GetAccessAsync(groupId, userId, cancellationToken)).IsMember)
            return ChatResult<SendMessageResult>.Failed(ChatFailures.HiddenGroup());

        var existing = await FindExistingAsync(groupId, userId, clientMessageId, cancellationToken);
        var messageKind = Enum.Parse<ChatMessageKind>(kind);
        if (existing is not null) return await ExistingResultAsync(existing, messageKind, content, cancellationToken);

        var message = ChatMessage.Create(groupId, userId, clientMessageId, messageKind, content, timeProvider.GetUtcNow());
        dbContext.Messages.Add(message);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            dbContext.ChangeTracker.Clear();
            existing = await FindExistingAsync(groupId, userId, clientMessageId, cancellationToken);
            if (existing is null) throw;
            return await ExistingResultAsync(existing, messageKind, content, cancellationToken);
        }

        var dto = await ToDtoAsync(message, cancellationToken);
        try
        {
            await notificationPublisher.PublishCreatedAsync(dto, CancellationToken.None);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Realtime notification failed after chat message {MessageId} was persisted for group {GroupId}", message.Id, groupId);
        }
        return ChatResult<SendMessageResult>.Success(new SendMessageResult(dto, true));
    }

    public async Task<ChatResult<ChatMessagePage>> GetAsync(Guid groupId, Guid userId, string? before, string? after, int limit, CancellationToken cancellationToken)
    {
        if (!(await membershipService.GetAccessAsync(groupId, userId, cancellationToken)).IsMember)
            return ChatResult<ChatMessagePage>.Failed(ChatFailures.HiddenGroup());

        ChatCursor cursor = default;
        var cursorText = before ?? after;
        if (cursorText is not null && !ChatCursorCodec.TryDecode(cursorText, out cursor))
            return ChatResult<ChatMessagePage>.Failed(ChatFailures.InvalidCursor());

        var query = dbContext.Messages.AsNoTracking().Where(message => message.GroupId == groupId);
        if (before is not null)
            query = query.Where(message => message.SentAtUtc < cursor.SentAtUtc ||
                (message.SentAtUtc == cursor.SentAtUtc && message.Id.CompareTo(cursor.MessageId) < 0));
        if (after is not null)
            query = query.Where(message => message.SentAtUtc > cursor.SentAtUtc ||
                (message.SentAtUtc == cursor.SentAtUtc && message.Id.CompareTo(cursor.MessageId) > 0));

        var ascending = after is not null;
        query = ascending
            ? query.OrderBy(message => message.SentAtUtc).ThenBy(message => message.Id)
            : query.OrderByDescending(message => message.SentAtUtc).ThenByDescending(message => message.Id);
        var messages = await query.Take(limit + 1).ToListAsync(cancellationToken);
        var hasMore = messages.Count > limit;
        if (hasMore) messages.RemoveAt(messages.Count - 1);
        if (!ascending) messages.Reverse();

        var profiles = await profileReader.GetByIdsAsync(messages.Select(message => message.SenderUserId).Distinct().ToArray(), cancellationToken);
        var items = messages.Select(message => ToDto(message, profiles)).ToArray();
        var olderCursor = hasMore && messages.Count > 0 ? ChatCursorCodec.Encode(messages[0].SentAtUtc, messages[0].Id) : null;
        var latestCursor = messages.Count > 0 ? ChatCursorCodec.Encode(messages[^1].SentAtUtc, messages[^1].Id) : cursorText;
        return ChatResult<ChatMessagePage>.Success(new ChatMessagePage(items, olderCursor, latestCursor, hasMore));
    }

    private Task<ChatMessage?> FindExistingAsync(Guid groupId, Guid userId, Guid clientMessageId, CancellationToken cancellationToken) =>
        dbContext.Messages.AsNoTracking().SingleOrDefaultAsync(message => message.GroupId == groupId && message.SenderUserId == userId && message.ClientMessageId == clientMessageId, cancellationToken);

    private async Task<ChatResult<SendMessageResult>> ExistingResultAsync(
        ChatMessage message,
        ChatMessageKind kind,
        string content,
        CancellationToken cancellationToken) =>
        message.Kind == kind && message.Content == content
            ? ChatResult<SendMessageResult>.Success(new SendMessageResult(await ToDtoAsync(message, cancellationToken), false))
            : ChatResult<SendMessageResult>.Failed(ChatFailures.IdempotencyConflict());

    private async Task<ChatMessageDto> ToDtoAsync(ChatMessage message, CancellationToken cancellationToken)
    {
        var profiles = await profileReader.GetByIdsAsync([message.SenderUserId], cancellationToken);
        return ToDto(message, profiles);
    }

    private static ChatMessageDto ToDto(ChatMessage message, IReadOnlyDictionary<Guid, UserProfile> profiles) =>
        new(message.Id, message.GroupId, message.SenderUserId,
            profiles.TryGetValue(message.SenderUserId, out var profile) ? profile.DisplayName : "Thành viên",
            message.ClientMessageId, message.Kind.ToString(), message.Content, message.SentAtUtc);
}
