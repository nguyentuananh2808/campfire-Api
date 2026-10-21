using Campfire.Modules.Werewolf.Application.Abstractions;
using Campfire.Modules.Werewolf.Application.Common;
using Campfire.Modules.Werewolf.Application.Models;
using MediatR;
namespace Campfire.Modules.Werewolf.Application.SubmitAction;

public sealed class SubmitActionCommandHandler(IWerewolfService service) : IRequestHandler<SubmitActionCommand, WerewolfResult<WerewolfRoomSnapshot>>
{ public Task<WerewolfResult<WerewolfRoomSnapshot>> Handle(SubmitActionCommand request, CancellationToken cancellationToken) => service.SubmitActionAsync(request.GameId, request.UserId, request.ClientActionId, request.Kind, request.TargetUserId, cancellationToken); }
