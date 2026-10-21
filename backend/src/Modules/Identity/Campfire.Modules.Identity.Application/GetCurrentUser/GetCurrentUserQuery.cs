using Campfire.Modules.Identity.Application.Common;
using Campfire.Modules.Identity.Contracts;
using MediatR;

namespace Campfire.Modules.Identity.Application.GetCurrentUser;

public sealed record GetCurrentUserQuery(Guid UserId) : IRequest<IdentityResult<UserAccount>>;
