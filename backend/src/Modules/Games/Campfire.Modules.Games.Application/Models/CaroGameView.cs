namespace Campfire.Modules.Games.Application.Models;

public sealed record CaroGameView(
    Guid Id,
    Guid GroupId,
    Guid XPlayerUserId,
    string XPlayerDisplayName,
    Guid OPlayerUserId,
    string OPlayerDisplayName,
    Guid CurrentPlayerUserId,
    Guid? WinnerUserId,
    string Board,
    string Status,
    Guid Version,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
