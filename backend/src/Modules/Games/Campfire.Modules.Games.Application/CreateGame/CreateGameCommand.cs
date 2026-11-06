using Campfire.Modules.Games.Application.Common;
using Campfire.Modules.Games.Application.Models;
using MediatR;

namespace Campfire.Modules.Games.Application.CreateGame;

public sealed record CreateGameCommand(Guid GroupId, Guid UserId, Guid OpponentUserId) : IRequest<GameResult<CaroGameView>>;
