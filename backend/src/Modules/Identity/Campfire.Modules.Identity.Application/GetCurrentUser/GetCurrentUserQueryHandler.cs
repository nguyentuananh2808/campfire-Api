using Campfire.Modules.Identity.Application.Abstractions;
using Campfire.Modules.Identity.Application.Common;
using Campfire.Modules.Identity.Contracts;
using MediatR;

namespace Campfire.Modules.Identity.Application.GetCurrentUser;

public sealed class GetCurrentUserQueryHandler(IIdentityService identityService)
    : IRequestHandler<GetCurrentUserQuery, IdentityResult<UserAccount>>
{
    public Task<IdentityResult<UserAccount>> Handle(
        GetCurrentUserQuery query,
        CancellationToken cancellationToken) =>
        identityService.GetCurrentUserAsync(query.UserId, cancellationToken);
}
