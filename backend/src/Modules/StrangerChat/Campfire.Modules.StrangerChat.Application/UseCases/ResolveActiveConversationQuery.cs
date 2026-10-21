using MediatR;

namespace Campfire.Modules.StrangerChat.Application.UseCases;

public sealed record ResolveActiveConversationQuery(string Token) : IRequest<Guid?>;
