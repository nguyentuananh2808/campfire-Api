namespace Campfire.Modules.Groups.Domain.Groups;

public sealed class Group
{
    private readonly List<GroupMember> members = [];

    private Group()
    {
    }

    private Group(Guid id, string name, Guid ownerUserId, DateTimeOffset createdAtUtc)
    {
        Id = id;
        Name = NormalizeName(name);
        CreatedAtUtc = createdAtUtc;
        Version = Guid.NewGuid();
        members.Add(GroupMember.CreateOwner(id, ownerUserId, createdAtUtc));
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public Guid Version { get; private set; }

    public IReadOnlyCollection<GroupMember> Members => members;

    public static Group Create(string name, Guid ownerUserId, DateTimeOffset createdAtUtc) =>
        new(Guid.NewGuid(), name, ownerUserId, createdAtUtc);

    public void Rename(string name)
    {
        Name = NormalizeName(name);
        Version = Guid.NewGuid();
    }

    public bool TryAdvanceVersion(Guid expectedVersion)
    {
        if (Version != expectedVersion)
        {
            return false;
        }

        Version = Guid.NewGuid();
        return true;
    }

    private static string NormalizeName(string name)
    {
        var normalized = name.Trim();
        if (normalized.Length is < 2 or > 80)
        {
            throw new ArgumentOutOfRangeException(nameof(name), "Group name must contain 2 to 80 characters.");
        }

        return normalized;
    }
}
