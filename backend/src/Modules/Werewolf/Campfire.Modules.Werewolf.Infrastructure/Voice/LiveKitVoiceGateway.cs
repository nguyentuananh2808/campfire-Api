using Campfire.Modules.Werewolf.Application.Abstractions;
using Campfire.Modules.Werewolf.Application.Models;
using Livekit.Server.Sdk.Dotnet;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Campfire.Modules.Werewolf.Infrastructure.Voice;

public sealed class LiveKitVoiceGateway(IOptions<LiveKitOptions> options, ILogger<LiveKitVoiceGateway> logger) : IWerewolfVoiceGateway
{
    private readonly LiveKitOptions configuration = options.Value;
    public bool IsConfigured => Uri.TryCreate(configuration.PublicUrl, UriKind.Absolute, out _) &&
        Uri.TryCreate(configuration.ApiUrl, UriKind.Absolute, out _) &&
        !string.IsNullOrWhiteSpace(configuration.ApiKey) && !string.IsNullOrWhiteSpace(configuration.ApiSecret);

    public VoiceAccessView IssueAccess(string roomName, Guid userId, string displayName, string channel,
        bool canPublish, bool canSubscribe, DateTimeOffset now)
    {
        if (!IsConfigured) throw new InvalidOperationException("LiveKit is not configured.");
        var lifetime = TimeSpan.FromSeconds(Math.Clamp(configuration.TokenLifetimeSeconds, 30, 300));
        var token = new AccessToken(configuration.ApiKey, configuration.ApiSecret)
            .WithIdentity(userId.ToString("N"))
            .WithName(displayName)
            .WithAttributes(new Dictionary<string, string> { ["campfire.channel"] = channel })
            .WithGrants(new VideoGrants
            {
                RoomJoin = true,
                Room = roomName,
                CanPublish = canPublish,
                CanSubscribe = canSubscribe,
                CanPublishData = false,
                CanPublishSources = canPublish ? ["microphone"] : []
            }).WithTtl(lifetime).ToJwt();
        return new VoiceAccessView(configuration.PublicUrl, token, channel, now.Add(lifetime), canPublish, canSubscribe);
    }

    public async Task ClosePhaseRoomsAsync(Guid roomId, Guid gameId, int phaseSequence, CancellationToken cancellationToken)
    {
        if (!IsConfigured) return;
        var client = new RoomServiceClient(configuration.ApiUrl, configuration.ApiKey, configuration.ApiSecret);
        foreach (var channel in new[] { "public", "wolves", "dead" })
        {
            try
            {
                await client.DeleteRoom(new DeleteRoomRequest
                { Room = Services.EfWerewolfService.VoiceRoomName(roomId, gameId, phaseSequence, channel) });
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "Could not close LiveKit room for Werewolf room {RoomId} phase {PhaseSequence}.", roomId, phaseSequence);
            }
        }
    }
}
