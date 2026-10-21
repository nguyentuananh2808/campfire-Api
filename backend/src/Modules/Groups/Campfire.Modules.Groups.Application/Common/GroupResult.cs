namespace Campfire.Modules.Groups.Application.Common;

public sealed class GroupResult<T>
{
    private GroupResult(T? value, GroupFailure? failure)
    {
        Value = value;
        Failure = failure;
    }

    public T? Value { get; }

    public GroupFailure? Failure { get; }

    public bool IsSuccess => Failure is null;

    public static GroupResult<T> Success(T value) => new(value, null);

    public static GroupResult<T> Failed(GroupFailure failure) => new(default, failure);
}
