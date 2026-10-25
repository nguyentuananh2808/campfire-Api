namespace Campfire.Modules.StrangerChat.Domain.Entities;

public static class StrangerReactionCatalog
{
    private static readonly HashSet<string> Values = new(StringComparer.Ordinal)
    {
        "Like", "Love", "Laugh", "Wow", "Sad"
    };

    public static bool Contains(string? value) => value is not null && Values.Contains(value);
}
