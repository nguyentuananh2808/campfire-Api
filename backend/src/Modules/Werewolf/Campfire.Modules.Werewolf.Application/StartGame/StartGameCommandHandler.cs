using Campfire.Modules.Werewolf.Application.Abstractions;
using Campfire.Modules.Werewolf.Application.Common;
using Campfire.Modules.Werewolf.Application.Models;
using MediatR;
namespace Campfire.Modules.Werewolf.Application.StartGame;

public sealed class StartGameCommandHandler(IWerewolfService service) : IRequestHandler<StartGameCommand, WerewolfResult<WerewolfRoomSnapshot>>
{ public Task<WerewolfResult<WerewolfRoomSnapshot>> Handle(StartGameCommand request, CancellationToken cancellationToken) => service.StartAsync(request.RoomId, request.UserId, request.ExpectedVersion, cancellationToken); }
