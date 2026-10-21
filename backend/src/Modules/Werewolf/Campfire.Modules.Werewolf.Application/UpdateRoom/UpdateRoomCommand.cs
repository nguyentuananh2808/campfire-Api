using Campfire.Modules.Werewolf.Application.Common;
using Campfire.Modules.Werewolf.Application.Models;
using MediatR;
namespace Campfire.Modules.Werewolf.Application.UpdateRoom;

public sealed record UpdateRoomCommand(Guid RoomId, Guid UserId, Guid ExpectedVersion, string Name, string Visibility, int MinimumPlayers, int MaximumPlayers, bool DeadVoiceEnabled) : IRequest<WerewolfResult<WerewolfRoomSnapshot>>;
