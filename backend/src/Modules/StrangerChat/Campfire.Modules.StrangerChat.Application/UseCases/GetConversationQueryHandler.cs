using Campfire.Modules.StrangerChat.Application.Abstractions; using Campfire.Modules.StrangerChat.Application.Common; using Campfire.Modules.StrangerChat.Application.Models; using MediatR;
namespace Campfire.Modules.StrangerChat.Application.UseCases;
public sealed class GetConversationQueryHandler(IStrangerChatService service) : IRequestHandler<GetConversationQuery, StrangerResult<ConversationModel>> { public Task<StrangerResult<ConversationModel>> Handle(GetConversationQuery r, CancellationToken ct) => service.GetConversationAsync(r.Token, ct); }
