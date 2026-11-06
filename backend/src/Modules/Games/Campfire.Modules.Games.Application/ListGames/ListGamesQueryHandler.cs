using Campfire.Modules.Games.Application.Abstractions;
using Campfire.Modules.Games.Application.Common;
using Campfire.Modules.Games.Application.Models;
using MediatR;

namespace Campfire.Modules.Games.Application.ListGames;

public sealed class ListGamesQueryHandler(IGameService service) : IRequestHandler<ListGamesQuery, GameResult<IReadOnlyList<CaroGameView>>>
{
    public Task<GameResult<IReadOnlyList<CaroGameView>>> Handle(ListGamesQuery request, CancellationToken cancellationToken) => service.ListAsync(request.UserId, cancellationToken);
}
