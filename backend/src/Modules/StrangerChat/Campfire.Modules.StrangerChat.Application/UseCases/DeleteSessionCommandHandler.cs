using Campfire.Modules.StrangerChat.Application.Abstractions; using Campfire.Modules.StrangerChat.Application.Common; using MediatR;
namespace Campfire.Modules.StrangerChat.Application.UseCases;
public sealed class DeleteSessionCommandHandler(IStrangerChatService service) : IRequestHandler<DeleteSessionCommand, StrangerResult<bool>> { public Task<StrangerResult<bool>> Handle(DeleteSessionCommand r, CancellationToken ct) => service.DeleteSessionAsync(r.Token, ct); }
