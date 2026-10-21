namespace Campfire.Modules.Identity.Application.Common;

public sealed class IdentityResult<T>
{
    private IdentityResult(T? value, IReadOnlyCollection<IdentityFailure> failures)
    {
        Value = value;
        Failures = failures;
    }

    public bool IsSuccess => Failures.Count == 0;

    public T? Value { get; }

    public IReadOnlyCollection<IdentityFailure> Failures { get; }

    public static IdentityResult<T> Success(T value) => new(value, []);

    public static IdentityResult<T> Failed(params IdentityFailure[] failures) => new(default, failures);
}
