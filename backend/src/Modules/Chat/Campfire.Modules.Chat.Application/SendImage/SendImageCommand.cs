using Campfire.Modules.Chat.Application.Common;
using Campfire.Modules.Chat.Application.Models;
using MediatR;

namespace Campfire.Modules.Chat.Application.SendImage;

public sealed record SendImageCommand(Guid GroupId, Guid UserId, Guid ClientMessageId, byte[] Bytes, string ContentType, Guid? ReplyToMessageId)
    : IRequest<ChatResult<SendMessageResult>>;
