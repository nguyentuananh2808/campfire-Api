using Campfire.Modules.Chat.Application.Abstractions;
using Campfire.Modules.Chat.Application.Common;
using MediatR;

namespace Campfire.Modules.Chat.Application.MarkRead;

public sealed class MarkGroupReadCommandHandler(IChatMessageService service)
    : IRequestHandler<MarkGroupReadCommand, ChatResult<DateTimeOffset>>
{
    public Task<ChatResult<DateTimeOffset>> Handle(
        MarkGroupReadCommand request,
        CancellationToken cancellationToken) =>
        service.MarkReadAsync(
            request.GroupId,
            request.UserId,
            request.MessageId,
            cancellationToken);
}
