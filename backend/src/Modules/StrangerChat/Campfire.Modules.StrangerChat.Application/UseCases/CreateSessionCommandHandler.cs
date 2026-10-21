using Campfire.Modules.StrangerChat.Application.Abstractions;
using Campfire.Modules.StrangerChat.Application.Common;
using Campfire.Modules.StrangerChat.Application.Models;
using MediatR;
namespace Campfire.Modules.StrangerChat.Application.UseCases;

public sealed class CreateSessionCommandHandler(IStrangerChatService service) : IRequestHandler<CreateSessionCommand, StrangerResult<CreatedSession>> { public Task<StrangerResult<CreatedSession>> Handle(CreateSessionCommand r, CancellationToken ct) => service.CreateSessionAsync(r.Nickname, r.Gender, r.GenderPreference, r.ConfirmsAdult, ct); }
