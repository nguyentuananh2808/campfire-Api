using Campfire.Modules.Chat.Domain.Messages;

namespace Campfire.UnitTests;

public sealed class ChatMessageTests
{
    [Fact]
    public void Create_rejects_blank_and_over_limit_content()
    {
        Assert.Throws<ArgumentException>(() => ChatMessage.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "   ", DateTimeOffset.UtcNow));
        Assert.Throws<ArgumentException>(() => ChatMessage.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new string('x', 2001), DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Create_preserves_valid_plain_text()
    {
        const string content = "  <b>không phải HTML</b>  ";
        var message = ChatMessage.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), content, DateTimeOffset.UtcNow);
        Assert.Equal(ChatMessageKind.Text, message.Kind);
        Assert.Equal(content, message.Content);
    }

    [Fact]
    public void Create_accepts_only_allow_listed_stickers()
    {
        var message = ChatMessage.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            ChatMessageKind.Sticker,
            "campfire",
            DateTimeOffset.UtcNow);

        Assert.Equal(ChatMessageKind.Sticker, message.Kind);
        Assert.Equal("campfire", message.Content);
        Assert.Throws<ArgumentException>(() => ChatMessage.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            ChatMessageKind.Sticker,
            "https://untrusted.example/sticker.svg",
            DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Create_keeps_reply_target_and_reactions_use_allow_list()
    {
        var replyId = Guid.NewGuid();
        var message = ChatMessage.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), ChatMessageKind.Text,
            "Trả lời", replyId, DateTimeOffset.UtcNow);
        var reaction = ChatMessageReaction.Create(message.Id, Guid.NewGuid(), "Love", DateTimeOffset.UtcNow);

        Assert.Equal(replyId, message.ReplyToMessageId);
        Assert.Equal("Love", reaction.Reaction);
        Assert.Throws<ArgumentException>(() =>
            ChatMessageReaction.Create(message.Id, Guid.NewGuid(), "<script>", DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Edit_updates_text_timestamp_and_version_but_rejects_stickers()
    {
        var message = ChatMessage.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Ban đầu", DateTimeOffset.UtcNow);
        var originalVersion = message.Version;
        var editedAt = DateTimeOffset.UtcNow.AddMinutes(1);

        message.Edit("  Đã sửa  ", editedAt);

        Assert.Equal("Đã sửa", message.Content);
        Assert.Equal(editedAt, message.EditedAtUtc);
        Assert.NotEqual(originalVersion, message.Version);

        var sticker = ChatMessage.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), ChatMessageKind.Sticker, "campfire", DateTimeOffset.UtcNow);
        Assert.Throws<InvalidOperationException>(() => sticker.Edit("không hợp lệ", editedAt));
    }
}
