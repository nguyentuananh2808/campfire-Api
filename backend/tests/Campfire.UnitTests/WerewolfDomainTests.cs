using Campfire.Modules.Werewolf.Domain.Games;
using Campfire.Modules.Werewolf.Domain.Roles;
using Campfire.Modules.Werewolf.Domain.Rooms;

namespace Campfire.UnitTests;

public sealed class WerewolfDomainTests
{
    [Fact]
    public void Basic_deck_is_complete_and_balanced_for_minimum_game()
    {
        var deck = WerewolfRoleRegistry.BuildDeck(6);
        Assert.Equal(6, deck.Count);
        Assert.Equal(2, deck.Count(role => role == WerewolfRole.Werewolf));
        Assert.Contains(WerewolfRole.Seer, deck);
        Assert.Contains(WerewolfRole.Guard, deck);
        Assert.Contains(WerewolfRole.Witch, deck);
    }

    [Fact]
    public void Village_wins_when_last_werewolf_is_eliminated()
    {
        var gameId = Guid.NewGuid();
        var wolf = new GamePlayer(gameId, Guid.NewGuid(), WerewolfRole.Werewolf, 1);
        wolf.Eliminate();
        var players = new[] { wolf, new GamePlayer(gameId, Guid.NewGuid(), WerewolfRole.Villager, 2) };
        Assert.Equal(Winner.Village, WerewolfWinEvaluator.Evaluate(players));
    }

    [Fact]
    public void Werewolves_win_at_parity()
    {
        var gameId = Guid.NewGuid();
        var players = new[]
        {
            new GamePlayer(gameId, Guid.NewGuid(), WerewolfRole.Werewolf, 1),
            new GamePlayer(gameId, Guid.NewGuid(), WerewolfRole.Villager, 2)
        };
        Assert.Equal(Winner.Werewolves, WerewolfWinEvaluator.Evaluate(players));
    }

    [Fact]
    public void Game_rejects_stale_phase_transition()
    {
        var now = DateTimeOffset.Parse("2026-10-16T00:00:00Z");
        var game = new WerewolfGame(Guid.NewGuid(), now, TimeSpan.FromSeconds(30));
        var staleVersion = Guid.NewGuid();
        Assert.False(game.TryAdvance(staleVersion, GamePhase.NightSeer, now, TimeSpan.FromSeconds(30)));
        Assert.Equal(GamePhase.NightWerewolves, game.Phase);
    }

    [Fact]
    public void Game_advances_in_the_basic_night_day_order_and_increments_round()
    {
        var now = DateTimeOffset.Parse("2026-10-16T00:00:00Z");
        var game = new WerewolfGame(Guid.NewGuid(), now, TimeSpan.FromSeconds(30));
        var phases = new[]
        {
            GamePhase.NightSeer,
            GamePhase.NightGuard,
            GamePhase.NightWitch,
            GamePhase.DayDiscussion,
            GamePhase.DayVoting,
            GamePhase.NightWerewolves
        };

        foreach (var phase in phases)
            Assert.True(game.TryAdvance(game.Version, phase, now, TimeSpan.FromSeconds(30)));

        Assert.Equal(GamePhase.NightWerewolves, game.Phase);
        Assert.Equal(2, game.Round);
        Assert.Equal(7, game.PhaseSequence);
    }

    [Fact]
    public void Game_rejects_an_out_of_order_phase_even_with_current_version()
    {
        var now = DateTimeOffset.Parse("2026-10-16T00:00:00Z");
        var game = new WerewolfGame(Guid.NewGuid(), now, TimeSpan.FromSeconds(30));

        Assert.False(game.TryAdvance(game.Version, GamePhase.DayDiscussion, now, TimeSpan.FromSeconds(30)));
        Assert.Equal(GamePhase.NightWerewolves, game.Phase);
        Assert.Equal(1, game.PhaseSequence);
    }

