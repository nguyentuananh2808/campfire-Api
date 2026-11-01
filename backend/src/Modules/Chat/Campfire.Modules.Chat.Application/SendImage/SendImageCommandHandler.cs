using Campfire.Modules.Chat.Application.Abstractions;
using Campfire.Modules.Chat.Application.Common;
using Campfire.Modules.Chat.Application.Models;
using MediatR;

namespace Campfire.Modules.Chat.Application.SendImage;

public sealed class SendImageCommandHandler(IChatMessageService service)
    : IRequestHandler<SendImageCommand, ChatResult<SendMessageResult>>
{
    public Task<ChatResult<SendMessageResult>> Handle(SendImageCommand request, CancellationToken cancellationToken) =>
        service.SendImageAsync(request.GroupId, request.UserId, request.ClientMessageId,
            new ChatImageUpload(request.Bytes, request.ContentType), request.ReplyToMessageId, cancellationToken);
}
