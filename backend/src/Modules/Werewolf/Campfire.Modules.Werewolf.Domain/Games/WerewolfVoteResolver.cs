namespace Campfire.Modules.Werewolf.Domain.Games;

public static class WerewolfVoteResolver
{
    public static Guid? Resolve(IEnumerable<GameVote> votes)
    {
        var ranked = votes
            .Where(vote => vote.TargetUserId.HasValue)
            .GroupBy(vote => vote.TargetUserId!.Value)
            .Select(group => new { Target = group.Key, Count = group.Count() })
            .OrderByDescending(result => result.Count)
            .ThenBy(result => result.Target)
            .ToArray();

        return ranked.Length > 0 && (ranked.Length == 1 || ranked[0].Count > ranked[1].Count)
            ? ranked[0].Target
            : null;
    }
}
