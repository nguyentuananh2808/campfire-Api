using System.Security.Cryptography;
using Campfire.Modules.Identity.Contracts;
using Campfire.Modules.Werewolf.Application.Abstractions;
using Campfire.Modules.Werewolf.Application.Common;
using Campfire.Modules.Werewolf.Application.Models;
using Campfire.Modules.Werewolf.Domain.Games;
using Campfire.Modules.Werewolf.Domain.Roles;
using Campfire.Modules.Werewolf.Domain.Rooms;
using Campfire.Modules.Werewolf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Campfire.Modules.Werewolf.Infrastructure.Services;

public sealed class EfWerewolfService(WerewolfDbContext dbContext, IUserProfileReader profiles,
    IWerewolfNotificationPublisher notifications, IWerewolfVoiceGateway voice, TimeProvider timeProvider) : IWerewolfService
{
    private static readonly TimeSpan NightDuration = TimeSpan.FromSeconds(45);
    private static readonly TimeSpan DiscussionDuration = TimeSpan.FromMinutes(3);
    private static readonly TimeSpan VotingDuration = TimeSpan.FromSeconds(90);

    public async Task<WerewolfResult<IReadOnlyList<WerewolfRoomSummary>>> ListPublicAsync(Guid userId, CancellationToken cancellationToken)
    {
        var rooms = await dbContext.Rooms.AsNoTracking().Where(room => room.Visibility == RoomVisibility.Public && room.Status == RoomStatus.Lobby)
            .OrderByDescending(room => room.CreatedAtUtc).Take(100)
            .Select(room => new WerewolfRoomSummary(room.Id, room.Code, room.Name, room.Visibility.ToString(), room.Status.ToString(),
                dbContext.RoomPlayers.Count(player => player.RoomId == room.Id && player.Kind == PlayerKind.Player), room.MaximumPlayers, room.CreatedAtUtc))
            .ToListAsync(cancellationToken);
        return WerewolfResult<IReadOnlyList<WerewolfRoomSummary>>.Success(rooms);
    }

    public async Task<WerewolfResult<WerewolfRoomSnapshot>> CreateAsync(Guid userId, string name, string visibility,
        int minimumPlayers, int maximumPlayers, bool deadVoiceEnabled, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var room = WerewolfRoom.Create(userId, CreateRoomCode(), name,
                Enum.Parse<RoomVisibility>(visibility, true), minimumPlayers, maximumPlayers, deadVoiceEnabled, now);
            dbContext.Rooms.Add(room); dbContext.RoomPlayers.Add(new RoomPlayer(room.Id, userId, PlayerKind.Player, now));
            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
                await notifications.RoomChangedAsync(room.Id, room.Version, cancellationToken);
                return await BuildSnapshotAsync(room, userId, cancellationToken);
            }
            catch (DbUpdateException) when (attempt < 4)
            {
                dbContext.ChangeTracker.Clear();
            }
        }
        return WerewolfResult<WerewolfRoomSnapshot>.Failed(WerewolfFailures.Conflict("Không thể cấp mã phòng duy nhất. Vui lòng thử lại."));
    }

    public async Task<WerewolfResult<WerewolfRoomSnapshot>> JoinAsync(Guid userId, string code, bool spectator, CancellationToken cancellationToken)
    {
        var normalized = code.Trim().ToUpperInvariant();
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var room = await dbContext.Rooms.FromSqlInterpolated($@"SELECT * FROM werewolf.""Rooms"" WHERE ""Code"" = {normalized} FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken);
        if (room is null) return WerewolfResult<WerewolfRoomSnapshot>.Failed(WerewolfFailures.Hidden());
        var existing = await dbContext.RoomPlayers.SingleOrDefaultAsync(x => x.RoomId == room.Id && x.UserId == userId, cancellationToken);
        if (existing is null)
        {
            if (!spectator && room.Status != RoomStatus.Lobby) return WerewolfResult<WerewolfRoomSnapshot>.Failed(WerewolfFailures.Conflict("Trận đã bắt đầu; chỉ có thể vào xem."));
            var count = await dbContext.RoomPlayers.CountAsync(x => x.RoomId == room.Id && x.Kind == PlayerKind.Player, cancellationToken);
            if (!spectator && count >= room.MaximumPlayers) return WerewolfResult<WerewolfRoomSnapshot>.Failed(WerewolfFailures.Full());
            dbContext.RoomPlayers.Add(new RoomPlayer(room.Id, userId, spectator ? PlayerKind.Spectator : PlayerKind.Player, timeProvider.GetUtcNow()));
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        await transaction.CommitAsync(cancellationToken);
        await notifications.RoomChangedAsync(room.Id, room.Version, cancellationToken);
        return await BuildSnapshotAsync(room, userId, cancellationToken);
    }

    public async Task<WerewolfResult<WerewolfRoomSnapshot>> GetAsync(Guid roomId, Guid userId, CancellationToken cancellationToken)
    {
        if (!await IsParticipantAsync(roomId, userId, cancellationToken)) return WerewolfResult<WerewolfRoomSnapshot>.Failed(WerewolfFailures.Hidden());
        var room = await dbContext.Rooms.AsNoTracking().SingleOrDefaultAsync(x => x.Id == roomId, cancellationToken);
        return room is null ? WerewolfResult<WerewolfRoomSnapshot>.Failed(WerewolfFailures.Hidden()) : await BuildSnapshotAsync(room, userId, cancellationToken);
    }

    public async Task<WerewolfResult<WerewolfRoomSnapshot>> UpdateAsync(Guid roomId, Guid userId, Guid expectedVersion,
        string name, string visibility, int minimumPlayers, int maximumPlayers, bool deadVoiceEnabled, CancellationToken cancellationToken)
    {
        var room = await dbContext.Rooms.SingleOrDefaultAsync(x => x.Id == roomId, cancellationToken);
        if (room is null || !await IsParticipantAsync(roomId, userId, cancellationToken)) return WerewolfResult<WerewolfRoomSnapshot>.Failed(WerewolfFailures.Hidden());
        if (room.HostUserId != userId) return WerewolfResult<WerewolfRoomSnapshot>.Failed(WerewolfFailures.Forbidden());
        var playerCount = await dbContext.RoomPlayers.CountAsync(x => x.RoomId == roomId && x.Kind == PlayerKind.Player, cancellationToken);
        if (maximumPlayers < playerCount) return WerewolfResult<WerewolfRoomSnapshot>.Failed(WerewolfFailures.Conflict("Số người tối đa không thể thấp hơn số người hiện có."));
        if (!room.TryUpdate(expectedVersion, name, Enum.Parse<RoomVisibility>(visibility, true), minimumPlayers, maximumPlayers, deadVoiceEnabled, timeProvider.GetUtcNow()))
            return WerewolfResult<WerewolfRoomSnapshot>.Failed(WerewolfFailures.Conflict());
        try { await dbContext.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException) { return WerewolfResult<WerewolfRoomSnapshot>.Failed(WerewolfFailures.Conflict()); }
        await notifications.RoomChangedAsync(roomId, room.Version, cancellationToken);
        return await BuildSnapshotAsync(room, userId, cancellationToken);
    }

    public async Task<WerewolfResult<WerewolfRoomSnapshot>> SetReadyAsync(Guid roomId, Guid userId, bool ready, CancellationToken cancellationToken)
    {
        var room = await dbContext.Rooms.SingleOrDefaultAsync(x => x.Id == roomId, cancellationToken);
        var player = await dbContext.RoomPlayers.SingleOrDefaultAsync(x => x.RoomId == roomId && x.UserId == userId, cancellationToken);
        if (room is null || player is null) return WerewolfResult<WerewolfRoomSnapshot>.Failed(WerewolfFailures.Hidden());
        if (room.Status != RoomStatus.Lobby || player.Kind != PlayerKind.Player) return WerewolfResult<WerewolfRoomSnapshot>.Failed(WerewolfFailures.Conflict());
        player.SetReady(ready); await dbContext.SaveChangesAsync(cancellationToken);
        await notifications.RoomChangedAsync(roomId, room.Version, cancellationToken);
        return await BuildSnapshotAsync(room, userId, cancellationToken);
    }

    public async Task<WerewolfResult<WerewolfRoomSnapshot>> StartAsync(Guid roomId, Guid userId, Guid expectedVersion, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
        var room = await dbContext.Rooms.SingleOrDefaultAsync(x => x.Id == roomId, cancellationToken);
        if (room is null || !await IsParticipantAsync(roomId, userId, cancellationToken)) return WerewolfResult<WerewolfRoomSnapshot>.Failed(WerewolfFailures.Hidden());
        if (room.HostUserId != userId) return WerewolfResult<WerewolfRoomSnapshot>.Failed(WerewolfFailures.Forbidden());
        var roomPlayers = await dbContext.RoomPlayers.Where(x => x.RoomId == roomId && x.Kind == PlayerKind.Player).OrderBy(x => x.JoinedAtUtc).ToListAsync(cancellationToken);
        if (roomPlayers.Count < room.MinimumPlayers || roomPlayers.Any(x => !x.IsReady))
            return WerewolfResult<WerewolfRoomSnapshot>.Failed(WerewolfFailures.Conflict("Cần đủ người và tất cả người chơi phải sẵn sàng."));
        var now = timeProvider.GetUtcNow();
        if (!room.TryStart(expectedVersion, now)) return WerewolfResult<WerewolfRoomSnapshot>.Failed(WerewolfFailures.Conflict());
        var game = new WerewolfGame(roomId, now, NightDuration); dbContext.Games.Add(game);
        var deck = WerewolfRoleRegistry.BuildDeck(roomPlayers.Count).ToArray();
        Shuffle(deck);
        for (var index = 0; index < roomPlayers.Count; index++) dbContext.GamePlayers.Add(new GamePlayer(game.Id, roomPlayers[index].UserId, deck[index], index + 1));
        dbContext.Events.Add(new GameEvent(game.Id, game.PhaseSequence, "game.started", "Trận đấu bắt đầu. Mọi người đi ngủ.", null, now));
        try { await dbContext.SaveChangesAsync(cancellationToken); await transaction.CommitAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException) { return WerewolfResult<WerewolfRoomSnapshot>.Failed(WerewolfFailures.Conflict()); }
        await notifications.RoomChangedAsync(roomId, room.Version, cancellationToken);
        await notifications.VoicePermissionsChangedAsync(roomId, cancellationToken);
        return await BuildSnapshotAsync(room, userId, cancellationToken);
    }

    public async Task<WerewolfResult<WerewolfRoomSnapshot>> SubmitActionAsync(Guid gameId, Guid userId, Guid clientActionId,
        string kind, Guid? targetUserId, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted, cancellationToken);
        var game = await LockGameAsync(gameId, cancellationToken);
        if (game is null) return WerewolfResult<WerewolfRoomSnapshot>.Failed(WerewolfFailures.Hidden());
        var now = timeProvider.GetUtcNow();
        if (game.PhaseEndsAtUtc <= now) return WerewolfResult<WerewolfRoomSnapshot>.Failed(WerewolfFailures.Conflict("Lượt đã hết thời gian; đang chờ server chuyển pha."));
        var player = await dbContext.GamePlayers.SingleOrDefaultAsync(x => x.GameId == gameId && x.UserId == userId, cancellationToken);
        if (player is null || !player.IsAlive || !await IsParticipantAsync(game.RoomId, userId, cancellationToken)) return WerewolfResult<WerewolfRoomSnapshot>.Failed(WerewolfFailures.Hidden());
        var actionByClientId = await dbContext.Actions.AsNoTracking()
            .SingleOrDefaultAsync(x => x.GameId == gameId && x.ActorUserId == userId && x.ClientActionId == clientActionId, cancellationToken);
        if (actionByClientId is not null)
        {
            if (actionByClientId.PhaseSequence != game.PhaseSequence ||
                !string.Equals(actionByClientId.Kind.ToString(), kind, StringComparison.OrdinalIgnoreCase) ||
                actionByClientId.TargetUserId != targetUserId)
                return WerewolfResult<WerewolfRoomSnapshot>.Failed(WerewolfFailures.Conflict("clientActionId đã được dùng cho một hành động khác."));
            await transaction.RollbackAsync(cancellationToken);
            return await BuildSnapshotForGameAsync(game, userId, cancellationToken);
        }
        if (await dbContext.Actions.AnyAsync(x => x.GameId == gameId && x.PhaseSequence == game.PhaseSequence && x.ActorUserId == userId, cancellationToken))
            return WerewolfResult<WerewolfRoomSnapshot>.Failed(WerewolfFailures.Conflict("Bạn đã hành động trong lượt này."));
        var actionKind = Enum.Parse<GameActionKind>(kind, true);
        var validation = ValidateAction(game, player, actionKind, targetUserId);
        if (validation is not null) return WerewolfResult<WerewolfRoomSnapshot>.Failed(validation);
        var target = targetUserId.HasValue
            ? await dbContext.GamePlayers.SingleOrDefaultAsync(x => x.GameId == gameId && x.UserId == targetUserId && x.IsAlive, cancellationToken)
            : null;
        if (targetUserId.HasValue && target is null)
            return WerewolfResult<WerewolfRoomSnapshot>.Failed(WerewolfFailures.Conflict("Mục tiêu không còn hợp lệ."));
        if (actionKind == GameActionKind.WerewolfTarget && target?.Role == WerewolfRole.Werewolf)
            return WerewolfResult<WerewolfRoomSnapshot>.Failed(WerewolfFailures.Conflict("Ma Sói không thể tấn công đồng đội."));
        if (actionKind == GameActionKind.Protect && player.LastProtectedUserId == targetUserId)
            return WerewolfResult<WerewolfRoomSnapshot>.Failed(WerewolfFailures.Conflict("Bảo vệ không được chọn cùng một người hai đêm liên tiếp."));
        if (actionKind == GameActionKind.WitchHeal && !player.HasHealPotion || actionKind == GameActionKind.WitchPoison && !player.HasPoisonPotion)
            return WerewolfResult<WerewolfRoomSnapshot>.Failed(WerewolfFailures.Conflict("Bình thuốc này đã được sử dụng."));
        dbContext.Actions.Add(new GameAction(gameId, game.PhaseSequence, userId, clientActionId, actionKind, targetUserId, now));
        if (actionKind == GameActionKind.Protect && targetUserId.HasValue) player.RememberProtection(targetUserId.Value);
        if (actionKind == GameActionKind.WitchHeal) player.UseHeal();
        if (actionKind == GameActionKind.WitchPoison) player.UsePoison();
        if (actionKind == GameActionKind.Investigate && targetUserId.HasValue)
        {
            var faction = WerewolfRoleRegistry.FactionOf(target!.Role) == RoleFaction.Werewolves ? "Ma Sói" : "Dân làng";
            dbContext.Events.Add(new GameEvent(gameId, game.PhaseSequence, "seer.result", $"Kết quả soi: {faction}.", userId, now));
        }
        await dbContext.SaveChangesAsync(cancellationToken);
        var oldSequence = game.PhaseSequence;
        if (await IsCurrentActionCompleteAsync(game, cancellationToken)) await AdvanceCoreAsync(game, now, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken); await transaction.CommitAsync(cancellationToken);
        await AfterStateChangeAsync(game.RoomId, game.Id, oldSequence, game.PhaseSequence, game.Version, cancellationToken);
        return await BuildSnapshotForGameAsync(game, userId, cancellationToken);
    }

    public async Task<WerewolfResult<WerewolfRoomSnapshot>> SubmitVoteAsync(Guid gameId, Guid userId, Guid clientActionId, Guid? targetUserId, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted, cancellationToken);
        var game = await LockGameAsync(gameId, cancellationToken);
        if (game is null || game.Phase != GamePhase.DayVoting) return WerewolfResult<WerewolfRoomSnapshot>.Failed(WerewolfFailures.InvalidPhase());
        if (game.PhaseEndsAtUtc <= timeProvider.GetUtcNow()) return WerewolfResult<WerewolfRoomSnapshot>.Failed(WerewolfFailures.Conflict("Lượt bỏ phiếu đã hết thời gian."));
        var voter = await dbContext.GamePlayers.SingleOrDefaultAsync(x => x.GameId == gameId && x.UserId == userId, cancellationToken);
        if (voter is null || !voter.IsAlive || !await IsParticipantAsync(game.RoomId, userId, cancellationToken)) return WerewolfResult<WerewolfRoomSnapshot>.Failed(WerewolfFailures.Hidden());
        var voteByClientId = await dbContext.Votes.AsNoTracking()
            .SingleOrDefaultAsync(x => x.GameId == gameId && x.VoterUserId == userId && x.ClientActionId == clientActionId, cancellationToken);
        if (voteByClientId is not null)
        {
            if (voteByClientId.PhaseSequence != game.PhaseSequence || voteByClientId.TargetUserId != targetUserId)
                return WerewolfResult<WerewolfRoomSnapshot>.Failed(WerewolfFailures.Conflict("clientActionId đã được dùng cho một phiếu khác."));
            await transaction.RollbackAsync(cancellationToken);
            return await BuildSnapshotForGameAsync(game, userId, cancellationToken);
        }
        var existing = await dbContext.Votes.SingleOrDefaultAsync(x => x.GameId == gameId && x.PhaseSequence == game.PhaseSequence && x.VoterUserId == userId, cancellationToken);
        if (existing is not null)
        {
            return WerewolfResult<WerewolfRoomSnapshot>.Failed(WerewolfFailures.Conflict("Bạn đã bỏ phiếu trong lượt này."));
        }
        if (targetUserId.HasValue && !await dbContext.GamePlayers.AnyAsync(x => x.GameId == gameId && x.UserId == targetUserId && x.IsAlive, cancellationToken))
            return WerewolfResult<WerewolfRoomSnapshot>.Failed(WerewolfFailures.Conflict("Mục tiêu không còn hợp lệ."));
        dbContext.Votes.Add(new GameVote(gameId, game.PhaseSequence, userId, clientActionId, targetUserId, timeProvider.GetUtcNow()));
        await dbContext.SaveChangesAsync(cancellationToken);
        var aliveCount = await dbContext.GamePlayers.CountAsync(x => x.GameId == gameId && x.IsAlive, cancellationToken);
        var voteCount = await dbContext.Votes.CountAsync(x => x.GameId == gameId && x.PhaseSequence == game.PhaseSequence, cancellationToken);
        var oldSequence = game.PhaseSequence;
        if (voteCount >= aliveCount) await AdvanceCoreAsync(game, timeProvider.GetUtcNow(), cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken); await transaction.CommitAsync(cancellationToken);
        await AfterStateChangeAsync(game.RoomId, game.Id, oldSequence, game.PhaseSequence, game.Version, cancellationToken);
        return await BuildSnapshotForGameAsync(game, userId, cancellationToken);
    }

    public async Task<WerewolfResult<bool>> LeaveAsync(Guid roomId, Guid userId, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
        var room = await dbContext.Rooms.SingleOrDefaultAsync(x => x.Id == roomId, cancellationToken);
        var player = await dbContext.RoomPlayers.SingleOrDefaultAsync(x => x.RoomId == roomId && x.UserId == userId, cancellationToken);
        if (room is null || player is null) return WerewolfResult<bool>.Failed(WerewolfFailures.Hidden());
        if (room.Status == RoomStatus.InProgress)
        {
            var now = timeProvider.GetUtcNow();
            player.SetConnected(false, now);
            if (room.HostUserId == userId)
            {
                var nextHost = await dbContext.RoomPlayers
                    .Where(x => x.RoomId == roomId && x.UserId != userId && x.Kind == PlayerKind.Player)
                    .OrderByDescending(x => x.IsConnected)
                    .ThenBy(x => x.JoinedAtUtc)
                    .FirstOrDefaultAsync(cancellationToken);
                if (nextHost is not null) room.TransferHost(nextHost.UserId, now);
            }
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            await notifications.RoomChangedAsync(roomId, room.Version, cancellationToken);
            return WerewolfResult<bool>.Success(true);
        }
        dbContext.RoomPlayers.Remove(player);
        if (room.HostUserId == userId)
        {
            var next = await dbContext.RoomPlayers
                .Where(x => x.RoomId == roomId && x.UserId != userId && x.Kind == PlayerKind.Player)
                .OrderBy(x => x.JoinedAtUtc)
                .FirstOrDefaultAsync(cancellationToken);
            if (next is not null) room.TransferHost(next.UserId, timeProvider.GetUtcNow());
            else dbContext.Rooms.Remove(room);
        }
        await dbContext.SaveChangesAsync(cancellationToken); await transaction.CommitAsync(cancellationToken);
        await notifications.RoomChangedAsync(roomId, room.Version, cancellationToken);
        return WerewolfResult<bool>.Success(true);
    }

    public async Task<WerewolfResult<VoiceAccessView>> IssueVoiceAccessAsync(Guid roomId, Guid userId, CancellationToken cancellationToken)
    {
        if (!voice.IsConfigured) return WerewolfResult<VoiceAccessView>.Failed(WerewolfFailures.VoiceUnavailable());
        var room = await dbContext.Rooms.AsNoTracking().SingleOrDefaultAsync(x => x.Id == roomId, cancellationToken);
        var member = await dbContext.RoomPlayers.AsNoTracking().SingleOrDefaultAsync(x => x.RoomId == roomId && x.UserId == userId, cancellationToken);
        if (room is null || member is null) return WerewolfResult<VoiceAccessView>.Failed(WerewolfFailures.Hidden());
        var game = await dbContext.Games.AsNoTracking().SingleOrDefaultAsync(x => x.RoomId == roomId, cancellationToken);
        var channel = "public"; var canSubscribe = true; var canPublish = member.Kind == PlayerKind.Player && !member.IsHostMuted;
        if (game is not null && game.Phase != GamePhase.Finished)
        {
            var gamePlayer = await dbContext.GamePlayers.AsNoTracking().SingleOrDefaultAsync(x => x.GameId == game.Id && x.UserId == userId, cancellationToken);
            if (gamePlayer is null) return WerewolfResult<VoiceAccessView>.Failed(WerewolfFailures.VoiceUnavailable());
            if (!gamePlayer.IsAlive)
            {
                if (!room.DeadVoiceEnabled) return WerewolfResult<VoiceAccessView>.Failed(WerewolfFailures.VoiceUnavailable());
                channel = "dead"; canPublish = !member.IsHostMuted;
            }
            else if (game.Phase == GamePhase.NightWerewolves && gamePlayer.Role == WerewolfRole.Werewolf) channel = "wolves";
            else if (game.Phase is GamePhase.DayDiscussion or GamePhase.DayVoting) channel = "public";
            else return WerewolfResult<VoiceAccessView>.Failed(WerewolfFailures.VoiceUnavailable());
        }
        var displayName = (await profiles.GetByIdsAsync([userId], cancellationToken)).GetValueOrDefault(userId)?.DisplayName ?? "Người chơi";
        var gameId = game?.Id ?? Guid.Empty; var phaseSequence = game?.PhaseSequence ?? 0;
        return WerewolfResult<VoiceAccessView>.Success(voice.IssueAccess(VoiceRoomName(roomId, gameId, phaseSequence, channel), userId, displayName, channel, canPublish, canSubscribe, timeProvider.GetUtcNow()));
    }

    public async Task<WerewolfResult<bool>> SetConnectionAsync(Guid roomId, Guid userId, bool connected, CancellationToken cancellationToken)
    {
        var player = await dbContext.RoomPlayers.SingleOrDefaultAsync(x => x.RoomId == roomId && x.UserId == userId, cancellationToken);
        if (player is null) return WerewolfResult<bool>.Failed(WerewolfFailures.Hidden());
        player.SetConnected(connected, timeProvider.GetUtcNow()); await dbContext.SaveChangesAsync(cancellationToken);
        var version = await dbContext.Rooms.Where(x => x.Id == roomId).Select(x => x.Version).SingleAsync(cancellationToken);
        await notifications.RoomChangedAsync(roomId, version, cancellationToken); return WerewolfResult<bool>.Success(true);
    }

    public async Task<WerewolfResult<WerewolfRoomSnapshot>> ModerateAsync(Guid roomId, Guid userId, Guid targetUserId, string action, CancellationToken cancellationToken)
    {
        var room = await dbContext.Rooms.SingleOrDefaultAsync(x => x.Id == roomId, cancellationToken);
        if (room is null || !await IsParticipantAsync(roomId, userId, cancellationToken)) return WerewolfResult<WerewolfRoomSnapshot>.Failed(WerewolfFailures.Hidden());
        if (room.HostUserId != userId || targetUserId == userId) return WerewolfResult<WerewolfRoomSnapshot>.Failed(WerewolfFailures.Forbidden());
        var target = await dbContext.RoomPlayers.SingleOrDefaultAsync(x => x.RoomId == roomId && x.UserId == targetUserId, cancellationToken);
        if (target is null) return WerewolfResult<WerewolfRoomSnapshot>.Failed(WerewolfFailures.Hidden());
        if (action == "kick")
        {
            if (room.Status != RoomStatus.Lobby) return WerewolfResult<WerewolfRoomSnapshot>.Failed(WerewolfFailures.Conflict("Không thể kick giữa trận; host có thể mute và người mất kết nối vẫn được phép quay lại."));
            dbContext.RoomPlayers.Remove(target);
        }
        else target.SetHostMuted(action == "mute");
        await dbContext.SaveChangesAsync(cancellationToken);
        var game = await dbContext.Games.AsNoTracking().SingleOrDefaultAsync(x => x.RoomId == roomId, cancellationToken);
        await voice.ClosePhaseRoomsAsync(roomId, game?.Id ?? Guid.Empty, game?.PhaseSequence ?? 0, cancellationToken);
        await notifications.RoomChangedAsync(roomId, room.Version, cancellationToken);
        await notifications.VoicePermissionsChangedAsync(roomId, cancellationToken);
        return await BuildSnapshotAsync(room, userId, cancellationToken);
    }

    public async Task AdvanceDueGamesAsync(CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var ids = await dbContext.Games.AsNoTracking().Where(x => x.Phase != GamePhase.Finished && x.PhaseEndsAtUtc <= now).OrderBy(x => x.PhaseEndsAtUtc).Select(x => x.Id).Take(25).ToListAsync(cancellationToken);
        foreach (var id in ids)
        {
            dbContext.ChangeTracker.Clear();
            await using var transaction = await dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
            var game = await LockGameAsync(id, cancellationToken);
            if (game is null || game.PhaseEndsAtUtc > now) { await transaction.RollbackAsync(cancellationToken); continue; }
            var oldSequence = game.PhaseSequence; await AdvanceCoreAsync(game, now, cancellationToken); await dbContext.SaveChangesAsync(cancellationToken); await transaction.CommitAsync(cancellationToken);
            await AfterStateChangeAsync(game.RoomId, game.Id, oldSequence, game.PhaseSequence, game.Version, cancellationToken);
        }
    }

    private async Task<WerewolfGame?> LockGameAsync(Guid gameId, CancellationToken cancellationToken) =>
        await dbContext.Games.FromSqlInterpolated($@"SELECT * FROM werewolf.""Games"" WHERE ""Id"" = {gameId} FOR UPDATE").SingleOrDefaultAsync(cancellationToken);

    private static WerewolfFailure? ValidateAction(WerewolfGame game, GamePlayer player, GameActionKind kind, Guid? target)
    {
        var expected = game.Phase switch
        {
            GamePhase.NightWerewolves => player.Role == WerewolfRole.Werewolf && kind == GameActionKind.WerewolfTarget,
            GamePhase.NightSeer => player.Role == WerewolfRole.Seer && kind == GameActionKind.Investigate,
            GamePhase.NightGuard => player.Role == WerewolfRole.Guard && kind == GameActionKind.Protect,
            GamePhase.NightWitch => player.Role == WerewolfRole.Witch && kind is GameActionKind.WitchHeal or GameActionKind.WitchPoison or GameActionKind.WitchPass,
            _ => false
        };
        if (!expected) return WerewolfFailures.InvalidPhase();
        if (kind is not GameActionKind.WitchPass && target is null) return WerewolfFailures.Conflict("Hành động cần một mục tiêu.");
        if ((kind is GameActionKind.WerewolfTarget or GameActionKind.Investigate) && target == player.UserId)
            return WerewolfFailures.Conflict("Vai trò này không thể chọn chính mình.");
        return null;
    }

    private async Task<bool> IsCurrentActionCompleteAsync(WerewolfGame game, CancellationToken cancellationToken)
    {
        var role = game.Phase switch { GamePhase.NightWerewolves => WerewolfRole.Werewolf, GamePhase.NightSeer => WerewolfRole.Seer, GamePhase.NightGuard => WerewolfRole.Guard, GamePhase.NightWitch => WerewolfRole.Witch, _ => (WerewolfRole?)null };
        if (role is null) return false;
        var eligible = await dbContext.GamePlayers.CountAsync(x => x.GameId == game.Id && x.IsAlive && x.Role == role, cancellationToken);
        var acted = await dbContext.Actions.CountAsync(x => x.GameId == game.Id && x.PhaseSequence == game.PhaseSequence, cancellationToken);
        return eligible == 0 || acted >= eligible;
    }

    private async Task AdvanceCoreAsync(WerewolfGame game, DateTimeOffset now, CancellationToken cancellationToken)
    {
        if (game.Phase == GamePhase.NightGuard) await CreateWitchInformationAsync(game, now, cancellationToken);
        if (game.Phase == GamePhase.NightWitch) await ResolveNightAsync(game, now, cancellationToken);
        if (game.Phase == GamePhase.DayVoting) await ResolveVoteAsync(game, now, cancellationToken);
        var players = await dbContext.GamePlayers.Where(x => x.GameId == game.Id).ToListAsync(cancellationToken);
        var winner = WerewolfWinEvaluator.Evaluate(players);
        if (winner.HasValue)
        {
            game.Finish(winner.Value, now);
            var room = await dbContext.Rooms.SingleAsync(x => x.Id == game.RoomId, cancellationToken); room.Finish(now);
            dbContext.Events.Add(new GameEvent(game.Id, game.PhaseSequence, "game.finished", winner == Winner.Village ? "Dân làng chiến thắng." : "Ma Sói chiến thắng.", null, now));
            return;
        }
        var (next, duration, message) = game.Phase switch
        {
            GamePhase.NightWerewolves => (GamePhase.NightSeer, NightDuration, "Tiên tri thức dậy."),
            GamePhase.NightSeer => (GamePhase.NightGuard, NightDuration, "Bảo vệ thức dậy."),
            GamePhase.NightGuard => (GamePhase.NightWitch, NightDuration, "Phù thủy thức dậy."),
            GamePhase.NightWitch => (GamePhase.DayDiscussion, DiscussionDuration, "Trời sáng. Bắt đầu thảo luận."),
            GamePhase.DayDiscussion => (GamePhase.DayVoting, VotingDuration, "Bắt đầu bỏ phiếu. Có thể bỏ phiếu trắng."),
            GamePhase.DayVoting => (GamePhase.NightWerewolves, NightDuration, "Mọi người đi ngủ. Ma Sói thức dậy."),
            _ => throw new InvalidOperationException("Unsupported phase transition.")
        };
        game.TryAdvance(game.Version, next, now, duration);
        dbContext.Events.Add(new GameEvent(game.Id, game.PhaseSequence, "phase.changed", message, null, now));
    }

    private async Task CreateWitchInformationAsync(WerewolfGame game, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var attack = await dbContext.Actions.Where(x => x.GameId == game.Id && x.PhaseSequence == game.PhaseSequence - 2 && x.Kind == GameActionKind.WerewolfTarget && x.TargetUserId != null)
            .GroupBy(x => x.TargetUserId).Select(group => new { Target = group.Key, Count = group.Count() })
            .OrderByDescending(x => x.Count).ThenBy(x => x.Target).Select(x => x.Target).FirstOrDefaultAsync(cancellationToken);
        var witch = await dbContext.GamePlayers.AsNoTracking().FirstOrDefaultAsync(x => x.GameId == game.Id && x.Role == WerewolfRole.Witch && x.IsAlive, cancellationToken);
        if (witch is null) return;
        var message = "Đêm nay Ma Sói không chọn được mục tiêu.";
        if (attack.HasValue)
        {
            var targetProfile = (await profiles.GetByIdsAsync([attack.Value], cancellationToken)).GetValueOrDefault(attack.Value);
            message = $"Ma Sói đã chọn {targetProfile?.DisplayName ?? "một người chơi"}. Bạn có thể dùng bình cứu.";
        }
        dbContext.Events.Add(new GameEvent(game.Id, game.PhaseSequence + 1, "witch.attack", message, witch.UserId, now));
    }

    private async Task ResolveNightAsync(WerewolfGame game, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var actions = await dbContext.Actions.Where(x => x.GameId == game.Id && x.PhaseSequence >= game.PhaseSequence - 3 && x.PhaseSequence <= game.PhaseSequence).ToListAsync(cancellationToken);
        var eliminated = WerewolfNightResolver.Resolve(actions);
        var victims = await dbContext.GamePlayers.Where(x => x.GameId == game.Id && eliminated.Contains(x.UserId) && x.IsAlive).ToListAsync(cancellationToken);
        foreach (var victim in victims) victim.Eliminate();
        dbContext.Events.Add(new GameEvent(game.Id, game.PhaseSequence, "night.resolved", victims.Count == 0 ? "Đêm qua không ai bị loại." : $"Đêm qua có {victims.Count} người bị loại.", null, now));
    }

    private async Task ResolveVoteAsync(WerewolfGame game, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var votes = await dbContext.Votes
            .Where(x => x.GameId == game.Id && x.PhaseSequence == game.PhaseSequence)
            .ToListAsync(cancellationToken);
        var eliminated = WerewolfVoteResolver.Resolve(votes);
        if (eliminated.HasValue)
        {
            var player = await dbContext.GamePlayers.SingleOrDefaultAsync(x => x.GameId == game.Id && x.UserId == eliminated && x.IsAlive, cancellationToken); player?.Eliminate();
        }
        dbContext.Events.Add(new GameEvent(game.Id, game.PhaseSequence, "vote.resolved", eliminated.HasValue ? "Một người chơi đã bị loại sau bỏ phiếu." : "Bỏ phiếu hòa hoặc trắng; không ai bị loại.", null, now));
    }

    private async Task<WerewolfResult<WerewolfRoomSnapshot>> BuildSnapshotForGameAsync(WerewolfGame game, Guid userId, CancellationToken cancellationToken)
    {
        var room = await dbContext.Rooms.AsNoTracking().SingleAsync(x => x.Id == game.RoomId, cancellationToken);
        return await BuildSnapshotAsync(room, userId, cancellationToken);
    }

    private async Task<WerewolfResult<WerewolfRoomSnapshot>> BuildSnapshotAsync(WerewolfRoom room, Guid userId, CancellationToken cancellationToken)
    {
        var members = await dbContext.RoomPlayers.AsNoTracking().Where(x => x.RoomId == room.Id).OrderBy(x => x.JoinedAtUtc).ToListAsync(cancellationToken);
        if (members.All(x => x.UserId != userId)) return WerewolfResult<WerewolfRoomSnapshot>.Failed(WerewolfFailures.Hidden());
        var game = await dbContext.Games.AsNoTracking().SingleOrDefaultAsync(x => x.RoomId == room.Id, cancellationToken);
        var gamePlayers = game is null ? [] : await dbContext.GamePlayers.AsNoTracking().Where(x => x.GameId == game.Id).OrderBy(x => x.Seat).ToListAsync(cancellationToken);
        var names = await profiles.GetByIdsAsync(members.Select(x => x.UserId).ToArray(), cancellationToken);
        var playerViews = members.Select((member, index) =>
        {
            var gamePlayer = gamePlayers.SingleOrDefault(x => x.UserId == member.UserId);
            return new WerewolfPlayerView(member.UserId, names.GetValueOrDefault(member.UserId)?.DisplayName ?? "Người chơi", gamePlayer?.Seat ?? index + 1,
                room.HostUserId == member.UserId, member.IsReady, member.IsConnected, gamePlayer?.IsAlive ?? true, false, member.IsHostMuted, member.Kind.ToString());
        }).ToArray();
        PrivateRoleView? roleView = null;
        var mine = gamePlayers.SingleOrDefault(x => x.UserId == userId);
        if (mine is not null)
        {
            var definition = WerewolfRoleRegistry.Get(mine.Role);
            var teammates = mine.Role == WerewolfRole.Werewolf ? gamePlayers.Where(x => x.Role == WerewolfRole.Werewolf && x.UserId != userId).Select(x => x.UserId).ToArray() : [];
            var investigation = await dbContext.Events.AsNoTracking().Where(x => x.GameId == game!.Id && x.RecipientUserId == userId && x.Kind == "seer.result").OrderByDescending(x => x.OccurredAtUtc).Select(x => x.Message).FirstOrDefaultAsync(cancellationToken);
            roleView = new PrivateRoleView(mine.Role.ToString(), definition.Name, definition.Faction.ToString(), definition.Icon,
                definition.Description, definition.Ability, definition.WinCondition, mine.HasHealPotion, mine.HasPoisonPotion,
                teammates, investigation);
        }
        var events = game is null ? [] : await dbContext.Events.AsNoTracking().Where(x => x.GameId == game.Id && (x.RecipientUserId == null || x.RecipientUserId == userId))
            .OrderByDescending(x => x.OccurredAtUtc).ThenByDescending(x => x.Id).Take(30).Select(x => new GameEventView(x.Id, x.Kind, x.Message, x.OccurredAtUtc)).ToArrayAsync(cancellationToken);
        var hasAction = game is not null && await dbContext.Actions.AsNoTracking().AnyAsync(x => x.GameId == game.Id && x.PhaseSequence == game.PhaseSequence && x.ActorUserId == userId, cancellationToken);
        var hasVote = game is not null && await dbContext.Votes.AsNoTracking().AnyAsync(x => x.GameId == game.Id && x.PhaseSequence == game.PhaseSequence && x.VoterUserId == userId, cancellationToken);
        var votesSubmitted = game?.Phase == GamePhase.DayVoting ? await dbContext.Votes.AsNoTracking().CountAsync(x => x.GameId == game.Id && x.PhaseSequence == game.PhaseSequence, cancellationToken) : 0;
        var eligibleVoters = game?.Phase == GamePhase.DayVoting ? gamePlayers.Count(x => x.IsAlive) : 0;
        var canAct = game is not null && mine is { IsAlive: true } && !hasAction && RoleCanAct(mine.Role, game.Phase);
        var canVote = game?.Phase == GamePhase.DayVoting && mine is { IsAlive: true } && !hasVote;
        var readyPlayers = members.Count(x => x.Kind == PlayerKind.Player && x.IsReady);
        return WerewolfResult<WerewolfRoomSnapshot>.Success(new WerewolfRoomSnapshot(room.Id, room.Code, room.Name, room.Visibility.ToString(), room.Status.ToString(), room.HostUserId,
            room.MinimumPlayers, room.MaximumPlayers, room.DeadVoiceEnabled, room.Version, game?.Id, game?.Phase.ToString(), game?.Round ?? 0, game?.PhaseSequence ?? 0,
            game?.PhaseEndsAtUtc, game?.Winner?.ToString(), playerViews, roleView, events, room.HostUserId == userId,
            room.Status == RoomStatus.Lobby && room.HostUserId == userId && readyPlayers >= room.MinimumPlayers && members.Where(x => x.Kind == PlayerKind.Player).All(x => x.IsReady), canAct, canVote, votesSubmitted, eligibleVoters));
    }

    private static bool RoleCanAct(WerewolfRole role, GamePhase phase) => (role, phase) switch
    { (WerewolfRole.Werewolf, GamePhase.NightWerewolves) => true, (WerewolfRole.Seer, GamePhase.NightSeer) => true, (WerewolfRole.Guard, GamePhase.NightGuard) => true, (WerewolfRole.Witch, GamePhase.NightWitch) => true, _ => false };

    private Task<bool> IsParticipantAsync(Guid roomId, Guid userId, CancellationToken cancellationToken) => dbContext.RoomPlayers.AsNoTracking().AnyAsync(x => x.RoomId == roomId && x.UserId == userId, cancellationToken);

    private async Task AfterStateChangeAsync(Guid roomId, Guid gameId, int oldSequence, int newSequence, Guid version, CancellationToken cancellationToken)
    {
        await notifications.RoomChangedAsync(roomId, version, cancellationToken);
        if (oldSequence != newSequence)
        {
            await voice.ClosePhaseRoomsAsync(roomId, gameId, oldSequence, cancellationToken);
            await notifications.VoicePermissionsChangedAsync(roomId, cancellationToken);
        }
    }

    public static string VoiceRoomName(Guid roomId, Guid gameId, int phaseSequence, string channel) => $"ww-{roomId:N}-{gameId:N}-{phaseSequence}-{channel}";

    private static void Shuffle<T>(T[] values)
    {
        for (var index = values.Length - 1; index > 0; index--)
        {
            var swap = RandomNumberGenerator.GetInt32(index + 1);
            (values[index], values[swap]) = (values[swap], values[index]);
        }
    }

    private static string CreateRoomCode()
    {
        const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var code = new char[8];
        for (var index = 0; index < code.Length; index++) code[index] = alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)];
        return new string(code);
    }
}
