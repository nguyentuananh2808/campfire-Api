using Campfire.Modules.Werewolf.Domain.Games;

namespace Campfire.Modules.Werewolf.Domain.Roles;

public sealed record RoleDefinition(WerewolfRole Id, string Name, RoleFaction Faction, string Icon,
    string Description, GamePhase? WakePhase, int? WakeOrder, string Ability, string WinCondition);
