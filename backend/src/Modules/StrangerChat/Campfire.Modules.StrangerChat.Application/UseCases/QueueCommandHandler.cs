using Campfire.Modules.StrangerChat.Application.Abstractions; using Campfire.Modules.StrangerChat.Application.Common; using Campfire.Modules.StrangerChat.Application.Models; using MediatR;
namespace Campfire.Modules.StrangerChat.Application.UseCases;
public sealed class QueueCommandHandler(IStrangerChatService service) : IRequestHandler<QueueCommand, StrangerResult<MatchmakingModel>> { public Task<StrangerResult<MatchmakingModel>> Handle(QueueCommand r, CancellationToken ct) => service.QueueAsync(r.Token, ct); }
