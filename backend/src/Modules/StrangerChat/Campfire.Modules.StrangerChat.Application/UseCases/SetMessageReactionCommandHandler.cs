using Campfire.Modules.StrangerChat.Application.Abstractions;
using Campfire.Modules.StrangerChat.Application.Common;
using Campfire.Modules.StrangerChat.Application.Models;
using MediatR;

namespace Campfire.Modules.StrangerChat.Application.UseCases;

public sealed class SetMessageReactionCommandHandler(IStrangerChatService service)
    : IRequestHandler<SetMessageReactionCommand, StrangerResult<MessageModel>>
{
    public Task<StrangerResult<MessageModel>> Handle(SetMessageReactionCommand request, CancellationToken cancellationToken) =>
        service.SetReactionAsync(request.Token, request.ConversationId, request.MessageId, request.Reaction, cancellationToken);
}
