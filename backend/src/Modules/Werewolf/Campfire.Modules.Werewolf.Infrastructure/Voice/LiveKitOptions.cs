namespace Campfire.Modules.Werewolf.Infrastructure.Voice;

public sealed class LiveKitOptions
{
    public const string SectionName = "LiveKit";
    public string PublicUrl { get; init; } = string.Empty;
    public string ApiUrl { get; init; } = string.Empty;
    public string ApiKey { get; init; } = string.Empty;
    public string ApiSecret { get; init; } = string.Empty;
    public int TokenLifetimeSeconds { get; init; } = 75;
}
