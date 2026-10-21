using Campfire.Modules.StrangerChat.Application.Abstractions; using Campfire.Modules.StrangerChat.Application.Common; using Campfire.Modules.StrangerChat.Application.Models; using MediatR;
namespace Campfire.Modules.StrangerChat.Application.UseCases;
public sealed class GetMessagesQueryHandler(IStrangerChatService service) : IRequestHandler<GetMessagesQuery, StrangerResult<MessagePage>> { public Task<StrangerResult<MessagePage>> Handle(GetMessagesQuery r, CancellationToken ct) => service.GetMessagesAsync(r.Token, r.ConversationId, r.Limit, ct); }
