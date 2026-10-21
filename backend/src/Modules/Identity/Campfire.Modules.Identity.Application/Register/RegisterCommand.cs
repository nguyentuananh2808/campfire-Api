using Campfire.Modules.Identity.Application.Common;
using Campfire.Modules.Identity.Contracts;
using MediatR;

namespace Campfire.Modules.Identity.Application.Register;

public sealed record RegisterCommand(string Email, string Password, string DisplayName)
    : IRequest<IdentityResult<UserAccount>>;
