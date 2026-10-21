using Campfire.Modules.Groups.Domain.Groups;
using Campfire.Modules.Groups.Domain.Invitations;

namespace Campfire.UnitTests;

public sealed class GroupDomainTests
{
    [Fact]
    public void Create_adds_exactly_one_owner_and_rename_changes_version()
    {
        var ownerId = Guid.NewGuid();
        var group = Group.Create("  Bạn thân  ", ownerId, DateTimeOffset.UtcNow);
        var originalVersion = group.Version;

        Assert.Equal("Bạn thân", group.Name);
        var owner = Assert.Single(group.Members);
        Assert.Equal(ownerId, owner.UserId);
        Assert.Equal(GroupRole.Owner, owner.Role);

        group.Rename("Lửa trại");

        Assert.Equal("Lửa trại", group.Name);
        Assert.NotEqual(originalVersion, group.Version);
    }

    [Fact]
    public void Invitation_requires_a_32_byte_hash_and_future_expiration()
    {
        var now = DateTimeOffset.UtcNow;

        Assert.Throws<ArgumentException>(() => GroupInvitation.Create(
            Guid.NewGuid(),
            new byte[16],
            Guid.NewGuid(),
            now,
            now.AddHours(1)));
        Assert.Throws<ArgumentOutOfRangeException>(() => GroupInvitation.Create(
            Guid.NewGuid(),
            new byte[32],
            Guid.NewGuid(),
            now,
            now));

        var invitation = GroupInvitation.Create(
            Guid.NewGuid(),
            new byte[32],
            Guid.NewGuid(),
            now,
            now.AddHours(1));
        Assert.True(invitation.IsAvailable(now));
        Assert.False(invitation.IsAvailable(now.AddHours(1)));
        Assert.True(invitation.Revoke(now.AddMinutes(1)));
        Assert.False(invitation.IsAvailable(now.AddMinutes(2)));
        Assert.False(invitation.Revoke(now.AddMinutes(3)));
    }
}
