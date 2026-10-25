using Campfire.Modules.StrangerChat.Application.Common; using Campfire.Modules.StrangerChat.Application.Models; using MediatR;
namespace Campfire.Modules.StrangerChat.Application.UseCases;
public sealed record SendMessageCommand(string Token, Guid ConversationId, Guid ClientMessageId, string Kind, string Content, Guid? ReplyToMessageId) : IRequest<StrangerResult<MessageModel>>;
