using Campfire.Modules.Chat.Application.Common;
using Campfire.Modules.Chat.Contracts;
using MediatR;

namespace Campfire.Modules.Chat.Application.EditMessage;

public sealed record EditMessageCommand(Guid GroupId, Guid MessageId, Guid UserId, string Content, Guid ExpectedVersion)
    : IRequest<ChatResult<ChatMessageDto>>;
