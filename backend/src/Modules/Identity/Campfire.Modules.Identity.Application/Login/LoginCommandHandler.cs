using Campfire.Modules.Identity.Application.Abstractions;
using Campfire.Modules.Identity.Application.Common;
using Campfire.Modules.Identity.Contracts;
using MediatR;

namespace Campfire.Modules.Identity.Application.Login;

public sealed class LoginCommandHandler(IIdentityService identityService)
    : IRequestHandler<LoginCommand, IdentityResult<UserAccount>>
{
    public Task<IdentityResult<UserAccount>> Handle(
        LoginCommand command,
        CancellationToken cancellationToken) =>
        identityService.LoginAsync(command.Email.Trim(), command.Password, cancellationToken);
}
