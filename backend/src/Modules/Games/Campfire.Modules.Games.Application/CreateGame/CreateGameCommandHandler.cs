using Campfire.Modules.Games.Application.Abstractions;
using Campfire.Modules.Games.Application.Common;
using Campfire.Modules.Games.Application.Models;
using MediatR;

namespace Campfire.Modules.Games.Application.CreateGame;

public sealed class CreateGameCommandHandler(IGameService service) : IRequestHandler<CreateGameCommand, GameResult<CaroGameView>>
{
    public Task<GameResult<CaroGameView>> Handle(CreateGameCommand request, CancellationToken cancellationToken) =>
        service.CreateAsync(request.GroupId, request.UserId, request.OpponentUserId, cancellationToken);
}
