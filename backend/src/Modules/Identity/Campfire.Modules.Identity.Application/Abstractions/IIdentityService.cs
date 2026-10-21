using Campfire.Modules.Identity.Application.Common;
using Campfire.Modules.Identity.Contracts;

namespace Campfire.Modules.Identity.Application.Abstractions;

public interface IIdentityService
{
    Task<IdentityResult<UserAccount>> RegisterAsync(
        string email,
        string password,
        string displayName,
        CancellationToken cancellationToken);

    Task<IdentityResult<UserAccount>> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken);

    Task<IdentityResult<UserAccount>> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken);

    Task<IdentityResult<bool>> ChangePasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken);

    Task<IdentityResult<bool>> LogoutAsync(CancellationToken cancellationToken);
}
