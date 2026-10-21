using Campfire.Modules.Werewolf.Application.Common;
using Campfire.Modules.Werewolf.Application.Models;
using MediatR;
namespace Campfire.Modules.Werewolf.Application.GetRoom;

public sealed record GetRoomQuery(Guid RoomId, Guid UserId) : IRequest<WerewolfResult<WerewolfRoomSnapshot>>;
