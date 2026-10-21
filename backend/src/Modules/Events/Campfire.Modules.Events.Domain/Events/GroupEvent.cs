namespace Campfire.Modules.Events.Domain.Events;

public sealed class GroupEvent
{
    private GroupEvent() { }

    private GroupEvent(Guid groupId, Guid creatorUserId, string title, string description,
        DateTimeOffset startsAtUtc, string location, DateTimeOffset now)
    {
        Id = Guid.NewGuid();
        GroupId = groupId;
        CreatorUserId = creatorUserId;
        SetDetails(title, description, startsAtUtc, location, now);
        Status = EventStatus.Open;
        Version = Guid.NewGuid();
        CreatedAtUtc = now;
        UpdatedAtUtc = now;
    }

    public Guid Id { get; private set; }
    public Guid GroupId { get; private set; }
    public Guid CreatorUserId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DateTimeOffset StartsAtUtc { get; private set; }
    public string Location { get; private set; } = string.Empty;
    public EventStatus Status { get; private set; }
    public Guid Version { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static GroupEvent Create(Guid groupId, Guid creatorUserId, string title, string description,
        DateTimeOffset startsAtUtc, string location, DateTimeOffset now) =>
        new(groupId, creatorUserId, title, description, startsAtUtc, location, now);

    public bool TryUpdate(Guid expectedVersion, string title, string description,
        DateTimeOffset startsAtUtc, string location, DateTimeOffset now)
    {
        if (Status == EventStatus.Cancelled || Version != expectedVersion) return false;
        SetDetails(title, description, startsAtUtc, location, now);
        Version = Guid.NewGuid();
        UpdatedAtUtc = now;
        return true;
    }

    public bool TryChangeStatus(Guid expectedVersion, EventStatus target, DateTimeOffset now)
    {
        if (Version != expectedVersion || !CanTransition(Status, target)) return false;
        Status = target;
        Version = Guid.NewGuid();
        UpdatedAtUtc = now;
        return true;
    }

    public static bool CanTransition(EventStatus current, EventStatus target) =>
        (current == EventStatus.Open && target is EventStatus.Confirmed or EventStatus.Cancelled) ||
        (current == EventStatus.Confirmed && target == EventStatus.Cancelled);

    private void SetDetails(string title, string description, DateTimeOffset startsAtUtc, string location, DateTimeOffset now)
    {
        Title = ValidateRequired(title, 120, nameof(title));
        Description = ValidateOptional(description, 2000, nameof(description));
        Location = ValidateRequired(location, 200, nameof(location));
        if (startsAtUtc <= now) throw new ArgumentException("Event start time must be in the future.", nameof(startsAtUtc));
        StartsAtUtc = startsAtUtc;
    }

    private static string ValidateRequired(string value, int maximumLength, string parameter)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (string.IsNullOrWhiteSpace(value) || value.Length > maximumLength) throw new ArgumentException(parameter, parameter);
        return value.Trim();
    }

    private static string ValidateOptional(string value, int maximumLength, string parameter)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value.Length > maximumLength) throw new ArgumentException(parameter, parameter);
        return value.Trim();
    }
}
