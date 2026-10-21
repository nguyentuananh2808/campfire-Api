using Campfire.Modules.StrangerChat.Application.Abstractions; using Campfire.Modules.StrangerChat.Application.Common; using MediatR;
namespace Campfire.Modules.StrangerChat.Application.UseCases;
public sealed class CancelQueueCommandHandler(IStrangerChatService service) : IRequestHandler<CancelQueueCommand, StrangerResult<bool>> { public Task<StrangerResult<bool>> Handle(CancelQueueCommand r, CancellationToken ct) => service.CancelQueueAsync(r.Token, ct); }
