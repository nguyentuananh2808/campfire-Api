using Campfire.Modules.Groups.Application.Abstractions;
using Campfire.Modules.Groups.Application.Common;
using Campfire.Modules.Groups.Application.Models;
using Campfire.Modules.Groups.Contracts;
using Campfire.Modules.Groups.Domain.Auditing;
using Campfire.Modules.Groups.Domain.Groups;
using Campfire.Modules.Groups.Infrastructure.Persistence;
using Campfire.Modules.Identity.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Campfire.Modules.Groups.Infrastructure.Services;

public sealed class EfGroupManagementService(
    GroupsDbContext dbContext,
    IUserProfileReader userProfileReader,
    IGroupMembershipRevocationPublisher revocationPublisher,
    TimeProvider timeProvider) : IGroupManagementService
{
    public async Task<GroupResult<GroupSummary>> CreateAsync(
        Guid userId,
        string name,
        string traceId,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var group = Group.Create(name, userId, now);
        dbContext.Groups.Add(group);
        dbContext.AuditRecords.Add(new GroupAuditRecord(
            group.Id,
            userId,
            "group.created",
            null,
            now,
            traceId));
        await dbContext.SaveChangesAsync(cancellationToken);
        return GroupResult<GroupSummary>.Success(
            new GroupSummary(group.Id, group.Name, GroupRole.Owner.ToString(), 1, group.Version));
    }

    public async Task<GroupResult<GroupSummary>> RenameAsync(
        Guid groupId,
        Guid userId,
        string name,
        string traceId,
        CancellationToken cancellationToken)
    {
        var group = await dbContext.Groups.SingleOrDefaultAsync(
            candidate => candidate.Id == groupId,
            cancellationToken);
        var membership = await dbContext.Memberships.SingleOrDefaultAsync(
            candidate => candidate.GroupId == groupId && candidate.UserId == userId,
            cancellationToken);
        var accessFailure = CheckOwnerAccess(group, membership);
        if (accessFailure is not null)
        {
            return GroupResult<GroupSummary>.Failed(accessFailure);
        }

        group!.Rename(name);
        dbContext.AuditRecords.Add(new GroupAuditRecord(
            groupId,
            userId,
            "group.renamed",
            null,
            timeProvider.GetUtcNow(),
            traceId));
        await dbContext.SaveChangesAsync(cancellationToken);
        var memberCount = await dbContext.Memberships.CountAsync(
            candidate => candidate.GroupId == groupId,
            cancellationToken);
        return GroupResult<GroupSummary>.Success(ToSummary(group, membership!.Role, memberCount));
    }

    public async Task<IReadOnlyList<GroupSummary>> GetMineAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var groups = await (
            from membership in dbContext.Memberships.AsNoTracking()
            join groupEntity in dbContext.Groups.AsNoTracking() on membership.GroupId equals groupEntity.Id
            where membership.UserId == userId
            orderby groupEntity.Name
            select new
            {
                groupEntity.Id,
                groupEntity.Name,
                membership.Role,
                MemberCount = dbContext.Memberships.Count(candidate => candidate.GroupId == groupEntity.Id),
                groupEntity.Version
            })
            .ToListAsync(cancellationToken);
        return groups
            .Select(group => new GroupSummary(
                group.Id,
                group.Name,
                group.Role.ToString(),
                group.MemberCount,
                group.Version))
            .ToArray();
    }

    public async Task<GroupResult<IReadOnlyList<GroupMemberView>>> GetMembersAsync(
        Guid groupId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var canView = await dbContext.Memberships
            .AsNoTracking()
            .AnyAsync(
                membership => membership.GroupId == groupId && membership.UserId == userId,
                cancellationToken);
        if (!canView)
        {
            return GroupResult<IReadOnlyList<GroupMemberView>>.Failed(GroupFailures.HiddenGroup());
        }

        var memberships = await dbContext.Memberships
            .AsNoTracking()
            .Where(membership => membership.GroupId == groupId)
            .OrderByDescending(membership => membership.Role)
            .ThenBy(membership => membership.JoinedAtUtc)
            .ToListAsync(cancellationToken);
        var profiles = await userProfileReader.GetByIdsAsync(
            memberships.Select(membership => membership.UserId).ToArray(),
            cancellationToken);
        var members = memberships
            .Select(membership => new GroupMemberView(
                membership.UserId,
                profiles.TryGetValue(membership.UserId, out var profile)
                    ? profile.DisplayName
                    : "Thành viên",
                membership.Role.ToString(),
                membership.JoinedAtUtc))
            .ToArray();
        return GroupResult<IReadOnlyList<GroupMemberView>>.Success(members);
    }

    public async Task<GroupResult<bool>> RemoveMemberAsync(
        Guid groupId,
        Guid memberUserId,
        Guid actorUserId,
        string traceId,
        CancellationToken cancellationToken)
    {
        var actor = await dbContext.Memberships.SingleOrDefaultAsync(
            membership => membership.GroupId == groupId && membership.UserId == actorUserId,
            cancellationToken);
        if (actor is null)
        {
            return GroupResult<bool>.Failed(GroupFailures.HiddenGroup());
        }

        if (actor.Role != GroupRole.Owner)
        {
            return GroupResult<bool>.Failed(GroupFailures.OwnerRequired());
        }

        var target = await dbContext.Memberships.SingleOrDefaultAsync(
            membership => membership.GroupId == groupId && membership.UserId == memberUserId,
            cancellationToken);
        if (target is null)
        {
            return GroupResult<bool>.Failed(GroupFailures.MemberNotFound());
        }

        if (target.Role == GroupRole.Owner)
        {
            return GroupResult<bool>.Failed(GroupFailures.OwnerMustTransfer());
        }

        dbContext.Memberships.Remove(target);
        dbContext.AuditRecords.Add(new GroupAuditRecord(
            groupId,
            actorUserId,
            "member.removed",
            memberUserId,
            timeProvider.GetUtcNow(),
            traceId));
        await dbContext.SaveChangesAsync(cancellationToken);
        await revocationPublisher.PublishAsync(groupId, memberUserId, cancellationToken);
        return GroupResult<bool>.Success(true);
    }

    public async Task<GroupResult<bool>> LeaveAsync(
        Guid groupId,
        Guid userId,
        string traceId,
        CancellationToken cancellationToken)
    {
        var membership = await dbContext.Memberships.SingleOrDefaultAsync(
            candidate => candidate.GroupId == groupId && candidate.UserId == userId,
            cancellationToken);
        if (membership is null)
        {
            return GroupResult<bool>.Failed(GroupFailures.HiddenGroup());
        }

        if (membership.Role == GroupRole.Owner)
        {
            return GroupResult<bool>.Failed(GroupFailures.OwnerMustTransfer());
        }

        dbContext.Memberships.Remove(membership);
        dbContext.AuditRecords.Add(new GroupAuditRecord(
            groupId,
            userId,
            "member.left",
            userId,
            timeProvider.GetUtcNow(),
            traceId));
        await dbContext.SaveChangesAsync(cancellationToken);
        await revocationPublisher.PublishAsync(groupId, userId, cancellationToken);
        return GroupResult<bool>.Success(true);
    }

    public async Task<GroupResult<GroupSummary>> TransferOwnershipAsync(
        Guid groupId,
        Guid newOwnerUserId,
        Guid expectedVersion,
        Guid actorUserId,
        string traceId,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var group = await dbContext.Groups.SingleOrDefaultAsync(
            candidate => candidate.Id == groupId,
            cancellationToken);
        var memberships = await dbContext.Memberships
            .Where(membership => membership.GroupId == groupId)
            .ToListAsync(cancellationToken);
        var actor = memberships.SingleOrDefault(membership => membership.UserId == actorUserId);
        var accessFailure = CheckOwnerAccess(group, actor);
        if (accessFailure is not null)
        {
            return GroupResult<GroupSummary>.Failed(accessFailure);
        }

        var newOwner = memberships.SingleOrDefault(membership => membership.UserId == newOwnerUserId);
        if (newOwner is null || newOwner.Role != GroupRole.Member)
        {
            return GroupResult<GroupSummary>.Failed(GroupFailures.MemberNotFound());
        }

        if (!group!.TryAdvanceVersion(expectedVersion))
        {
            return GroupResult<GroupSummary>.Failed(GroupFailures.OwnershipConflict());
        }

        actor!.DemoteToMember();
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            newOwner.PromoteToOwner();
            dbContext.AuditRecords.Add(new GroupAuditRecord(
                groupId,
                actorUserId,
                "owner.transferred",
                newOwnerUserId,
                timeProvider.GetUtcNow(),
                traceId));
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            return GroupResult<GroupSummary>.Failed(GroupFailures.OwnershipConflict());
        }

        return GroupResult<GroupSummary>.Success(
            ToSummary(group, GroupRole.Member, memberships.Count));
    }

    private static GroupFailure? CheckOwnerAccess(Group? group, GroupMember? membership)
    {
        if (group is null || membership is null)
        {
            return GroupFailures.HiddenGroup();
        }

        return membership.Role == GroupRole.Owner ? null : GroupFailures.OwnerRequired();
    }

    private static GroupSummary ToSummary(Group group, GroupRole role, int memberCount) =>
        new(group.Id, group.Name, role.ToString(), memberCount, group.Version);
}
