using Campfire.Modules.Identity.Application.Common;
using MediatR;

namespace Campfire.Modules.Identity.Application.Logout;

public sealed record LogoutCommand : IRequest<IdentityResult<bool>>;
