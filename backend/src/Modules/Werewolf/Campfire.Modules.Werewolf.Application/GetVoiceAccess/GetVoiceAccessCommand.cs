using Campfire.Modules.Werewolf.Application.Common;
using Campfire.Modules.Werewolf.Application.Models;
using MediatR;
namespace Campfire.Modules.Werewolf.Application.GetVoiceAccess;

public sealed record GetVoiceAccessCommand(Guid RoomId, Guid UserId) : IRequest<WerewolfResult<VoiceAccessView>>;