    [Fact]
    public void Night_resolution_combines_attack_protection_heal_and_poison_deterministically()
    {
        var gameId = Guid.NewGuid();
        var attacked = Guid.NewGuid();
        var poisoned = Guid.NewGuid();
        var now = DateTimeOffset.Parse("2026-10-16T00:00:00Z");
        var actions = new[]
        {
            new GameAction(gameId, 1, Guid.NewGuid(), Guid.NewGuid(), GameActionKind.WerewolfTarget, attacked, now),
            new GameAction(gameId, 1, Guid.NewGuid(), Guid.NewGuid(), GameActionKind.WerewolfTarget, attacked, now),
            new GameAction(gameId, 3, Guid.NewGuid(), Guid.NewGuid(), GameActionKind.Protect, attacked, now),
            new GameAction(gameId, 4, Guid.NewGuid(), Guid.NewGuid(), GameActionKind.WitchPoison, poisoned, now)
        };

        var eliminated = WerewolfNightResolver.Resolve(actions);

        Assert.DoesNotContain(attacked, eliminated);
        Assert.Contains(poisoned, eliminated);
    }

    [Fact]
    public void Witch_heal_prevents_the_selected_werewolf_attack()
    {
        var gameId = Guid.NewGuid();
        var attacked = Guid.NewGuid();
        var now = DateTimeOffset.Parse("2026-10-16T00:00:00Z");
        var actions = new[]
        {
            new GameAction(gameId, 1, Guid.NewGuid(), Guid.NewGuid(), GameActionKind.WerewolfTarget, attacked, now),
            new GameAction(gameId, 4, Guid.NewGuid(), Guid.NewGuid(), GameActionKind.WitchHeal, attacked, now)
        };

        Assert.Empty(WerewolfNightResolver.Resolve(actions));
    }

    [Fact]
    public void Vote_resolution_eliminates_the_unique_plurality_and_ignores_blank_votes()
    {
        var gameId = Guid.NewGuid();
        var target = Guid.NewGuid();
        var other = Guid.NewGuid();
        var now = DateTimeOffset.Parse("2026-10-16T00:00:00Z");
        var votes = new[]
        {
            new GameVote(gameId, 6, Guid.NewGuid(), Guid.NewGuid(), target, now),
            new GameVote(gameId, 6, Guid.NewGuid(), Guid.NewGuid(), target, now),
            new GameVote(gameId, 6, Guid.NewGuid(), Guid.NewGuid(), other, now),
            new GameVote(gameId, 6, Guid.NewGuid(), Guid.NewGuid(), null, now)
        };

        Assert.Equal(target, WerewolfVoteResolver.Resolve(votes));
    }

    [Fact]
    public void Vote_resolution_eliminates_nobody_when_the_lead_is_tied()
    {
        var gameId = Guid.NewGuid();
        var now = DateTimeOffset.Parse("2026-10-16T00:00:00Z");
        var votes = new[]
        {
            new GameVote(gameId, 6, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), now),
            new GameVote(gameId, 6, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), now)
        };

        Assert.Null(WerewolfVoteResolver.Resolve(votes));
    }

    [Fact]
    public void Every_basic_role_has_icon_ability_and_win_condition_metadata()
    {
        foreach (var role in Enum.GetValues<WerewolfRole>())
        {
            var definition = WerewolfRoleRegistry.Get(role);
            Assert.False(string.IsNullOrWhiteSpace(definition.Icon));
            Assert.False(string.IsNullOrWhiteSpace(definition.Ability));
            Assert.False(string.IsNullOrWhiteSpace(definition.WinCondition));
        }
    }

    [Fact]
    public void Transferring_host_rotates_the_concurrency_version()
    {
        var now = DateTimeOffset.Parse("2026-10-16T00:00:00Z");
        var room = WerewolfRoom.Create(Guid.NewGuid(), "ABCDEF12", "Phòng", RoomVisibility.Private, 6, 20, true, now);
        var originalVersion = room.Version;
        var nextHost = Guid.NewGuid();

        room.TransferHost(nextHost, now.AddSeconds(1));

        Assert.Equal(nextHost, room.HostUserId);
        Assert.NotEqual(originalVersion, room.Version);
    }

    [Fact]
    public void Room_settings_require_supported_player_range()
    {
        var now = DateTimeOffset.Parse("2026-10-16T00:00:00Z");
        Assert.Throws<ArgumentOutOfRangeException>(() => WerewolfRoom.Create(Guid.NewGuid(), "ABCDEF12", "Phòng", RoomVisibility.Private, 5, 20, true, now));
    }
}
