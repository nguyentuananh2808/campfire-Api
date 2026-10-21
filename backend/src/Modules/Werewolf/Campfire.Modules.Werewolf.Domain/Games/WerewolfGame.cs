namespace Campfire.Modules.Werewolf.Domain.Games;

public sealed class WerewolfGame
{
    private WerewolfGame() { }
    public WerewolfGame(Guid roomId, DateTimeOffset now, TimeSpan actionDuration)
    {
        Id = Guid.NewGuid(); RoomId = roomId; Phase = GamePhase.NightWerewolves; Round = 1;
        PhaseSequence = 1; PhaseEndsAtUtc = now.Add(actionDuration); Version = Guid.NewGuid(); StartedAtUtc = now;
    }
    public Guid Id { get; private set; }
    public Guid RoomId { get; private set; }
    public GamePhase Phase { get; private set; }
    public int Round { get; private set; }
    public int PhaseSequence { get; private set; }
    public DateTimeOffset PhaseEndsAtUtc { get; private set; }
    public Guid Version { get; private set; }
    public Winner? Winner { get; private set; }
    public DateTimeOffset StartedAtUtc { get; private set; }
    public DateTimeOffset? FinishedAtUtc { get; private set; }

    public bool TryAdvance(Guid expectedVersion, GamePhase next, DateTimeOffset now, TimeSpan duration)
    {
        if (Version != expectedVersion || Phase == GamePhase.Finished || next != NextPhase(Phase)) return false;
        Phase = next;
        if (next == GamePhase.NightWerewolves && PhaseSequence > 1) Round++;
        PhaseSequence++;
        PhaseEndsAtUtc = now.Add(duration);
        Version = Guid.NewGuid();
        return true;
    }

    public void Finish(Winner winner, DateTimeOffset now)
    {
        Winner = winner; Phase = GamePhase.Finished; FinishedAtUtc = now; PhaseEndsAtUtc = now;
        PhaseSequence++; Version = Guid.NewGuid();
    }

    private static GamePhase NextPhase(GamePhase phase) => phase switch
    {
        GamePhase.NightWerewolves => GamePhase.NightSeer,
        GamePhase.NightSeer => GamePhase.NightGuard,
        GamePhase.NightGuard => GamePhase.NightWitch,
        GamePhase.NightWitch => GamePhase.DayDiscussion,
        GamePhase.DayDiscussion => GamePhase.DayVoting,
        GamePhase.DayVoting => GamePhase.NightWerewolves,
        _ => GamePhase.Finished
    };
}
