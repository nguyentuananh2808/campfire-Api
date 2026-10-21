namespace Campfire.Modules.StrangerChat.Application.Models;

public sealed record GuestSessionModel(
    string Nickname,
    string Gender,
    string GenderPreference,
    DateTimeOffset ExpiresAtUtc);
