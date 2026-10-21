using Campfire.Modules.StrangerChat.Application.Abstractions;
using MediatR;

namespace Campfire.Modules.StrangerChat.Application.UseCases;

public sealed class ResolveActiveConversationQueryHandler(IStrangerChatService service)
    : IRequestHandler<ResolveActiveConversationQuery, Guid?>
{
    public Task<Guid?> Handle(ResolveActiveConversationQuery request, CancellationToken cancellationToken) =>
        service.ResolveActiveConversationAsync(request.Token, cancellationToken);
}
