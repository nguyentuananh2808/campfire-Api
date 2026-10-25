namespace Campfire.Modules.StrangerChat.Domain.Entities;

public static class StrangerStickerCatalog
{
    private static readonly HashSet<string> Values = new(StringComparer.Ordinal)
    {
        "campfire", "laugh", "cheer", "heart", "thumbs-up", "sad"
    };

    public static bool Contains(string? value) => value is not null && Values.Contains(value);
}
