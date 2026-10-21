namespace Campfire.Modules.StrangerChat.Application.Common;
public sealed class StrangerResult<T>
{
    private StrangerResult(T? value, StrangerFailure? failure) { Value = value; Failure = failure; }
    public T? Value { get; }
    public StrangerFailure? Failure { get; }
    public bool IsSuccess => Failure is null;
    public static StrangerResult<T> Success(T value) => new(value, null);
    public static StrangerResult<T> Failed(string code, string message, int status) => new(default, new(code, message, status));
}
