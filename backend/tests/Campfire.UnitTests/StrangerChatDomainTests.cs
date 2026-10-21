using Campfire.Modules.StrangerChat.Domain.Entities;

namespace Campfire.UnitTests;

public sealed class StrangerChatDomainTests
{
    [Fact]
    public void Guest_session_rejects_short_nickname() =>
        Assert.Throws<ArgumentException>(() => GuestSession.Create("hash", "x", GuestGender.Other, GenderPreference.Any, DateTimeOffset.UtcNow));

    [Fact]
    public void Guest_session_keeps_gender_matching_preferences()
    {
        var session = GuestSession.Create("hash", "Đốm Lửa", GuestGender.Female, GenderPreference.Male, DateTimeOffset.UtcNow);

        Assert.Equal(GuestGender.Female, session.Gender);
        Assert.Equal(GenderPreference.Male, session.GenderPreference);
    }

    [Fact]
    public void Conversation_cannot_match_a_session_with_itself()
    {
        var sessionId = Guid.NewGuid();
        Assert.Throws<ArgumentException>(() => StrangerConversation.Create(sessionId, sessionId, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Message_rejects_blank_and_oversized_content()
    {
        Assert.Throws<ArgumentException>(() => StrangerMessage.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "   ", DateTimeOffset.UtcNow));
        Assert.Throws<ArgumentException>(() => StrangerMessage.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new string('a', 1001), DateTimeOffset.UtcNow));
    }
}
