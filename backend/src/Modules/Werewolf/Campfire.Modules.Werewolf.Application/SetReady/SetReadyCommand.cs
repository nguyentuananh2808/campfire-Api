using Campfire.Modules.Werewolf.Application.Common;
using Campfire.Modules.Werewolf.Application.Models;
using MediatR;
namespace Campfire.Modules.Werewolf.Application.SetReady;

public sealed record SetReadyCommand(Guid RoomId, Guid UserId, bool Ready) : IRequest<WerewolfResult<WerewolfRoomSnapshot>>;
