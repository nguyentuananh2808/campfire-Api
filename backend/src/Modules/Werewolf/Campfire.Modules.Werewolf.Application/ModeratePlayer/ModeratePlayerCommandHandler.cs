using Campfire.Modules.Werewolf.Application.Abstractions;
using Campfire.Modules.Werewolf.Application.Common;
using Campfire.Modules.Werewolf.Application.Models;
using MediatR;
namespace Campfire.Modules.Werewolf.Application.ModeratePlayer;

public sealed class ModeratePlayerCommandHandler(IWerewolfService service) : IRequestHandler<ModeratePlayerCommand, WerewolfResult<WerewolfRoomSnapshot>>
{ public Task<WerewolfResult<WerewolfRoomSnapshot>> Handle(ModeratePlayerCommand request, CancellationToken cancellationToken) => service.ModerateAsync(request.RoomId, request.UserId, request.TargetUserId, request.Action, cancellationToken); }
