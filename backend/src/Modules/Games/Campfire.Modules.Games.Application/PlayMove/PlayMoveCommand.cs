using Campfire.Modules.Games.Application.Common;
using Campfire.Modules.Games.Application.Models;
using MediatR;

namespace Campfire.Modules.Games.Application.PlayMove;

public sealed record PlayMoveCommand(Guid GameId, Guid UserId, int Cell, Guid ExpectedVersion) : IRequest<GameResult<CaroGameView>>;
