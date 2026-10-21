namespace Campfire.Modules.Chat.Domain.Messages;

public static class ChatStickerCatalog
{
    private static readonly HashSet<string> Codes = new(StringComparer.Ordinal)
    {
        "campfire",
        "laugh",
        "cheer",
        "heart",
        "thumbs-up",
        "sad"
    };

    public static bool Contains(string code) => Codes.Contains(code);
}
