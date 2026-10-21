using Campfire.Modules.Werewolf.Application.Abstractions;
using Campfire.Modules.Werewolf.Application.Common;
using Campfire.Modules.Werewolf.Application.Models;
using MediatR;
namespace Campfire.Modules.Werewolf.Application.CreateRoom;

public sealed class CreateRoomCommandHandler(IWerewolfService service) : IRequestHandler<CreateRoomCommand, WerewolfResult<WerewolfRoomSnapshot>>
{ public Task<WerewolfResult<WerewolfRoomSnapshot>> Handle(CreateRoomCommand request, CancellationToken cancellationToken) => service.CreateAsync(request.UserId, request.Name, request.Visibility, request.MinimumPlayers, request.MaximumPlayers, request.DeadVoiceEnabled, cancellationToken); }
