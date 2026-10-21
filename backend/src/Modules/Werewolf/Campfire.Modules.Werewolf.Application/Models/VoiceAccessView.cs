namespace Campfire.Modules.Werewolf.Application.Models;

public sealed record VoiceAccessView(string ServerUrl, string Token, string Channel, DateTimeOffset ExpiresAtUtc,
    bool CanPublish, bool CanSubscribe);
