using Campfire.Modules.Werewolf.Domain.Roles;

namespace Campfire.Modules.Werewolf.Domain.Games;

public static class WerewolfWinEvaluator
{
    public static Winner? Evaluate(IEnumerable<GamePlayer> players)
    {
        var alive = players.Where(player => player.IsAlive).ToArray();
        var wolves = alive.Count(player => WerewolfRoleRegistry.FactionOf(player.Role) == RoleFaction.Werewolves);
        if (wolves == 0) return Winner.Village;
        return wolves >= alive.Length - wolves ? Winner.Werewolves : null;
    }
}
