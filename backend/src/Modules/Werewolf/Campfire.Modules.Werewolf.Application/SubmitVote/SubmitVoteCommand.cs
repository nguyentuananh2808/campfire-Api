using Campfire.Modules.Werewolf.Application.Common;
using Campfire.Modules.Werewolf.Application.Models;
using MediatR;
namespace Campfire.Modules.Werewolf.Application.SubmitVote;

public sealed record SubmitVoteCommand(Guid GameId, Guid UserId, Guid ClientActionId, Guid? TargetUserId) : IRequest<WerewolfResult<WerewolfRoomSnapshot>>;
