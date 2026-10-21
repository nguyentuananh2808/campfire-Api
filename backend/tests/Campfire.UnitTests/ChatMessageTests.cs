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
}
