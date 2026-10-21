using Campfire.Modules.Groups.Contracts;
using Campfire.Modules.Groups.Domain.Groups;
using Campfire.Modules.Groups.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Campfire.Modules.Groups.Infrastructure.Services;

public sealed class EfGroupMembershipService(GroupsDbContext dbContext) : IGroupMembershipService
{
    public async Task<GroupAccess> GetAccessAsync(
        Guid groupId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var role = await dbContext.Memberships
            .AsNoTracking()
            .Where(membership => membership.GroupId == groupId && membership.UserId == userId)
            .Select(membership => (GroupRole?)membership.Role)
            .SingleOrDefaultAsync(cancellationToken);
        return new GroupAccess(role is not null, role == GroupRole.Owner);
    }

    public async Task<IReadOnlyList<GroupMemberAccess>> GetMembersAsync(
        Guid groupId,
        CancellationToken cancellationToken) =>
        await dbContext.Memberships
            .AsNoTracking()
            .Where(membership => membership.GroupId == groupId)
            .Select(membership => new GroupMemberAccess(
                membership.UserId,
                membership.Role == GroupRole.Owner))
            .ToListAsync(cancellationToken);
}
