namespace Campfire.Modules.Werewolf.Domain.Rooms;

public sealed class WerewolfRoom
{
    private WerewolfRoom() { }

    private WerewolfRoom(Guid hostUserId, string code, string name, RoomVisibility visibility,
        int minimumPlayers, int maximumPlayers, bool deadVoiceEnabled, DateTimeOffset now)
    {
        Id = Guid.NewGuid();
        HostUserId = hostUserId;
        Code = ValidateCode(code);
        SetSettings(name, visibility, minimumPlayers, maximumPlayers, deadVoiceEnabled);
        Status = RoomStatus.Lobby;
        Version = Guid.NewGuid();
        CreatedAtUtc = now;
        UpdatedAtUtc = now;
    }

    public Guid Id { get; private set; }
    public Guid HostUserId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public RoomVisibility Visibility { get; private set; }
    public RoomStatus Status { get; private set; }
    public int MinimumPlayers { get; private set; }
    public int MaximumPlayers { get; private set; }
    public bool DeadVoiceEnabled { get; private set; }
    public Guid Version { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static WerewolfRoom Create(Guid hostUserId, string code, string name, RoomVisibility visibility,
        int minimumPlayers, int maximumPlayers, bool deadVoiceEnabled, DateTimeOffset now) =>
        new(hostUserId, code, name, visibility, minimumPlayers, maximumPlayers, deadVoiceEnabled, now);

    public bool TryUpdate(Guid expectedVersion, string name, RoomVisibility visibility, int minimumPlayers,
        int maximumPlayers, bool deadVoiceEnabled, DateTimeOffset now)
    {
        if (Status != RoomStatus.Lobby || Version != expectedVersion) return false;
        SetSettings(name, visibility, minimumPlayers, maximumPlayers, deadVoiceEnabled);
        Touch(now);
        return true;
    }

    public bool TryStart(Guid expectedVersion, DateTimeOffset now)
    {
        if (Status != RoomStatus.Lobby || Version != expectedVersion) return false;
        Status = RoomStatus.InProgress;
        Touch(now);
        return true;
    }

    public void Finish(DateTimeOffset now) { Status = RoomStatus.Finished; Touch(now); }
    public void TransferHost(Guid userId, DateTimeOffset now) { HostUserId = userId; Touch(now); }

    private void Touch(DateTimeOffset now) { Version = Guid.NewGuid(); UpdatedAtUtc = now; }

    private void SetSettings(string name, RoomVisibility visibility, int minimumPlayers, int maximumPlayers, bool deadVoiceEnabled)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (string.IsNullOrWhiteSpace(name) || name.Length > 80) throw new ArgumentException(nameof(name));
        if (minimumPlayers is < 6 or > 20 || maximumPlayers is < 6 or > 20 || minimumPlayers > maximumPlayers)
            throw new ArgumentOutOfRangeException(nameof(minimumPlayers));
        Name = name.Trim();
        Visibility = visibility;
        MinimumPlayers = minimumPlayers;
        MaximumPlayers = maximumPlayers;
        DeadVoiceEnabled = deadVoiceEnabled;
    }

    private static string ValidateCode(string code)
    {
        if (code.Length != 8 || code.Any(character => !char.IsAsciiLetterOrDigit(character)))
            throw new ArgumentException(nameof(code));
        return code.ToUpperInvariant();
    }
}
