using Campfire.Modules.Chat.Contracts;

namespace Campfire.Modules.Chat.Application.Models;

public sealed record ChatMessagePage(
    IReadOnlyList<ChatMessageDto> Items,
    string? OlderCursor,
    string? LatestCursor,
    bool HasMore);
