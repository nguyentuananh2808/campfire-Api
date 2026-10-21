namespace Campfire.Modules.Events.Application.Common;

public sealed record EventFailure(string Code, string Message, int StatusCode);
