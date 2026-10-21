using Campfire.Modules.Chat.Application.Common;
using Campfire.Modules.Chat.Application.Models;
using MediatR;

namespace Campfire.Modules.Chat.Application.GetMessages;

public sealed record GetMessagesQuery(Guid GroupId, Guid UserId, string? Before, string? After, int Limit)
    : IRequest<ChatResult<ChatMessagePage>>;
