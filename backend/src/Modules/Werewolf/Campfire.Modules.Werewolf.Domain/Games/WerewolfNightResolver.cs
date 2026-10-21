namespace Campfire.Modules.Werewolf.Domain.Games;

public static class WerewolfNightResolver
{
    public static IReadOnlySet<Guid> Resolve(IEnumerable<GameAction> actions)
    {
        var materialized = actions.ToArray();
        var attack = materialized
            .Where(action => action.Kind == GameActionKind.WerewolfTarget && action.TargetUserId.HasValue)
            .GroupBy(action => action.TargetUserId!.Value)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key)
            .Select(group => (Guid?)group.Key)
            .FirstOrDefault();
        var protectedTarget = materialized
            .Where(action => action.Kind == GameActionKind.Protect)
            .OrderByDescending(action => action.CreatedAtUtc)
            .Select(action => action.TargetUserId)
            .FirstOrDefault();
        var healed = materialized.Any(action => action.Kind == GameActionKind.WitchHeal && action.TargetUserId == attack);
        var poison = materialized
            .Where(action => action.Kind == GameActionKind.WitchPoison)
            .OrderByDescending(action => action.CreatedAtUtc)
            .Select(action => action.TargetUserId)
            .FirstOrDefault();

        var eliminated = new HashSet<Guid>();
        if (attack.HasValue && attack != protectedTarget && !healed) eliminated.Add(attack.Value);
        if (poison.HasValue) eliminated.Add(poison.Value);
        return eliminated;
    }
}
