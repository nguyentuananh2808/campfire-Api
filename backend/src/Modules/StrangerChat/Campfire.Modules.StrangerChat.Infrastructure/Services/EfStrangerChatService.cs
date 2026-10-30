using System.Security.Cryptography;
using System.Text;
using Campfire.Modules.StrangerChat.Application.Abstractions;
using Campfire.Modules.StrangerChat.Application.Common;
using Campfire.Modules.StrangerChat.Application.Models;
using Campfire.Modules.StrangerChat.Domain.Entities;
using Campfire.Modules.StrangerChat.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
namespace Campfire.Modules.StrangerChat.Infrastructure.Services;

public sealed class EfStrangerChatService(StrangerChatDbContext db, TimeProvider clock, IStrangerNotificationPublisher notifications) : IStrangerChatService
{
    private static StrangerResult<T> Unauthorized<T>() => StrangerResult<T>.Failed("guest_session_invalid", "Phiên khách không hợp lệ hoặc đã hết hạn.", 401);
    private static StrangerResult<T> NotFound<T>() => StrangerResult<T>.Failed("conversation_not_found", "Không tìm thấy cuộc trò chuyện.", 404);
    private static string Hash(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    private async Task<GuestSession?> Session(string token, CancellationToken ct) { if (string.IsNullOrWhiteSpace(token)) return null; var now = clock.GetUtcNow(); return await db.Sessions.SingleOrDefaultAsync(x => x.TokenHash == Hash(token) && x.LastSeenAtUtc > now.AddHours(-24), ct); }
    public async Task<StrangerResult<CreatedSession>> CreateSessionAsync(string nickname, string gender, string genderPreference, bool confirmsAdult, CancellationToken ct) { if (!confirmsAdult) return StrangerResult<CreatedSession>.Failed("adult_confirmation_required", "Bạn phải xác nhận đủ 18 tuổi.", 400); if (!Enum.TryParse<GuestGender>(gender, out var parsedGender) || !Enum.TryParse<GenderPreference>(genderPreference, out var parsedPreference)) return StrangerResult<CreatedSession>.Failed("gender_selection_invalid", "Lựa chọn giới tính không hợp lệ.", 400); var bytes = RandomNumberGenerator.GetBytes(32); var raw = Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('='); var now = clock.GetUtcNow(); var entity = GuestSession.Create(Hash(raw), nickname, parsedGender, parsedPreference, now); db.Sessions.Add(entity); await db.SaveChangesAsync(ct); return StrangerResult<CreatedSession>.Success(new(raw, SessionModel(entity))); }
    public async Task<StrangerResult<GuestSessionModel>> GetSessionAsync(string token, CancellationToken ct) { var s = await Session(token, ct); return s is null ? Unauthorized<GuestSessionModel>() : StrangerResult<GuestSessionModel>.Success(SessionModel(s)); }
    public async Task<StrangerResult<bool>> DeleteSessionAsync(string token, CancellationToken ct) { var s = await Session(token, ct); if (s is null) return Unauthorized<bool>(); var active = await db.Conversations.Where(x => x.EndedAtUtc == null && (x.FirstSessionId == s.Id || x.SecondSessionId == s.Id)).SingleOrDefaultAsync(ct); if (active is not null) active.End(clock.GetUtcNow()); await db.Tickets.Where(x => x.SessionId == s.Id).ExecuteDeleteAsync(ct); db.Sessions.Remove(s); await db.SaveChangesAsync(ct); if (active is not null) await notifications.ConversationEndedAsync(active.Id, ct); return StrangerResult<bool>.Success(true); }
    public async Task<StrangerResult<MatchmakingModel>> QueueAsync(string token, CancellationToken ct) { var s = await Session(token, ct); if (s is null) return Unauthorized<MatchmakingModel>(); var active = await Active(s.Id, ct); if (active is not null) return StrangerResult<MatchmakingModel>.Success(new("matched", await Model(active, s.Id, ct))); await using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct); var now = clock.GetUtcNow(); await db.Tickets.Where(x => x.ExpiresAtUtc <= now).ExecuteDeleteAsync(ct); var blocked = db.Blocks.Where(x => x.ExpiresAtUtc > now && (x.BlockerSessionId == s.Id || x.BlockedSessionId == s.Id)).Select(x => x.BlockerSessionId == s.Id ? x.BlockedSessionId : x.BlockerSessionId); var compatibleSessionIds = db.Sessions.Where(candidate => (s.GenderPreference == GenderPreference.Any || (s.GenderPreference == GenderPreference.Male && candidate.Gender == GuestGender.Male) || (s.GenderPreference == GenderPreference.Female && candidate.Gender == GuestGender.Female) || (s.GenderPreference == GenderPreference.Other && candidate.Gender == GuestGender.Other)) && (candidate.GenderPreference == GenderPreference.Any || (candidate.GenderPreference == GenderPreference.Male && s.Gender == GuestGender.Male) || (candidate.GenderPreference == GenderPreference.Female && s.Gender == GuestGender.Female) || (candidate.GenderPreference == GenderPreference.Other && s.Gender == GuestGender.Other))).Select(candidate => candidate.Id); var ticket = await db.Tickets.Where(x => x.SessionId != s.Id && !blocked.Contains(x.SessionId) && compatibleSessionIds.Contains(x.SessionId)).OrderBy(x => x.QueuedAtUtc).FirstOrDefaultAsync(ct); if (ticket is null) { if (!await db.Tickets.AnyAsync(x => x.SessionId == s.Id, ct)) db.Tickets.Add(MatchmakingTicket.Create(s.Id, now)); await db.SaveChangesAsync(ct); await tx.CommitAsync(ct); return StrangerResult<MatchmakingModel>.Success(new("waiting", null)); } var conversation = StrangerConversation.Create(ticket.SessionId, s.Id, now); db.Conversations.Add(conversation); db.Tickets.Remove(ticket); await db.Tickets.Where(x => x.SessionId == s.Id).ExecuteDeleteAsync(ct); await db.SaveChangesAsync(ct); await tx.CommitAsync(ct); await notifications.MatchFoundAsync(conversation.Id, ct); return StrangerResult<MatchmakingModel>.Success(new("matched", await Model(conversation, s.Id, ct))); }
    public async Task<StrangerResult<bool>> CancelQueueAsync(string token, CancellationToken ct) { var s = await Session(token, ct); if (s is null) return Unauthorized<bool>(); await db.Tickets.Where(x => x.SessionId == s.Id).ExecuteDeleteAsync(ct); return StrangerResult<bool>.Success(true); }
    public async Task<StrangerResult<ConversationModel>> GetConversationAsync(string token, CancellationToken ct) { var s = await Session(token, ct); if (s is null) return Unauthorized<ConversationModel>(); var c = await Active(s.Id, ct); return c is null ? NotFound<ConversationModel>() : StrangerResult<ConversationModel>.Success(await Model(c, s.Id, ct)); }
    public async Task<StrangerResult<MessagePage>> GetMessagesAsync(string token, Guid id, int limit, CancellationToken ct)
    {
        var session = await Session(token, ct);
        if (session is null) return Unauthorized<MessagePage>();
        var conversation = await db.Conversations.SingleOrDefaultAsync(
            item => item.Id == id && (item.FirstSessionId == session.Id || item.SecondSessionId == session.Id), ct);
        if (conversation is null) return NotFound<MessagePage>();
        var messages = await db.Messages.AsNoTracking().Where(item => item.ConversationId == id)
            .OrderByDescending(item => item.SentAtUtc).ThenByDescending(item => item.Id)
            .Take(Math.Clamp(limit, 1, 100)).ToListAsync(ct);
        messages.Reverse();
        return StrangerResult<MessagePage>.Success(new(await ToModelsAsync(messages, session.Id, ct)));
    }

