using Campfire.Modules.Werewolf.Application.Abstractions;
using Microsoft.AspNetCore.SignalR;

namespace Campfire.Modules.Werewolf.Infrastructure.Realtime;

public sealed class SignalRWerewolfNotificationPublisher(IHubContext<WerewolfHub> hub) : IWerewolfNotificationPublisher
{
    public Task RoomChangedAsync(Guid roomId, Guid version, CancellationToken cancellationToken) =>
        hub.Clients.Group(WerewolfHub.Group(roomId)).SendAsync("RoomChanged", new { roomId, version }, cancellationToken);
    public Task VoicePermissionsChangedAsync(Guid roomId, CancellationToken cancellationToken) =>
        hub.Clients.Group(WerewolfHub.Group(roomId)).SendAsync("VoicePermissionsChanged", new { roomId }, cancellationToken);
}
