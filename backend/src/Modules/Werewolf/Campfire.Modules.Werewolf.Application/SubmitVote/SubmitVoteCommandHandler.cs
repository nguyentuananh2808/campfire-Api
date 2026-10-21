using Campfire.Modules.Werewolf.Application.Abstractions;
using Campfire.Modules.Werewolf.Application.Common;
using Campfire.Modules.Werewolf.Application.Models;
using MediatR;
namespace Campfire.Modules.Werewolf.Application.SubmitVote;

public sealed class SubmitVoteCommandHandler(IWerewolfService service) : IRequestHandler<SubmitVoteCommand, WerewolfResult<WerewolfRoomSnapshot>>
{ public Task<WerewolfResult<WerewolfRoomSnapshot>> Handle(SubmitVoteCommand request, CancellationToken cancellationToken) => service.SubmitVoteAsync(request.GameId, request.UserId, request.ClientActionId, request.TargetUserId, cancellationToken); }