    public async Task<StrangerResult<MessageModel>> SendMessageAsync(
        string token, Guid id, Guid clientId, string kind, string content, Guid? replyToMessageId, CancellationToken ct)
    {
        var session = await Session(token, ct);
        if (session is null) return Unauthorized<MessageModel>();
        var conversation = await Active(session.Id, ct);
        if (conversation?.Id != id) return NotFound<MessageModel>();
        var messageKind = Enum.Parse<StrangerMessageKind>(kind);
        var existing = await db.Messages.SingleOrDefaultAsync(
            item => item.ConversationId == id && item.SenderSessionId == session.Id && item.ClientMessageId == clientId, ct);
        if (existing is not null)
        {
            if (existing.Content != Normalize(messageKind, content) || existing.Kind != messageKind || existing.ReplyToMessageId != replyToMessageId)
                return StrangerResult<MessageModel>.Failed("idempotency_conflict", "Mã gửi đã được dùng cho nội dung khác.", 409);
            return StrangerResult<MessageModel>.Success(await ToModelAsync(existing, session.Id, ct));
        }

        if (replyToMessageId.HasValue && !await db.Messages.AsNoTracking()
                .AnyAsync(item => item.Id == replyToMessageId && item.ConversationId == id, ct))
            return NotFound<MessageModel>();

        var message = StrangerMessage.Create(id, session.Id, clientId, messageKind, content, replyToMessageId, clock.GetUtcNow());
        db.Messages.Add(message);
        await db.SaveChangesAsync(ct);
        var model = await ToModelAsync(message, session.Id, ct);
        await notifications.MessageCreatedAsync(id, model, ct);
        return StrangerResult<MessageModel>.Success(model);
    }

