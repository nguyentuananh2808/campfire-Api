using Campfire.Modules.Games.Application.Abstractions;
using Campfire.Modules.Games.Application.Common;
using Campfire.Modules.Games.Application.Models;
using MediatR;

namespace Campfire.Modules.Games.Application.GetGame;

public sealed class GetGameQueryHandler(IGameService service) : IRequestHandler<GetGameQuery, GameResult<CaroGameView>>
{
    public Task<GameResult<CaroGameView>> Handle(GetGameQuery request, CancellationToken cancellationToken) => service.GetAsync(request.GameId, request.UserId, cancellationToken);
}
