using Campfire.Modules.Chat.Application.Common;
using Campfire.Modules.Chat.Application.Models;
using MediatR;

namespace Campfire.Modules.Chat.Application.SendMessage;

public sealed record SendMessageCommand(Guid GroupId, Guid UserId, Guid ClientMessageId, string Kind, string Content, Guid? ReplyToMessageId)
    : IRequest<ChatResult<SendMessageResult>>;
