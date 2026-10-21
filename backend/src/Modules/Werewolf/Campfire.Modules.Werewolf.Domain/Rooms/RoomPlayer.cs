namespace Campfire.Modules.Werewolf.Domain.Rooms;

public sealed class RoomPlayer
{
    private RoomPlayer() { }
    public RoomPlayer(Guid roomId, Guid userId, PlayerKind kind, DateTimeOffset joinedAtUtc)
    {
        RoomId = roomId; UserId = userId; Kind = kind; JoinedAtUtc = joinedAtUtc;
    }
    public Guid RoomId { get; private set; }
    public Guid UserId { get; private set; }
    public PlayerKind Kind { get; private set; }
    public bool IsReady { get; private set; }
    public bool IsConnected { get; private set; }
    public bool IsHostMuted { get; private set; }
    public DateTimeOffset JoinedAtUtc { get; private set; }
    public DateTimeOffset? LastSeenAtUtc { get; private set; }
    public void SetReady(bool ready) => IsReady = ready;
    public void SetConnected(bool connected, DateTimeOffset now) { IsConnected = connected; LastSeenAtUtc = now; }
    public void SetHostMuted(bool muted) => IsHostMuted = muted;
}
