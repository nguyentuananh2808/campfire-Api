using Campfire.Modules.Werewolf.Application.Common;
using Campfire.Modules.Werewolf.Application.Models;
using MediatR;
namespace Campfire.Modules.Werewolf.Application.StartGame;

public sealed record StartGameCommand(Guid RoomId, Guid UserId, Guid ExpectedVersion) : IRequest<WerewolfResult<WerewolfRoomSnapshot>>;
