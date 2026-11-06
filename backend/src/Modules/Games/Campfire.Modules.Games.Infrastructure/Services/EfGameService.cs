using Campfire.Modules.Games.Application.Abstractions;
using Campfire.Modules.Games.Application.Common;
using Campfire.Modules.Games.Application.Models;
using Campfire.Modules.Games.Domain.Caro;
using Campfire.Modules.Games.Infrastructure.Persistence;
using Campfire.Modules.Groups.Contracts;
using Campfire.Modules.Identity.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Campfire.Modules.Games.Infrastructure.Services;

public sealed class EfGameService(
    GamesDbContext dbContext,
    IGroupMembershipService membershipService,
    IUserProfileReader profileReader,
    TimeProvider timeProvider) : IGameService
{
    public async Task<GameResult<CaroGameView>> CreateAsync(Guid groupId, Guid userId, Guid opponentUserId,
        CancellationToken cancellationToken)
    {
        if (!(await membershipService.GetAccessAsync(groupId, userId, cancellationToken)).IsMember)
            return GameResult<CaroGameView>.Failed(GameFailures.Hidden());
        var members = await membershipService.GetMembersAsync(groupId, cancellationToken);
        if (opponentUserId == userId || members.All(member => member.UserId != opponentUserId))
            return GameResult<CaroGameView>.Failed(GameFailures.InvalidOpponent());

        var game = CaroGame.Create(groupId, userId, opponentUserId, timeProvider.GetUtcNow());
        dbContext.CaroGames.Add(game);
        await dbContext.SaveChangesAsync(cancellationToken);
        return GameResult<CaroGameView>.Success(await ToViewAsync(game, cancellationToken));
    }

    public async Task<GameResult<IReadOnlyList<CaroGameView>>> ListAsync(Guid userId, CancellationToken cancellationToken)
    {
        var games = await dbContext.CaroGames.AsNoTracking()
            .Where(game => game.XPlayerUserId == userId || game.OPlayerUserId == userId)
            .OrderByDescending(game => game.UpdatedAtUtc).Take(50).ToListAsync(cancellationToken);
        var allowedGroupIds = new HashSet<Guid>();
        foreach (var groupId in games.Select(game => game.GroupId).Distinct())
        {
            if ((await membershipService.GetAccessAsync(groupId, userId, cancellationToken)).IsMember)
                allowedGroupIds.Add(groupId);
        }
        games = games.Where(game => allowedGroupIds.Contains(game.GroupId)).ToList();
        var profiles = await ProfilesAsync(games, cancellationToken);
        return GameResult<IReadOnlyList<CaroGameView>>.Success(games.Select(game => ToView(game, profiles)).ToArray());
    }

    public async Task<GameResult<CaroGameView>> GetAsync(Guid gameId, Guid userId, CancellationToken cancellationToken)
    {
        var game = await dbContext.CaroGames.AsNoTracking().SingleOrDefaultAsync(candidate =>
            candidate.Id == gameId && (candidate.XPlayerUserId == userId || candidate.OPlayerUserId == userId), cancellationToken);
        if (game is not null && !(await membershipService.GetAccessAsync(game.GroupId, userId, cancellationToken)).IsMember)
            game = null;
        return game is null
            ? GameResult<CaroGameView>.Failed(GameFailures.Hidden())
            : GameResult<CaroGameView>.Success(await ToViewAsync(game, cancellationToken));
    }

    public async Task<GameResult<CaroGameView>> PlayAsync(Guid gameId, Guid userId, int cell, Guid expectedVersion,
        CancellationToken cancellationToken)
    {
        var game = await dbContext.CaroGames.SingleOrDefaultAsync(candidate =>
            candidate.Id == gameId && (candidate.XPlayerUserId == userId || candidate.OPlayerUserId == userId), cancellationToken);
        if (game is null) return GameResult<CaroGameView>.Failed(GameFailures.Hidden());
        if (!(await membershipService.GetAccessAsync(game.GroupId, userId, cancellationToken)).IsMember)
            return GameResult<CaroGameView>.Failed(GameFailures.Hidden());
        if (game.Version != expectedVersion) return GameResult<CaroGameView>.Failed(GameFailures.Conflict());
        if (game.Status != CaroGameStatus.Active) return GameResult<CaroGameView>.Failed(GameFailures.Finished());
        if (game.CurrentPlayerUserId != userId) return GameResult<CaroGameView>.Failed(GameFailures.NotYourTurn());
        if (game.Board[cell] != '-') return GameResult<CaroGameView>.Failed(GameFailures.CellOccupied());

        game.Play(userId, cell, timeProvider.GetUtcNow());
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return GameResult<CaroGameView>.Failed(GameFailures.Conflict());
        }
        return GameResult<CaroGameView>.Success(await ToViewAsync(game, cancellationToken));
    }

    private async Task<CaroGameView> ToViewAsync(CaroGame game, CancellationToken cancellationToken) =>
        ToView(game, await ProfilesAsync([game], cancellationToken));

    private async Task<IReadOnlyDictionary<Guid, UserProfile>> ProfilesAsync(IEnumerable<CaroGame> games,
        CancellationToken cancellationToken)
    {
        var ids = games.SelectMany(game => new[] { game.XPlayerUserId, game.OPlayerUserId }).Distinct().ToArray();
        return ids.Length == 0 ? new Dictionary<Guid, UserProfile>() : await profileReader.GetByIdsAsync(ids, cancellationToken);
    }

    private static CaroGameView ToView(CaroGame game, IReadOnlyDictionary<Guid, UserProfile> profiles) => new(
        game.Id, game.GroupId, game.XPlayerUserId, Name(game.XPlayerUserId, profiles), game.OPlayerUserId,
        Name(game.OPlayerUserId, profiles), game.CurrentPlayerUserId, game.WinnerUserId, game.Board,
        game.Status.ToString(), game.Version, game.CreatedAtUtc, game.UpdatedAtUtc);

    private static string Name(Guid id, IReadOnlyDictionary<Guid, UserProfile> profiles) =>
        profiles.TryGetValue(id, out var profile) ? profile.DisplayName : "Thành viên";
}
