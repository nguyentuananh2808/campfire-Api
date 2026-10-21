using Campfire.Modules.Werewolf.Application.Abstractions;
using Campfire.Modules.Werewolf.Application.Common;
using MediatR;
namespace Campfire.Modules.Werewolf.Application.SetConnection;

public sealed class SetConnectionCommandHandler(IWerewolfService service) : IRequestHandler<SetConnectionCommand, WerewolfResult<bool>>
{ public Task<WerewolfResult<bool>> Handle(SetConnectionCommand request, CancellationToken cancellationToken) => service.SetConnectionAsync(request.RoomId, request.UserId, request.Connected, cancellationToken); }
