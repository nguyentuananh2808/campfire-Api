using Campfire.Modules.Werewolf.Application.Common;
using Campfire.Modules.Werewolf.Application.Models;
using MediatR;
namespace Campfire.Modules.Werewolf.Application.CreateRoom;

public sealed record CreateRoomCommand(Guid UserId, string Name, string Visibility, int MinimumPlayers, int MaximumPlayers, bool DeadVoiceEnabled) : IRequest<WerewolfResult<WerewolfRoomSnapshot>>;
