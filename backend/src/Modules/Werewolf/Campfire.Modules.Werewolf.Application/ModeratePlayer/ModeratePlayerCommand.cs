using Campfire.Modules.Werewolf.Application.Common;
using Campfire.Modules.Werewolf.Application.Models;
using MediatR;
namespace Campfire.Modules.Werewolf.Application.ModeratePlayer;

public sealed record ModeratePlayerCommand(Guid RoomId, Guid UserId, Guid TargetUserId, string Action) : IRequest<WerewolfResult<WerewolfRoomSnapshot>>;
