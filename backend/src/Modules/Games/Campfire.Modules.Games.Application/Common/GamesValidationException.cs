namespace Campfire.Modules.Games.Application.Common;

public sealed class GamesValidationException(IReadOnlyDictionary<string, string[]> errors)
    : Exception("Games request validation failed.")
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
}
