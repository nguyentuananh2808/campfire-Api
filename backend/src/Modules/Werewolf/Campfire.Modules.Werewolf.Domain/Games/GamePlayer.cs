namespace Campfire.Modules.Werewolf.Domain.Games;

public sealed class GamePlayer
{
    private GamePlayer() { }
    public GamePlayer(Guid gameId, Guid userId, WerewolfRole role, int seat)
    { GameId = gameId; UserId = userId; Role = role; Seat = seat; IsAlive = true; }
    public Guid GameId { get; private set; }
    public Guid UserId { get; private set; }
    public WerewolfRole Role { get; private set; }
    public int Seat { get; private set; }
    public bool IsAlive { get; private set; }
    public bool HasHealPotion { get; private set; } = true;
    public bool HasPoisonPotion { get; private set; } = true;
    public Guid? LastProtectedUserId { get; private set; }
    public void Eliminate() => IsAlive = false;
    public void UseHeal() => HasHealPotion = false;
    public void UsePoison() => HasPoisonPotion = false;
    public void RememberProtection(Guid targetUserId) => LastProtectedUserId = targetUserId;
}
