namespace Campfire.Modules.Games.Presentation.Models;

public sealed record PlayMoveRequest(int Cell, Guid ExpectedVersion);
