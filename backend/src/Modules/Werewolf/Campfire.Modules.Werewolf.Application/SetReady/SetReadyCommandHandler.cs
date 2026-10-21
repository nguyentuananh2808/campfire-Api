using Campfire.Modules.Werewolf.Application.Abstractions;
using Campfire.Modules.Werewolf.Application.Common;
using Campfire.Modules.Werewolf.Application.Models;
using MediatR;
namespace Campfire.Modules.Werewolf.Application.SetReady;

public sealed class SetReadyCommandHandler(IWerewolfService service) : IRequestHandler<SetReadyCommand, WerewolfResult<WerewolfRoomSnapshot>>
{ public Task<WerewolfResult<WerewolfRoomSnapshot>> Handle(SetReadyCommand request, CancellationToken cancellationToken) => service.SetReadyAsync(request.RoomId, request.UserId, request.Ready, cancellationToken); }
