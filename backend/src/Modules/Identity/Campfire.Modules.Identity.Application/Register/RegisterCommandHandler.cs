using Campfire.Modules.Identity.Application.Abstractions;
using Campfire.Modules.Identity.Application.Common;
using Campfire.Modules.Identity.Contracts;
using MediatR;

namespace Campfire.Modules.Identity.Application.Register;

public sealed class RegisterCommandHandler(IIdentityService identityService)
    : IRequestHandler<RegisterCommand, IdentityResult<UserAccount>>
{
    public Task<IdentityResult<UserAccount>> Handle(
        RegisterCommand command,
        CancellationToken cancellationToken) => identityService.RegisterAsync(
            command.Email.Trim(),
            command.Password,
            command.DisplayName.Trim(),
            cancellationToken);
}
