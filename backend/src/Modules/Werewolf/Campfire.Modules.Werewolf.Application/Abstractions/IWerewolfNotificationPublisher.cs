namespace Campfire.Modules.Werewolf.Application.Abstractions;

public interface IWerewolfNotificationPublisher
{
    Task RoomChangedAsync(Guid roomId, Guid version, CancellationToken cancellationToken);
    Task VoicePermissionsChangedAsync(Guid roomId, CancellationToken cancellationToken);
}