    public async Task<StrangerResult<MessageModel>> SetReactionAsync(
        string token, Guid conversationId, Guid messageId, string? reaction, CancellationToken ct)
    {
        var session = await Session(token, ct);
        if (session is null) return Unauthorized<MessageModel>();
        var conversation = await db.Conversations.SingleOrDefaultAsync(
            item => item.Id == conversationId && (item.FirstSessionId == session.Id || item.SecondSessionId == session.Id), ct);
        if (conversation is null || !conversation.IsActive) return NotFound<MessageModel>();
        var message = await db.Messages.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == messageId && item.ConversationId == conversationId, ct);
        if (message is null) return NotFound<MessageModel>();

        var current = await db.Reactions.SingleOrDefaultAsync(
            item => item.MessageId == messageId && item.SessionId == session.Id, ct);
        if (reaction is null)
        {
            if (current is not null) db.Reactions.Remove(current);
        }
        else if (current is null)
        {
            db.Reactions.Add(StrangerMessageReaction.Create(messageId, session.Id, reaction, clock.GetUtcNow()));
        }
        else if (current.Reaction != reaction)
        {
            current.Change(reaction, clock.GetUtcNow());
        }

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException) when (reaction is not null)
        {
            db.ChangeTracker.Clear();
            current = await db.Reactions.SingleOrDefaultAsync(
                item => item.MessageId == messageId && item.SessionId == session.Id, ct);
            if (current is null) throw;
            if (current.Reaction != reaction)
            {
                current.Change(reaction, clock.GetUtcNow());
                await db.SaveChangesAsync(ct);
            }
        }
        var model = await ToModelAsync(message, session.Id, ct);
        await notifications.MessageChangedAsync(conversationId, messageId, ct);
        return StrangerResult<MessageModel>.Success(model);
    }
    public async Task<StrangerResult<MessageModel>> EditMessageAsync(
        string token, Guid conversationId, Guid messageId, string content, Guid expectedVersion, CancellationToken ct)
    {
        var session = await Session(token, ct);
        if (session is null) return Unauthorized<MessageModel>();
        var conversation = await db.Conversations.SingleOrDefaultAsync(
            item => item.Id == conversationId && (item.FirstSessionId == session.Id || item.SecondSessionId == session.Id), ct);
        if (conversation is null || !conversation.IsActive) return NotFound<MessageModel>();
        var message = await db.Messages.SingleOrDefaultAsync(
            item => item.Id == messageId && item.ConversationId == conversationId, ct);
        if (message is null) return NotFound<MessageModel>();
        if (message.SenderSessionId != session.Id || message.Kind is not StrangerMessageKind.Text)
            return StrangerResult<MessageModel>.Failed("message_edit_forbidden", "Bạn chỉ có thể sửa tin nhắn văn bản do chính mình gửi.", 403);
        if (message.Version != expectedVersion)
            return StrangerResult<MessageModel>.Failed("message_edit_conflict", "Tin nhắn đã được thay đổi. Vui lòng tải lại rồi thử lại.", 409);

        db.Entry(message).Property(item => item.Version).OriginalValue = expectedVersion;
        message.Edit(content, clock.GetUtcNow());
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return StrangerResult<MessageModel>.Failed("message_edit_conflict", "Tin nhắn đã được thay đổi. Vui lòng tải lại rồi thử lại.", 409);
        }

        var model = await ToModelAsync(message, session.Id, ct);
        await notifications.MessageChangedAsync(conversationId, messageId, CancellationToken.None);
        return StrangerResult<MessageModel>.Success(model);
    }
    public async Task<StrangerResult<bool>> EndAsync(string token, Guid id, bool block, CancellationToken ct) { var s = await Session(token, ct); if (s is null) return Unauthorized<bool>(); var c = await Active(s.Id, ct); if (c?.Id != id) return NotFound<bool>(); if (block && !await db.Blocks.AnyAsync(x => x.BlockerSessionId == s.Id && x.BlockedSessionId == c.PartnerOf(s.Id), ct)) db.Blocks.Add(new StrangerBlock(s.Id, c.PartnerOf(s.Id), clock.GetUtcNow())); c.End(clock.GetUtcNow()); await db.SaveChangesAsync(ct); await notifications.ConversationEndedAsync(id, ct); return StrangerResult<bool>.Success(true); }
    public async Task<StrangerResult<bool>> ReportAsync(string token, Guid id, string reason, string? note, CancellationToken ct)
    {
        var s = await Session(token, ct);
        if (s is null) return Unauthorized<bool>();

        var c = await db.Conversations.SingleOrDefaultAsync(
            x => x.Id == id && (x.FirstSessionId == s.Id || x.SecondSessionId == s.Id),
            ct);
        if (c is null) return NotFound<bool>();

        var alreadyReported = await db.Reports.AnyAsync(
            x => x.ConversationId == id && x.ReporterSessionId == s.Id,
            ct);
        if (!alreadyReported)
        {
            db.Reports.Add(new StrangerReport(
                id,
                s.Id,
                c.PartnerOf(s.Id),
                reason,
                note?.Trim(),
                clock.GetUtcNow()));
        }

        var wasActive = c.IsActive;
        c.End(clock.GetUtcNow());

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: "UX_reports_conversation_reporter"
            })
        {
            return StrangerResult<bool>.Success(true);
        }

        if (wasActive)
        {
            await notifications.ConversationEndedAsync(id, ct);
        }

        return StrangerResult<bool>.Success(true);
    }
    public async Task<Guid?> ResolveActiveConversationAsync(string token, CancellationToken ct) { var s = await Session(token, ct); return s is null ? null : (await Active(s.Id, ct))?.Id; }
    private Task<StrangerConversation?> Active(Guid sid, CancellationToken ct) => db.Conversations.SingleOrDefaultAsync(x => x.EndedAtUtc == null && (x.FirstSessionId == sid || x.SecondSessionId == sid), ct);
    private async Task<ConversationModel> Model(StrangerConversation c, Guid sid, CancellationToken ct) { var partner = await db.Sessions.Where(x => x.Id == c.PartnerOf(sid)).Select(x => x.Nickname).SingleAsync(ct); return new(c.Id, partner, c.StartedAtUtc, c.IsActive); }
    private async Task<MessageModel> ToModelAsync(StrangerMessage message, Guid sessionId, CancellationToken ct) =>
        (await ToModelsAsync([message], sessionId, ct))[0];

    private async Task<IReadOnlyList<MessageModel>> ToModelsAsync(
        IReadOnlyList<StrangerMessage> messages, Guid sessionId, CancellationToken ct)
    {
        if (messages.Count == 0) return [];
        var replyIds = messages.Where(message => message.ReplyToMessageId.HasValue)
            .Select(message => message.ReplyToMessageId!.Value).Distinct().ToArray();
        IReadOnlyList<StrangerMessage> replies = replyIds.Length == 0
            ? []
            : await db.Messages.AsNoTracking().Where(message => replyIds.Contains(message.Id)).ToListAsync(ct);
        var ids = messages.Select(message => message.Id).ToArray();
        var reactions = await db.Reactions.AsNoTracking()
            .Where(reaction => ids.Contains(reaction.MessageId)).ToListAsync(ct);
        var replyLookup = replies.ToDictionary(reply => reply.Id);
        return messages.Select(message =>
        {
            MessageReplyModel? reply = null;
            if (message.ReplyToMessageId.HasValue && replyLookup.TryGetValue(message.ReplyToMessageId.Value, out var target))
                reply = new MessageReplyModel(target.Id, target.SenderSessionId == sessionId,
                    target.Kind.ToString(), Preview(target));
            var messageReactions = reactions.Where(reaction => reaction.MessageId == message.Id).ToArray();
            return new MessageModel(
                message.Id,
                message.ConversationId,
                message.ClientMessageId,
                message.Kind.ToString(),
                message.Content,
                message.SentAtUtc,
                message.EditedAtUtc,
                message.Version,
                message.SenderSessionId == sessionId,
                reply,
                messageReactions.GroupBy(reaction => reaction.Reaction)
                    .OrderBy(group => group.Key, StringComparer.Ordinal)
                    .Select(group => new MessageReactionModel(group.Key, group.Count())).ToArray(),
                messageReactions.SingleOrDefault(reaction => reaction.SessionId == sessionId)?.Reaction);
        }).ToArray();
    }

    private static string Normalize(StrangerMessageKind kind, string content) =>
        kind is StrangerMessageKind.Text ? content.Trim() : content;

    private static string Preview(StrangerMessage message) => message.Kind is StrangerMessageKind.Sticker
        ? message.Content
        : message.Content.Length <= 160 ? message.Content : $"{message.Content[..157]}...";
    private static GuestSessionModel SessionModel(GuestSession session) => new(session.Nickname, session.Gender.ToString(), session.GenderPreference.ToString(), session.LastSeenAtUtc.AddHours(24));
}
