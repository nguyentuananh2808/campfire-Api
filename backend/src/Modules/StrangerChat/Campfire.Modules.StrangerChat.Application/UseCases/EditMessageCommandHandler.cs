using Campfire.Modules.StrangerChat.Application.Abstractions;
using Campfire.Modules.StrangerChat.Application.Common;
using Campfire.Modules.StrangerChat.Application.Models;
using MediatR;

namespace Campfire.Modules.StrangerChat.Application.UseCases;

public sealed class EditMessageCommandHandler(IStrangerChatService service)
    : IRequestHandler<EditMessageCommand, StrangerResult<MessageModel>>
{
    public Task<StrangerResult<MessageModel>> Handle(EditMessageCommand request, CancellationToken cancellationToken) =>
        service.EditMessageAsync(request.Token, request.ConversationId, request.MessageId, request.Content, request.ExpectedVersion, cancellationToken);
}
