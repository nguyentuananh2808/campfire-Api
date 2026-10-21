using Campfire.Modules.StrangerChat.Application.Common; using MediatR;
namespace Campfire.Modules.StrangerChat.Application.UseCases;
public sealed record EndConversationCommand(string Token, Guid ConversationId, bool Block) : IRequest<StrangerResult<bool>>;
