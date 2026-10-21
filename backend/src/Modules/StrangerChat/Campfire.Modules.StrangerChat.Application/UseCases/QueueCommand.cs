using Campfire.Modules.StrangerChat.Application.Common; using Campfire.Modules.StrangerChat.Application.Models; using MediatR;
namespace Campfire.Modules.StrangerChat.Application.UseCases;
public sealed record QueueCommand(string Token) : IRequest<StrangerResult<MatchmakingModel>>;
