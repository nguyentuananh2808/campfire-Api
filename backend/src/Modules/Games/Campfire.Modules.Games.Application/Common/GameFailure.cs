namespace Campfire.Modules.Games.Application.Common;

public sealed record GameFailure(string Code, string Message, int StatusCode);
