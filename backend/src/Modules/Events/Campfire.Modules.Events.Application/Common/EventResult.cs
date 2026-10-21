namespace Campfire.Modules.Events.Application.Common;

public sealed class EventResult<T>
{
    private EventResult(T? value, EventFailure? failure) { Value = value; Failure = failure; }
    public T? Value { get; }
    public EventFailure? Failure { get; }
    public bool IsSuccess => Failure is null;
    public static EventResult<T> Success(T value) => new(value, null);
    public static EventResult<T> Failed(EventFailure failure) => new(default, failure);
}
