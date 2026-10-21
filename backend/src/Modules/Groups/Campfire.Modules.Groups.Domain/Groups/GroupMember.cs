namespace Campfire.Modules.Groups.Domain.Groups;

public sealed class GroupMember
{
    private GroupMember()
    {
    }

    private GroupMember(Guid groupId, Guid userId, GroupRole role, DateTimeOffset joinedAtUtc)
    {
        GroupId = groupId;
        UserId = userId;
        Role = role;
        JoinedAtUtc = joinedAtUtc;
    }

    public Guid GroupId { get; private set; }

    public Guid UserId { get; private set; }

    public GroupRole Role { get; private set; }

    public DateTimeOffset JoinedAtUtc { get; private set; }

    public static GroupMember CreateOwner(Guid groupId, Guid userId, DateTimeOffset joinedAtUtc) =>
        new(groupId, userId, GroupRole.Owner, joinedAtUtc);

    public static GroupMember CreateMember(Guid groupId, Guid userId, DateTimeOffset joinedAtUtc) =>
        new(groupId, userId, GroupRole.Member, joinedAtUtc);

    public void PromoteToOwner() => Role = GroupRole.Owner;

    public void DemoteToMember() => Role = GroupRole.Member;
}
