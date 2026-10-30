using Campfire.Modules.StrangerChat.Application.Common;
using Campfire.Modules.StrangerChat.Application.Models;
using MediatR;

namespace Campfire.Modules.StrangerChat.Application.UseCases;

public sealed record EditMessageCommand(string Token, Guid ConversationId, Guid MessageId, string Content, Guid ExpectedVersion)
    : IRequest<StrangerResult<MessageModel>>;
