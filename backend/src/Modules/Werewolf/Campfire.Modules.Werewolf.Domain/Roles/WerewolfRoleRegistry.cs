using Campfire.Modules.Werewolf.Domain.Games;

namespace Campfire.Modules.Werewolf.Domain.Roles;

public static class WerewolfRoleRegistry
{
    private static readonly IReadOnlyDictionary<WerewolfRole, RoleDefinition> Definitions =
        new Dictionary<WerewolfRole, RoleDefinition>
        {
            [WerewolfRole.Villager] = new(WerewolfRole.Villager, "Dân làng", RoleFaction.Village, "🏕️", "Tìm và loại Ma Sói bằng bỏ phiếu.", null, null, "Không có kỹ năng ban đêm.", "Loại toàn bộ Ma Sói."),
            [WerewolfRole.Werewolf] = new(WerewolfRole.Werewolf, "Ma Sói", RoleFaction.Werewolves, "🐺", "Ẩn mình và săn dân làng.", GamePhase.NightWerewolves, 10, "Cùng Sói chọn một mục tiêu mỗi đêm.", "Số Sói còn sống bằng hoặc nhiều hơn phe còn lại."),
            [WerewolfRole.Seer] = new(WerewolfRole.Seer, "Tiên tri", RoleFaction.Village, "🔮", "Tìm Ma Sói trong bí mật.", GamePhase.NightSeer, 20, "Soi phe của một người mỗi đêm.", "Loại toàn bộ Ma Sói."),
            [WerewolfRole.Guard] = new(WerewolfRole.Guard, "Bảo vệ", RoleFaction.Village, "🛡️", "Bảo vệ dân làng.", GamePhase.NightGuard, 30, "Bảo vệ một người, không cùng người hai đêm liên tiếp.", "Loại toàn bộ Ma Sói."),
            [WerewolfRole.Witch] = new(WerewolfRole.Witch, "Phù thủy", RoleFaction.Village, "🧪", "Có một bình cứu và một bình độc.", GamePhase.NightWitch, 40, "Mỗi bình chỉ dùng một lần; mỗi đêm chọn tối đa một hành động.", "Loại toàn bộ Ma Sói.")
        };
    public static RoleDefinition Get(WerewolfRole role) => Definitions[role];
    public static RoleFaction FactionOf(WerewolfRole role) => Definitions[role].Faction;
    public static IReadOnlyList<WerewolfRole> BuildDeck(int players)
    {
        if (players is < 6 or > 20) throw new ArgumentOutOfRangeException(nameof(players));
        var wolves = players >= 12 ? 3 : 2;
        var roles = new List<WerewolfRole> { WerewolfRole.Seer, WerewolfRole.Guard, WerewolfRole.Witch };
        roles.AddRange(Enumerable.Repeat(WerewolfRole.Werewolf, wolves));
        roles.AddRange(Enumerable.Repeat(WerewolfRole.Villager, players - roles.Count));
        return roles;
    }
}
