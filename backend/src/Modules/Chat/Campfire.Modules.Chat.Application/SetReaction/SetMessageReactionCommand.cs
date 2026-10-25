using Campfire.Modules.Chat.Application.Common;
using Campfire.Modules.Chat.Contracts;
using MediatR;

namespace Campfire.Modules.Chat.Application.SetReaction;

public sealed record SetMessageReactionCommand(Guid GroupId, Guid MessageId, Guid UserId, string? Reaction)
    : IRequest<ChatResult<ChatMessageDto>>;
