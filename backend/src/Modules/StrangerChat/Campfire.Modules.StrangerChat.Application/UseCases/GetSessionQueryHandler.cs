using Campfire.Modules.StrangerChat.Application.Abstractions; using Campfire.Modules.StrangerChat.Application.Common; using Campfire.Modules.StrangerChat.Application.Models; using MediatR;
namespace Campfire.Modules.StrangerChat.Application.UseCases;
public sealed class GetSessionQueryHandler(IStrangerChatService service) : IRequestHandler<GetSessionQuery, StrangerResult<GuestSessionModel>> { public Task<StrangerResult<GuestSessionModel>> Handle(GetSessionQuery r, CancellationToken ct) => service.GetSessionAsync(r.Token, ct); }
