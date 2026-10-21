using Campfire.Modules.Werewolf.Application.Abstractions;
using Campfire.Modules.Werewolf.Application.Common;
using Campfire.Modules.Werewolf.Application.Models;
using MediatR;
namespace Campfire.Modules.Werewolf.Application.GetVoiceAccess;

public sealed class GetVoiceAccessCommandHandler(IWerewolfService service) : IRequestHandler<GetVoiceAccessCommand, WerewolfResult<VoiceAccessView>>
{ public Task<WerewolfResult<VoiceAccessView>> Handle(GetVoiceAccessCommand request, CancellationToken cancellationToken) => service.IssueVoiceAccessAsync(request.RoomId, request.UserId, cancellationToken); }
