using Campfire.Modules.Chat.Application.Abstractions;
using Campfire.Modules.Chat.Application.Common;
using Campfire.Modules.Chat.Application.Models;
using MediatR;

namespace Campfire.Modules.Chat.Application.GetImage;

public sealed class GetImageQueryHandler(IChatMessageService service) : IRequestHandler<GetImageQuery, ChatResult<ChatImageContent>>
{
    public Task<ChatResult<ChatImageContent>> Handle(GetImageQuery request, CancellationToken cancellationToken) =>
        service.GetImageAsync(request.GroupId, request.MessageId, request.UserId, cancellationToken);
}
