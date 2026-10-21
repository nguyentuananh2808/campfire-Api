using Campfire.Modules.Chat.Application.Abstractions;
using Campfire.Modules.Chat.Application.Common;
using Campfire.Modules.Chat.Application.Models;
using MediatR;

namespace Campfire.Modules.Chat.Application.GetMessages;

public sealed class GetMessagesQueryHandler(IChatMessageService service)
    : IRequestHandler<GetMessagesQuery, ChatResult<ChatMessagePage>>
{
    public Task<ChatResult<ChatMessagePage>> Handle(GetMessagesQuery request, CancellationToken cancellationToken) =>
        service.GetAsync(request.GroupId, request.UserId, request.Before, request.After, request.Limit, cancellationToken);
}
