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
    public async Task<StrangerResult<MessagePage>> GetMessagesAsync(string token, Guid id, int limit, CancellationToken ct) { var s = await Session(token, ct); if (s is null) return Unauthorized<MessagePage>(); var c = await db.Conversations.SingleOrDefaultAsync(x => x.Id == id && (x.FirstSessionId == s.Id || x.SecondSessionId == s.Id), ct); if (c is null) return NotFound<MessagePage>(); var items = await db.Messages.Where(x => x.ConversationId == id).OrderByDescending(x => x.SentAtUtc).ThenByDescending(x => x.Id).Take(Math.Clamp(limit, 1, 100)).Select(x => new MessageModel(x.Id, x.ConversationId, x.ClientMessageId, x.Content, x.SentAtUtc, x.SenderSessionId == s.Id)).ToListAsync(ct); items.Reverse(); return StrangerResult<MessagePage>.Success(new(items)); }
    public async Task<StrangerResult<MessageModel>> SendMessageAsync(string token, Guid id, Guid clientId, string content, CancellationToken ct) { var s = await Session(token, ct); if (s is null) return Unauthorized<MessageModel>(); var c = await Active(s.Id, ct); if (c?.Id != id) return NotFound<MessageModel>(); var existing = await db.Messages.SingleOrDefaultAsync(x => x.ConversationId == id && x.SenderSessionId == s.Id && x.ClientMessageId == clientId, ct); if (existing is not null) { if (existing.Content != content.Trim()) return StrangerResult<MessageModel>.Failed("idempotency_conflict", "Mã gửi đã được dùng cho nội dung khác.", 409); return StrangerResult<MessageModel>.Success(ToModel(existing, true)); } var message = StrangerMessage.Create(id, s.Id, clientId, content, clock.GetUtcNow()); db.Messages.Add(message); await db.SaveChangesAsync(ct); var model = ToModel(message, true); await notifications.MessageCreatedAsync(id, model, ct); return StrangerResult<MessageModel>.Success(model); }
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
    private static MessageModel ToModel(StrangerMessage m, bool mine) => new(m.Id, m.ConversationId, m.ClientMessageId, m.Content, m.SentAtUtc, mine);
    private static GuestSessionModel SessionModel(GuestSession session) => new(session.Nickname, session.Gender.ToString(), session.GenderPreference.ToString(), session.LastSeenAtUtc.AddHours(24));
}
