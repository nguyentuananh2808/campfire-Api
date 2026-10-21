using Campfire.Modules.Identity.Application.Abstractions;
using Campfire.Modules.Identity.Application.Common;
using MediatR;

namespace Campfire.Modules.Identity.Application.ChangePassword;

public sealed class ChangePasswordCommandHandler(IIdentityService identityService)
    : IRequestHandler<ChangePasswordCommand, IdentityResult<bool>>
{
    public Task<IdentityResult<bool>> Handle(
        ChangePasswordCommand command,
        CancellationToken cancellationToken) => identityService.ChangePasswordAsync(
            command.UserId,
            command.CurrentPassword,
            command.NewPassword,
            cancellationToken);
}
