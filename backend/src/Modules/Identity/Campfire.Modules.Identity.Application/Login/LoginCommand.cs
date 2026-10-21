using Campfire.Modules.Identity.Application.Common;
using Campfire.Modules.Identity.Contracts;
using MediatR;

namespace Campfire.Modules.Identity.Application.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<IdentityResult<UserAccount>>;
