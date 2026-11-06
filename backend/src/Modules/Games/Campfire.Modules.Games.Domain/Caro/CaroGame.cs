namespace Campfire.Modules.Games.Domain.Caro;

public sealed class CaroGame
{
    private static readonly int[][] WinningLines =
    [
        [0, 1, 2], [3, 4, 5], [6, 7, 8],
        [0, 3, 6], [1, 4, 7], [2, 5, 8],
        [0, 4, 8], [2, 4, 6]
    ];

    private CaroGame() { }

    public Guid Id { get; private set; }
    public Guid GroupId { get; private set; }
    public Guid XPlayerUserId { get; private set; }
    public Guid OPlayerUserId { get; private set; }
    public Guid CurrentPlayerUserId { get; private set; }
    public Guid? WinnerUserId { get; private set; }
    public string Board { get; private set; } = "---------";
    public CaroGameStatus Status { get; private set; }
    public Guid Version { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static CaroGame Create(Guid groupId, Guid creatorUserId, Guid opponentUserId, DateTimeOffset now)
    {
        if (groupId == Guid.Empty || creatorUserId == Guid.Empty || opponentUserId == Guid.Empty)
            throw new ArgumentException("Game identifiers are required.");
        if (creatorUserId == opponentUserId)
            throw new ArgumentException("A game requires two different players.");

        return new CaroGame
        {
            Id = Guid.NewGuid(),
            GroupId = groupId,
            XPlayerUserId = creatorUserId,
            OPlayerUserId = opponentUserId,
            CurrentPlayerUserId = creatorUserId,
            Status = CaroGameStatus.Active,
            Version = Guid.NewGuid(),
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
    }

    public void Play(Guid playerUserId, int cell, DateTimeOffset now)
    {
        if (Status != CaroGameStatus.Active) throw new InvalidOperationException("Game is finished.");
        if (playerUserId != CurrentPlayerUserId) throw new InvalidOperationException("It is not this player's turn.");
        if (cell is < 0 or > 8) throw new ArgumentOutOfRangeException(nameof(cell));
        if (Board[cell] != '-') throw new InvalidOperationException("Cell is occupied.");

        var mark = playerUserId == XPlayerUserId ? 'X' : 'O';
        var cells = Board.ToCharArray();
        cells[cell] = mark;
        Board = new string(cells);
        UpdatedAtUtc = now;
        Version = Guid.NewGuid();

        if (WinningLines.Any(line => line.All(index => Board[index] == mark)))
        {
            Status = CaroGameStatus.Won;
            WinnerUserId = playerUserId;
            return;
        }

        if (!Board.Contains('-'))
        {
            Status = CaroGameStatus.Draw;
            return;
        }

        CurrentPlayerUserId = playerUserId == XPlayerUserId ? OPlayerUserId : XPlayerUserId;
    }
}
