namespace Campfire.Modules.Games.Presentation.Models;

public sealed record CreateGameRequest(Guid GroupId, Guid OpponentUserId);
