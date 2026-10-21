using Campfire.Modules.Werewolf.Application.Abstractions;
using Campfire.Modules.Werewolf.Application.Common;
using MediatR;
namespace Campfire.Modules.Werewolf.Application.LeaveRoom;

public sealed class LeaveRoomCommandHandler(IWerewolfService service) : IRequestHandler<LeaveRoomCommand, WerewolfResult<bool>>
{ public Task<WerewolfResult<bool>> Handle(LeaveRoomCommand request, CancellationToken cancellationToken) => service.LeaveAsync(request.RoomId, request.UserId, cancellationToken); }
