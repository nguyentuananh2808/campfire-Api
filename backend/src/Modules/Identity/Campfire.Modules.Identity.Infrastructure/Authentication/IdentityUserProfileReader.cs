using Campfire.Modules.Identity.Contracts;
using Campfire.Modules.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Campfire.Modules.Identity.Infrastructure.Authentication;

public sealed class IdentityUserProfileReader(IdentityDbContext dbContext) : IUserProfileReader
{
    public async Task<IReadOnlyDictionary<Guid, UserProfile>> GetByIdsAsync(
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken)
    {
        if (userIds.Count == 0)
        {
            return new Dictionary<Guid, UserProfile>();
        }

        return await dbContext.Users
            .AsNoTracking()
            .Where(user => userIds.Contains(user.Id))
            .Select(user => new UserProfile(user.Id, user.DisplayName))
            .ToDictionaryAsync(user => user.Id, cancellationToken);
    }
}
