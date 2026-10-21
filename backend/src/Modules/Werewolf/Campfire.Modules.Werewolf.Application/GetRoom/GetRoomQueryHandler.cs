using Campfire.Modules.Werewolf.Application.Abstractions;
using Campfire.Modules.Werewolf.Application.Common;
using Campfire.Modules.Werewolf.Application.Models;
using MediatR;
namespace Campfire.Modules.Werewolf.Application.GetRoom;

public sealed class GetRoomQueryHandler(IWerewolfService service) : IRequestHandler<GetRoomQuery, WerewolfResult<WerewolfRoomSnapshot>>
{ public Task<WerewolfResult<WerewolfRoomSnapshot>> Handle(GetRoomQuery request, CancellationToken cancellationToken) => service.GetAsync(request.RoomId, request.UserId, cancellationToken); }
