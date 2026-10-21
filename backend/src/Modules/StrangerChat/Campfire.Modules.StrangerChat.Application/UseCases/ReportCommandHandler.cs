using Campfire.Modules.StrangerChat.Application.Abstractions; using Campfire.Modules.StrangerChat.Application.Common; using MediatR;
namespace Campfire.Modules.StrangerChat.Application.UseCases;
public sealed class ReportCommandHandler(IStrangerChatService service) : IRequestHandler<ReportCommand, StrangerResult<bool>> { public Task<StrangerResult<bool>> Handle(ReportCommand r, CancellationToken ct) => service.ReportAsync(r.Token, r.ConversationId, r.Reason, r.Note, ct); }
