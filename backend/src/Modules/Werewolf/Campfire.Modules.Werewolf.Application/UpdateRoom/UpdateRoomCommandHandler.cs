using Campfire.Modules.Werewolf.Application.Abstractions;
using Campfire.Modules.Werewolf.Application.Common;
using Campfire.Modules.Werewolf.Application.Models;
using MediatR;
namespace Campfire.Modules.Werewolf.Application.UpdateRoom;

public sealed class UpdateRoomCommandHandler(IWerewolfService service) : IRequestHandler<UpdateRoomCommand, WerewolfResult<WerewolfRoomSnapshot>>
{ public Task<WerewolfResult<WerewolfRoomSnapshot>> Handle(UpdateRoomCommand request, CancellationToken cancellationToken) => service.UpdateAsync(request.RoomId, request.UserId, request.ExpectedVersion, request.Name, request.Visibility, request.MinimumPlayers, request.MaximumPlayers, request.DeadVoiceEnabled, cancellationToken); }
