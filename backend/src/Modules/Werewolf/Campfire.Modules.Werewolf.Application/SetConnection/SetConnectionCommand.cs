using Campfire.Modules.Werewolf.Application.Common;
using MediatR;
namespace Campfire.Modules.Werewolf.Application.SetConnection;

public sealed record SetConnectionCommand(Guid RoomId, Guid UserId, bool Connected) : IRequest<WerewolfResult<bool>>;
