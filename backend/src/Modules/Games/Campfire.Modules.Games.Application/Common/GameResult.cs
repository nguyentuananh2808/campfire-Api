namespace Campfire.Modules.Games.Application.Common;

public sealed class GameResult<T>
{
    private GameResult(T? value, GameFailure? failure) { Value = value; Failure = failure; }
    public T? Value { get; }
    public GameFailure? Failure { get; }
    public bool IsSuccess => Failure is null;
    public static GameResult<T> Success(T value) => new(value, null);
    public static GameResult<T> Failed(GameFailure failure) => new(default, failure);
}
