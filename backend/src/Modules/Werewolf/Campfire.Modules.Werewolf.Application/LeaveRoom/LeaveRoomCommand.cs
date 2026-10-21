using Campfire.Modules.Werewolf.Application.Common;
using MediatR;
namespace Campfire.Modules.Werewolf.Application.LeaveRoom;

public sealed record LeaveRoomCommand(Guid RoomId, Guid UserId) : IRequest<WerewolfResult<bool>>;
