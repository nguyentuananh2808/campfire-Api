using Campfire.Modules.StrangerChat.Application.Common; using MediatR;
namespace Campfire.Modules.StrangerChat.Application.UseCases;
public sealed record CancelQueueCommand(string Token) : IRequest<StrangerResult<bool>>;
