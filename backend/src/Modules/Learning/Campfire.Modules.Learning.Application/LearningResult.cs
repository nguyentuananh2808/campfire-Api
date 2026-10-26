namespace Campfire.Modules.Learning.Application;

public sealed record LearningResult<T>(T? Value, string? ErrorCode = null, string? Error = null)
{
    public bool IsSuccess => ErrorCode is null;
    public static LearningResult<T> Success(T value) => new(value);
    public static LearningResult<T> Failure(string code, string error) => new(default, code, error);
}
