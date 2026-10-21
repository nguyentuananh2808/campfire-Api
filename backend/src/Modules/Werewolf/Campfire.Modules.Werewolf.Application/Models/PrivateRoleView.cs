namespace Campfire.Modules.Werewolf.Application.Models;

public sealed record PrivateRoleView(string Id, string Name, string Faction, string Icon, string Description,
    string Ability, string WinCondition, bool HasHealPotion, bool HasPoisonPotion,
    IReadOnlyList<Guid> WerewolfTeammateIds, string? InvestigationResult);
