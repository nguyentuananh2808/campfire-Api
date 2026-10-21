using Campfire.Modules.Werewolf.Application.Common;
using Campfire.Modules.Werewolf.Application.Models;
using MediatR;
namespace Campfire.Modules.Werewolf.Application.ListRooms;

public sealed record ListRoomsQuery(Guid UserId) : IRequest<WerewolfResult<IReadOnlyList<WerewolfRoomSummary>>>;
