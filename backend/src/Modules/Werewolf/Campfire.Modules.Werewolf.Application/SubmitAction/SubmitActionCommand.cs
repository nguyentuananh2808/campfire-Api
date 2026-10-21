using Campfire.Modules.Werewolf.Application.Common;
using Campfire.Modules.Werewolf.Application.Models;
using MediatR;
namespace Campfire.Modules.Werewolf.Application.SubmitAction;

public sealed record SubmitActionCommand(Guid GameId, Guid UserId, Guid ClientActionId, string Kind, Guid? TargetUserId) : IRequest<WerewolfResult<WerewolfRoomSnapshot>>;
