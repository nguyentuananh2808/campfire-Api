namespace Campfire.Modules.Chat.Domain.Messages;

public static class ChatReactionCatalog
{
    private static readonly HashSet<string> Values = new(StringComparer.Ordinal)
    {
        "Like", "Love", "Laugh", "Wow", "Sad"
    };

    public static bool Contains(string? value) => value is not null && Values.Contains(value);
}
