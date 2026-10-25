using Campfire.Modules.Chat.Application.Abstractions;
using Campfire.Modules.Chat.Application.Common;
using Campfire.Modules.Chat.Contracts;
using MediatR;

namespace Campfire.Modules.Chat.Application.SetReaction;

public sealed class SetMessageReactionCommandHandler(IChatMessageService service)
    : IRequestHandler<SetMessageReactionCommand, ChatResult<ChatMessageDto>>
{
    public Task<ChatResult<ChatMessageDto>> Handle(SetMessageReactionCommand request, CancellationToken cancellationToken) =>
        service.SetReactionAsync(request.GroupId, request.MessageId, request.UserId, request.Reaction, cancellationToken);
}
