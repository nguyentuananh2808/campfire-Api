namespace Campfire.Modules.Werewolf.Application.Common;

public sealed class RequestValidationException(IReadOnlyDictionary<string, string[]> errors) : Exception("Request validation failed.")
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
}
