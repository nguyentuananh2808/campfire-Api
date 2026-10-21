using Campfire.Modules.Identity.Application.Abstractions;
using Campfire.Modules.Identity.Application.Common;
using MediatR;

namespace Campfire.Modules.Identity.Application.Logout;

public sealed class LogoutCommandHandler(IIdentityService identityService)
    : IRequestHandler<LogoutCommand, IdentityResult<bool>>
{
    public Task<IdentityResult<bool>> Handle(
        LogoutCommand command,
        CancellationToken cancellationToken) => identityService.LogoutAsync(cancellationToken);
}
