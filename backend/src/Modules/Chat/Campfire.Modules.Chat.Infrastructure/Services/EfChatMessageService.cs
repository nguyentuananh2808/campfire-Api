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
        Guid? replyToMessageId,
        CancellationToken cancellationToken)
    {
        if (!(await membershipService.GetAccessAsync(groupId, userId, cancellationToken)).IsMember)
            return ChatResult<SendMessageResult>.Failed(ChatFailures.HiddenGroup());

        var existing = await FindExistingAsync(groupId, userId, clientMessageId, cancellationToken);
        var messageKind = Enum.Parse<ChatMessageKind>(kind);
        if (existing is not null) return await ExistingResultAsync(existing, messageKind, content, replyToMessageId, userId, cancellationToken);

        if (replyToMessageId.HasValue && !await dbContext.Messages.AsNoTracking()
                .AnyAsync(message => message.Id == replyToMessageId && message.GroupId == groupId, cancellationToken))
            return ChatResult<SendMessageResult>.Failed(ChatFailures.MessageNotFound());

        var message = ChatMessage.Create(groupId, userId, clientMessageId, messageKind, content, replyToMessageId, timeProvider.GetUtcNow());
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
            return await ExistingResultAsync(existing, messageKind, content, replyToMessageId, userId, cancellationToken);
        }

        var dto = await ToDtoAsync(message, userId, cancellationToken);
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

        var items = await ToDtosAsync(messages, userId, cancellationToken);
        var olderCursor = hasMore && messages.Count > 0 ? ChatCursorCodec.Encode(messages[0].SentAtUtc, messages[0].Id) : null;
        var latestCursor = messages.Count > 0 ? ChatCursorCodec.Encode(messages[^1].SentAtUtc, messages[^1].Id) : cursorText;
        return ChatResult<ChatMessagePage>.Success(new ChatMessagePage(items, olderCursor, latestCursor, hasMore));
    }

    public async Task<ChatResult<ChatMessageDto>> SetReactionAsync(
        Guid groupId,
        Guid messageId,
        Guid userId,
        string? reaction,
        CancellationToken cancellationToken)
    {
        if (!(await membershipService.GetAccessAsync(groupId, userId, cancellationToken)).IsMember)
            return ChatResult<ChatMessageDto>.Failed(ChatFailures.HiddenGroup());

        var message = await dbContext.Messages.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == messageId && item.GroupId == groupId, cancellationToken);
        if (message is null) return ChatResult<ChatMessageDto>.Failed(ChatFailures.MessageNotFound());

        var current = await dbContext.Reactions
            .SingleOrDefaultAsync(item => item.MessageId == messageId && item.UserId == userId, cancellationToken);
        if (reaction is null)
        {
            if (current is not null) dbContext.Reactions.Remove(current);
        }
        else if (current is null)
        {
            dbContext.Reactions.Add(ChatMessageReaction.Create(messageId, userId, reaction, timeProvider.GetUtcNow()));
        }
        else if (current.Reaction != reaction)
        {
            current.Change(reaction, timeProvider.GetUtcNow());
        }

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException) when (reaction is not null)
        {
            dbContext.ChangeTracker.Clear();
            current = await dbContext.Reactions.SingleOrDefaultAsync(
                item => item.MessageId == messageId && item.UserId == userId, cancellationToken);
            if (current is null) throw;
            if (current.Reaction != reaction)
            {
                current.Change(reaction, timeProvider.GetUtcNow());
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }
        var dto = await ToDtoAsync(message, userId, cancellationToken);
        try
        {
            await notificationPublisher.PublishChangedAsync(groupId, messageId, CancellationToken.None);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Realtime notification failed after reaction changed for chat message {MessageId}", messageId);
        }

        return ChatResult<ChatMessageDto>.Success(dto);
    }

    private Task<ChatMessage?> FindExistingAsync(Guid groupId, Guid userId, Guid clientMessageId, CancellationToken cancellationToken) =>
        dbContext.Messages.AsNoTracking().SingleOrDefaultAsync(message => message.GroupId == groupId && message.SenderUserId == userId && message.ClientMessageId == clientMessageId, cancellationToken);

    private async Task<ChatResult<SendMessageResult>> ExistingResultAsync(
        ChatMessage message,
        ChatMessageKind kind,
        string content,
        Guid? replyToMessageId,
        Guid userId,
        CancellationToken cancellationToken) =>
        message.Kind == kind && message.Content == content && message.ReplyToMessageId == replyToMessageId
            ? ChatResult<SendMessageResult>.Success(new SendMessageResult(await ToDtoAsync(message, userId, cancellationToken), false))
            : ChatResult<SendMessageResult>.Failed(ChatFailures.IdempotencyConflict());

    private async Task<ChatMessageDto> ToDtoAsync(ChatMessage message, Guid userId, CancellationToken cancellationToken) =>
        (await ToDtosAsync([message], userId, cancellationToken))[0];

    private async Task<IReadOnlyList<ChatMessageDto>> ToDtosAsync(
        IReadOnlyList<ChatMessage> messages,
        Guid userId,
        CancellationToken cancellationToken)
    {
        if (messages.Count == 0) return [];

        var replyIds = messages.Where(message => message.ReplyToMessageId.HasValue)
            .Select(message => message.ReplyToMessageId!.Value).Distinct().ToArray();
        IReadOnlyList<ChatMessage> replies = replyIds.Length == 0
            ? []
            : await dbContext.Messages.AsNoTracking()
                .Where(message => replyIds.Contains(message.Id)).ToListAsync(cancellationToken);
        var messageIds = messages.Select(message => message.Id).ToArray();
        var reactions = await dbContext.Reactions.AsNoTracking()
            .Where(item => messageIds.Contains(item.MessageId)).ToListAsync(cancellationToken);
        var profileIds = messages.Select(message => message.SenderUserId)
            .Concat(replies.Select(reply => reply.SenderUserId)).Distinct().ToArray();
        var profiles = await profileReader.GetByIdsAsync(profileIds, cancellationToken);
        var replyLookup = replies.ToDictionary(reply => reply.Id);

        return messages.Select(message =>
        {
            ChatReplyPreviewDto? reply = null;
            if (message.ReplyToMessageId.HasValue && replyLookup.TryGetValue(message.ReplyToMessageId.Value, out var target))
            {
                reply = new ChatReplyPreviewDto(
                    target.Id,
                    DisplayName(target.SenderUserId, profiles),
                    target.Kind.ToString(),
                    Preview(target));
            }

            var messageReactions = reactions.Where(item => item.MessageId == message.Id).ToArray();
            var summaries = messageReactions.GroupBy(item => item.Reaction)
                .OrderBy(group => group.Key, StringComparer.Ordinal)
                .Select(group => new ChatReactionSummaryDto(group.Key, group.Count())).ToArray();
            return new ChatMessageDto(
                message.Id,
                message.GroupId,
                message.SenderUserId,
                DisplayName(message.SenderUserId, profiles),
                message.ClientMessageId,
                message.Kind.ToString(),
                message.Content,
                message.SentAtUtc,
                reply,
                summaries,
                messageReactions.SingleOrDefault(item => item.UserId == userId)?.Reaction);
        }).ToArray();
    }

    private static string DisplayName(Guid userId, IReadOnlyDictionary<Guid, UserProfile> profiles) =>
        profiles.TryGetValue(userId, out var profile) ? profile.DisplayName : "Thành viên";

    private static string Preview(ChatMessage message) => message.Kind is ChatMessageKind.Sticker
        ? message.Content
        : message.Content.Length <= 160 ? message.Content : $"{message.Content[..157]}...";
}
