using Campfire.Modules.Werewolf.Application.Abstractions;
using Campfire.Modules.Werewolf.Application.Common;
using Campfire.Modules.Werewolf.Application.Models;
using MediatR;
namespace Campfire.Modules.Werewolf.Application.ListRooms;

public sealed class ListRoomsQueryHandler(IWerewolfService service) : IRequestHandler<ListRoomsQuery, WerewolfResult<IReadOnlyList<WerewolfRoomSummary>>>
{ public Task<WerewolfResult<IReadOnlyList<WerewolfRoomSummary>>> Handle(ListRoomsQuery request, CancellationToken cancellationToken) => service.ListPublicAsync(request.UserId, cancellationToken); }
