using Campfire.Modules.Games.Application.Common;
using Campfire.Modules.Games.Application.Models;
using MediatR;

namespace Campfire.Modules.Games.Application.GetGame;

public sealed record GetGameQuery(Guid GameId, Guid UserId) : IRequest<GameResult<CaroGameView>>;
