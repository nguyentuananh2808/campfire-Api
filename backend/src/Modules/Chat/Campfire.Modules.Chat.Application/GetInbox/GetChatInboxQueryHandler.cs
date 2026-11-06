using Campfire.Modules.Chat.Application.Abstractions;
using Campfire.Modules.Chat.Application.Models;
using MediatR;

namespace Campfire.Modules.Chat.Application.GetInbox;

public sealed class GetChatInboxQueryHandler(IChatMessageService service)
    : IRequestHandler<GetChatInboxQuery, IReadOnlyList<ChatInboxItem>>
{
    public Task<IReadOnlyList<ChatInboxItem>> Handle(
        GetChatInboxQuery request,
        CancellationToken cancellationToken) =>
        service.GetInboxAsync(request.UserId, cancellationToken);
}
