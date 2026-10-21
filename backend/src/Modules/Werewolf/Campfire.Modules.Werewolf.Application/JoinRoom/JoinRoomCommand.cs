using Campfire.Modules.Werewolf.Application.Common;
using Campfire.Modules.Werewolf.Application.Models;
using MediatR;
namespace Campfire.Modules.Werewolf.Application.JoinRoom;

public sealed record JoinRoomCommand(Guid UserId, string Code, bool Spectator) : IRequest<WerewolfResult<WerewolfRoomSnapshot>>;
