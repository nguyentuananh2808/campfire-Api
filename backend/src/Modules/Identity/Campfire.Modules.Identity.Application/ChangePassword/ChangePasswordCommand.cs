using Campfire.Modules.Identity.Application.Common;
using MediatR;

namespace Campfire.Modules.Identity.Application.ChangePassword;

public sealed record ChangePasswordCommand(Guid UserId, string CurrentPassword, string NewPassword)
    : IRequest<IdentityResult<bool>>;
