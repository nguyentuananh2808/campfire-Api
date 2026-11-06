using Campfire.Modules.Chat.Domain.ReadStates;

namespace Campfire.UnitTests;

public sealed class ChatReadStateTests
{
    [Fact]
    public void MarkRead_only_advances_the_cursor()
    {
        var first = DateTimeOffset.Parse("2026-11-06T08:00:00Z");
        var state = ChatReadState.Create(Guid.NewGuid(), Guid.NewGuid(), first);

        state.MarkRead(first.AddMinutes(-1));
        Assert.Equal(first, state.LastReadAtUtc);

        var latest = first.AddMinutes(1);
        state.MarkRead(latest);
        Assert.Equal(latest, state.LastReadAtUtc);
    }
}
