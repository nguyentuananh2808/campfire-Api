using Campfire.Modules.StrangerChat.Application.Abstractions; using Campfire.Modules.StrangerChat.Application.Common; using MediatR;
namespace Campfire.Modules.StrangerChat.Application.UseCases;
public sealed class EndConversationCommandHandler(IStrangerChatService service) : IRequestHandler<EndConversationCommand, StrangerResult<bool>> { public Task<StrangerResult<bool>> Handle(EndConversationCommand r, CancellationToken ct) => service.EndAsync(r.Token, r.ConversationId, r.Block, ct); }
