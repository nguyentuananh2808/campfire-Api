using Campfire.Modules.Games.Application.Common;
using Campfire.Modules.Games.Application.Models;

namespace Campfire.Modules.Games.Application.Abstractions;

public interface IGameService
{
    Task<GameResult<CaroGameView>> CreateAsync(Guid groupId, Guid userId, Guid opponentUserId, CancellationToken cancellationToken);
    Task<GameResult<IReadOnlyList<CaroGameView>>> ListAsync(Guid userId, CancellationToken cancellationToken);
    Task<GameResult<CaroGameView>> GetAsync(Guid gameId, Guid userId, CancellationToken cancellationToken);
    Task<GameResult<CaroGameView>> PlayAsync(Guid gameId, Guid userId, int cell, Guid expectedVersion, CancellationToken cancellationToken);
}
