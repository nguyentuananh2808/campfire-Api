namespace Campfire.Modules.Chat.Application.Common;

public sealed class ChatResult<T>
{
    private ChatResult(T? value, ChatFailure? failure) { Value = value; Failure = failure; }
    public T? Value { get; }
    public ChatFailure? Failure { get; }
    public bool IsSuccess => Failure is null;
    public static ChatResult<T> Success(T value) => new(value, null);
    public static ChatResult<T> Failed(ChatFailure failure) => new(default, failure);
}
