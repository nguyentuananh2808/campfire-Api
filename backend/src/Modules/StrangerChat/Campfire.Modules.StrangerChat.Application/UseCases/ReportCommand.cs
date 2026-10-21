using Campfire.Modules.StrangerChat.Application.Common; using MediatR;
namespace Campfire.Modules.StrangerChat.Application.UseCases;
public sealed record ReportCommand(string Token, Guid ConversationId, string Reason, string? Note) : IRequest<StrangerResult<bool>>;
