using Campfire.Modules.Chat.Application.Common;
using MediatR;

namespace Campfire.Modules.Chat.Application.MarkRead;

public sealed record MarkGroupReadCommand(Guid GroupId, Guid UserId, Guid MessageId)
    : IRequest<ChatResult<DateTimeOffset>>;
