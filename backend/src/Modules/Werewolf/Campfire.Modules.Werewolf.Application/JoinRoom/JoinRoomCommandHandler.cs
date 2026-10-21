using Campfire.Modules.Werewolf.Application.Abstractions;
using Campfire.Modules.Werewolf.Application.Common;
using Campfire.Modules.Werewolf.Application.Models;
using MediatR;
namespace Campfire.Modules.Werewolf.Application.JoinRoom;

public sealed class JoinRoomCommandHandler(IWerewolfService service) : IRequestHandler<JoinRoomCommand, WerewolfResult<WerewolfRoomSnapshot>>
{ public Task<WerewolfResult<WerewolfRoomSnapshot>> Handle(JoinRoomCommand request, CancellationToken cancellationToken) => service.JoinAsync(request.UserId, request.Code, request.Spectator, cancellationToken); }
