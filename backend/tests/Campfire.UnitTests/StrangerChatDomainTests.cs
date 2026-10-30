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

    [Fact]
    public void Message_accepts_allow_listed_sticker_reply_and_rejects_external_asset()
    {
        var replyId = Guid.NewGuid();
        var message = StrangerMessage.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), StrangerMessageKind.Sticker,
            "campfire", replyId, DateTimeOffset.UtcNow);

        Assert.Equal(StrangerMessageKind.Sticker, message.Kind);
        Assert.Equal(replyId, message.ReplyToMessageId);
        Assert.Throws<ArgumentException>(() => StrangerMessage.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), StrangerMessageKind.Sticker,
            "https://untrusted.example/sticker.svg", null, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Reaction_accepts_only_basic_allow_list()
    {
        var reaction = StrangerMessageReaction.Create(Guid.NewGuid(), Guid.NewGuid(), "Laugh", DateTimeOffset.UtcNow);
        Assert.Equal("Laugh", reaction.Reaction);
        Assert.Throws<ArgumentException>(() =>
            StrangerMessageReaction.Create(Guid.NewGuid(), Guid.NewGuid(), "Custom", DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Message_edit_updates_text_and_rejects_stickers()
    {
        var message = StrangerMessage.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Ban đầu", DateTimeOffset.UtcNow);
        var version = message.Version;
        var editedAt = DateTimeOffset.UtcNow.AddMinutes(1);

        message.Edit("  Đã sửa  ", editedAt);

        Assert.Equal("Đã sửa", message.Content);
        Assert.Equal(editedAt, message.EditedAtUtc);
        Assert.NotEqual(version, message.Version);

        var sticker = StrangerMessage.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), StrangerMessageKind.Sticker, "campfire", null, DateTimeOffset.UtcNow);
        Assert.Throws<InvalidOperationException>(() => sticker.Edit("không hợp lệ", editedAt));
    }
}
