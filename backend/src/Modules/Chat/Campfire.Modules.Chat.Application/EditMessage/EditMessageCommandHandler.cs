using Campfire.Modules.Chat.Application.Abstractions;
using Campfire.Modules.Chat.Application.Common;
using Campfire.Modules.Chat.Contracts;
using MediatR;

namespace Campfire.Modules.Chat.Application.EditMessage;

public sealed class EditMessageCommandHandler(IChatMessageService service)
    : IRequestHandler<EditMessageCommand, ChatResult<ChatMessageDto>>
{
    public Task<ChatResult<ChatMessageDto>> Handle(EditMessageCommand request, CancellationToken cancellationToken) =>
        service.EditAsync(request.GroupId, request.MessageId, request.UserId, request.Content, request.ExpectedVersion, cancellationToken);
}
