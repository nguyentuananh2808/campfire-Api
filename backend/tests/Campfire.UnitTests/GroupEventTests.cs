using Campfire.Modules.Events.Domain.Events;

namespace Campfire.UnitTests;

public sealed class GroupEventTests
{
    [Fact]
    public void Create_requires_future_time()
    {
        var now = new DateTimeOffset(2026, 10, 16, 0, 0, 0, TimeSpan.Zero);
        Assert.Throws<ArgumentException>(() => GroupEvent.Create(Guid.NewGuid(), Guid.NewGuid(),
            "Đi cắm trại", string.Empty, now, "Đà Lạt", now));
    }

    [Fact]
    public void Status_transitions_and_versions_are_enforced()
    {
        var now = new DateTimeOffset(2026, 10, 16, 0, 0, 0, TimeSpan.Zero);
        var item = GroupEvent.Create(Guid.NewGuid(), Guid.NewGuid(), "Đi cắm trại", string.Empty,
            now.AddDays(2), "Đà Lạt", now);
        var initialVersion = item.Version;

        Assert.True(item.TryChangeStatus(initialVersion, EventStatus.Confirmed, now.AddMinutes(1)));
        Assert.NotEqual(initialVersion, item.Version);
        Assert.False(item.TryChangeStatus(initialVersion, EventStatus.Cancelled, now.AddMinutes(2)));
        Assert.True(item.TryChangeStatus(item.Version, EventStatus.Cancelled, now.AddMinutes(2)));
        Assert.False(item.TryChangeStatus(item.Version, EventStatus.Open, now.AddMinutes(3)));
    }

    [Fact]
    public void Cancelled_event_cannot_be_edited()
    {
        var now = new DateTimeOffset(2026, 10, 16, 0, 0, 0, TimeSpan.Zero);
        var item = GroupEvent.Create(Guid.NewGuid(), Guid.NewGuid(), "Đi cắm trại", string.Empty,
            now.AddDays(2), "Đà Lạt", now);
        item.TryChangeStatus(item.Version, EventStatus.Cancelled, now.AddMinutes(1));
        Assert.False(item.TryUpdate(item.Version, "Tên mới", string.Empty, now.AddDays(3), "Huế", now.AddMinutes(2)));
    }
}
