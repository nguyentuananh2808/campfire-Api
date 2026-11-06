using Campfire.Modules.Games.Application.Common;
using Campfire.Modules.Games.Application.Models;
using MediatR;

namespace Campfire.Modules.Games.Application.ListGames;

public sealed record ListGamesQuery(Guid UserId) : IRequest<GameResult<IReadOnlyList<CaroGameView>>>;
