namespace Campfire.Modules.Chat.Application.Common;

public sealed record ChatFailure(string Code, string Message, int StatusCode);
