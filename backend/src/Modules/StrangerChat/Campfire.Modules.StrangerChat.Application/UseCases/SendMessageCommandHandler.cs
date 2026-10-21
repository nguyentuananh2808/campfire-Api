using Campfire.Modules.StrangerChat.Application.Abstractions; using Campfire.Modules.StrangerChat.Application.Common; using Campfire.Modules.StrangerChat.Application.Models; using MediatR;
namespace Campfire.Modules.StrangerChat.Application.UseCases;
public sealed class SendMessageCommandHandler(IStrangerChatService service) : IRequestHandler<SendMessageCommand, StrangerResult<MessageModel>> { public Task<StrangerResult<MessageModel>> Handle(SendMessageCommand r, CancellationToken ct) => service.SendMessageAsync(r.Token, r.ConversationId, r.ClientMessageId, r.Content, ct); }
