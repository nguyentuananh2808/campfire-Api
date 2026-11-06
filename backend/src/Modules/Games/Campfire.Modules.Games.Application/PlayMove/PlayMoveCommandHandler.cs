using Campfire.Modules.Games.Application.Abstractions;
using Campfire.Modules.Games.Application.Common;
using Campfire.Modules.Games.Application.Models;
using MediatR;

namespace Campfire.Modules.Games.Application.PlayMove;

public sealed class PlayMoveCommandHandler(IGameService service) : IRequestHandler<PlayMoveCommand, GameResult<CaroGameView>>
{
    public Task<GameResult<CaroGameView>> Handle(PlayMoveCommand request, CancellationToken cancellationToken) =>
        service.PlayAsync(request.GameId, request.UserId, request.Cell, request.ExpectedVersion, cancellationToken);
}
