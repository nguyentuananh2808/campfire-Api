using Campfire.Modules.Chat.Application.Abstractions;
using Campfire.Modules.Chat.Application.Common;
using Campfire.Modules.Chat.Application.Models;
using MediatR;

namespace Campfire.Modules.Chat.Application.SendMessage;

public sealed class SendMessageCommandHandler(IChatMessageService service)
    : IRequestHandler<SendMessageCommand, ChatResult<SendMessageResult>>
{
    public Task<ChatResult<SendMessageResult>> Handle(SendMessageCommand request, CancellationToken cancellationToken) =>
        service.SendAsync(request.GroupId, request.UserId, request.ClientMessageId, request.Kind, request.Content, cancellationToken);
}
