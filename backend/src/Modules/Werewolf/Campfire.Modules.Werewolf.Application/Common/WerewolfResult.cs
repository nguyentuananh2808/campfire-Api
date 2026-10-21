namespace Campfire.Modules.Werewolf.Application.Common;

public sealed class WerewolfResult<T>
{
    private WerewolfResult(T? value, WerewolfFailure? failure) { Value = value; Failure = failure; }
    public T? Value { get; }
    public WerewolfFailure? Failure { get; }
    public bool IsSuccess => Failure is null;
    public static WerewolfResult<T> Success(T value) => new(value, null);
    public static WerewolfResult<T> Failed(WerewolfFailure failure) => new(default, failure);
}
