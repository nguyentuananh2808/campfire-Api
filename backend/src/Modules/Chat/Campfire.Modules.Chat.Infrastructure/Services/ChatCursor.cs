namespace Campfire.Modules.Chat.Infrastructure.Services;

internal readonly record struct ChatCursor(DateTimeOffset SentAtUtc, Guid MessageId);
