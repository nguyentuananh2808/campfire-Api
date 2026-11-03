namespace Campfire.Modules.Learning.Domain.Reading;

public sealed class ReadingDocument
{
    private ReadingDocument() { }

    public ReadingDocument(
        Guid userId,
        string title,
        string content,
        int chineseCharacterCount,
        DateTimeOffset createdAtUtc)
    {
        if (userId == Guid.Empty) throw new ArgumentException("User ID is required.", nameof(userId));
        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length > 120)
            throw new ArgumentException("Title must contain between 1 and 120 characters.", nameof(title));
        if (string.IsNullOrWhiteSpace(content) || content.Length > 10_000)
            throw new ArgumentException("Content must contain between 1 and 10000 characters.", nameof(content));
        if (chineseCharacterCount < 0 || chineseCharacterCount > content.Length)
            throw new ArgumentOutOfRangeException(nameof(chineseCharacterCount));

        Id = Guid.NewGuid();
        UserId = userId;
        Title = title.Trim();
        Content = content;
        CharacterCount = content.Length;
        ChineseCharacterCount = chineseCharacterCount;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public int CharacterCount { get; private set; }
    public int ChineseCharacterCount { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
}
