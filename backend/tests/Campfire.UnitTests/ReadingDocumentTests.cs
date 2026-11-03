using Campfire.Modules.Learning.Domain.Reading;

namespace Campfire.UnitTests;

public sealed class ReadingDocumentTests
{
    [Fact]
    public void New_document_keeps_owner_content_counts_and_utc_time()
    {
        var userId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        var document = new ReadingDocument(userId, "  Hội thoại 1  ", "你好。", 2, now);

        Assert.NotEqual(Guid.Empty, document.Id);
        Assert.Equal(userId, document.UserId);
        Assert.Equal("Hội thoại 1", document.Title);
        Assert.Equal("你好。", document.Content);
        Assert.Equal(3, document.CharacterCount);
        Assert.Equal(2, document.ChineseCharacterCount);
        Assert.Equal(now, document.CreatedAtUtc);
    }

    [Fact]
    public void New_document_rejects_empty_or_oversized_content()
    {
        Assert.Throws<ArgumentException>(() =>
            new ReadingDocument(Guid.NewGuid(), "Bài đọc", "   ", 0, DateTimeOffset.UtcNow));
        Assert.Throws<ArgumentException>(() =>
            new ReadingDocument(Guid.NewGuid(), "Bài đọc", new string('你', 10_001), 10_001, DateTimeOffset.UtcNow));
    }
}
