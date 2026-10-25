using Campfire.Modules.StrangerChat.Application.Common;
using Campfire.Modules.StrangerChat.Application.Models;
using MediatR;

namespace Campfire.Modules.StrangerChat.Application.UseCases;

public sealed record SetMessageReactionCommand(
    string Token,
    Guid ConversationId,
    Guid MessageId,
    string? Reaction) : IRequest<StrangerResult<MessageModel>>;
