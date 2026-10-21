using Campfire.Modules.StrangerChat.Application.Common;
using Campfire.Modules.StrangerChat.Application.Models;
using MediatR;
namespace Campfire.Modules.StrangerChat.Application.UseCases;

public sealed record CreateSessionCommand(string Nickname, string Gender, string GenderPreference, bool ConfirmsAdult) : IRequest<StrangerResult<CreatedSession>>;
