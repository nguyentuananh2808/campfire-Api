using Campfire.Modules.Chat.Contracts;

namespace Campfire.Modules.Chat.Application.Models;

public sealed record SendMessageResult(ChatMessageDto Message, bool WasCreated);
