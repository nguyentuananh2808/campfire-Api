using System.Security.Cryptography;
using System.Text;
using Campfire.Modules.Groups.Application.Abstractions;
using Campfire.Modules.Groups.Application.Common;
using Campfire.Modules.Groups.Application.Models;
using Campfire.Modules.Groups.Domain.Auditing;
using Campfire.Modules.Groups.Domain.Groups;
using Campfire.Modules.Groups.Domain.Invitations;
using Campfire.Modules.Groups.Infrastructure.Persistence;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace Campfire.Modules.Groups.Infrastructure.Services;

public sealed class EfGroupInvitationService(
    GroupsDbContext dbContext,
    TimeProvider timeProvider) : IGroupInvitationService
{
    public async Task<GroupResult<InvitationCreated>> CreateAsync(
        Guid groupId,
        Guid actorUserId,
        int expiresInHours,
        string traceId,
        CancellationToken cancellationToken)
    {
        var accessFailure = await CheckOwnerAccessAsync(groupId, actorUserId, cancellationToken);
        if (accessFailure is not null)
        {
            return GroupResult<InvitationCreated>.Failed(accessFailure);
        }

        var now = timeProvider.GetUtcNow();
        var rawToken = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
        var invitation = GroupInvitation.Create(
            groupId,
            HashToken(rawToken),
            actorUserId,
            now,
            now.AddHours(expiresInHours));
        dbContext.Invitations.Add(invitation);
        dbContext.AuditRecords.Add(new GroupAuditRecord(
            groupId,
            actorUserId,
            "invitation.created",
            null,
            now,
            traceId));
        await dbContext.SaveChangesAsync(cancellationToken);
        return GroupResult<InvitationCreated>.Success(
            new InvitationCreated(invitation.Id, rawToken, invitation.ExpiresAtUtc));
    }

    public async Task<GroupResult<IReadOnlyList<InvitationView>>> GetForGroupAsync(
        Guid groupId,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var accessFailure = await CheckOwnerAccessAsync(groupId, actorUserId, cancellationToken);
        if (accessFailure is not null)
        {
            return GroupResult<IReadOnlyList<InvitationView>>.Failed(accessFailure);
        }

        var invitations = await dbContext.Invitations
            .AsNoTracking()
            .Where(invitation => invitation.GroupId == groupId)
            .OrderByDescending(invitation => invitation.CreatedAtUtc)
            .Select(invitation => new InvitationView(
                invitation.Id,
                invitation.CreatedAtUtc,
                invitation.ExpiresAtUtc,
                invitation.RevokedAtUtc))
            .ToListAsync(cancellationToken);
        return GroupResult<IReadOnlyList<InvitationView>>.Success(invitations);
    }

    public async Task<GroupResult<bool>> RevokeAsync(
        Guid groupId,
        Guid invitationId,
        Guid actorUserId,
        string traceId,
        CancellationToken cancellationToken)
    {
        var accessFailure = await CheckOwnerAccessAsync(groupId, actorUserId, cancellationToken);
        if (accessFailure is not null)
        {
            return GroupResult<bool>.Failed(accessFailure);
        }

        var invitation = await dbContext.Invitations.SingleOrDefaultAsync(
            candidate => candidate.Id == invitationId && candidate.GroupId == groupId,
            cancellationToken);
        if (invitation is null)
        {
            return GroupResult<bool>.Failed(GroupFailures.InvitationUnavailable());
        }

        var now = timeProvider.GetUtcNow();
        if (invitation.Revoke(now))
        {
            dbContext.AuditRecords.Add(new GroupAuditRecord(
                groupId,
                actorUserId,
                "invitation.revoked",
                null,
                now,
                traceId));
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return GroupResult<bool>.Success(true);
    }

    public async Task<GroupResult<InvitationPreview>> PreviewAsync(
        string token,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var invitation = await FindByTokenAsync(token, cancellationToken);
        if (invitation is null || !invitation.IsAvailable(timeProvider.GetUtcNow()))
        {
            return GroupResult<InvitationPreview>.Failed(GroupFailures.InvitationUnavailable());
        }

        var group = await dbContext.Groups
            .AsNoTracking()
            .SingleAsync(candidate => candidate.Id == invitation.GroupId, cancellationToken);
        var alreadyMember = await dbContext.Memberships
            .AsNoTracking()
            .AnyAsync(
                membership => membership.GroupId == group.Id && membership.UserId == userId,
                cancellationToken);
        return GroupResult<InvitationPreview>.Success(
            new InvitationPreview(group.Id, group.Name, alreadyMember));
    }

    public async Task<GroupResult<GroupSummary>> JoinAsync(
        string token,
        Guid userId,
        string traceId,
        CancellationToken cancellationToken)
    {
        var invitation = await FindByTokenAsync(token, cancellationToken);
        if (invitation is null)
        {
            return GroupResult<GroupSummary>.Failed(GroupFailures.InvitationUnavailable());
        }

        var group = await dbContext.Groups
            .AsNoTracking()
            .SingleAsync(candidate => candidate.Id == invitation.GroupId, cancellationToken);
        var existing = await dbContext.Memberships
            .AsNoTracking()
            .SingleOrDefaultAsync(
                membership => membership.GroupId == group.Id && membership.UserId == userId,
                cancellationToken);
        if (existing is not null)
        {
            var count = await CountMembersAsync(group.Id, cancellationToken);
            return GroupResult<GroupSummary>.Success(ToSummary(group, existing.Role, count));
        }

        if (!invitation.IsAvailable(timeProvider.GetUtcNow()))
        {
            return GroupResult<GroupSummary>.Failed(GroupFailures.InvitationUnavailable());
        }

        var now = timeProvider.GetUtcNow();
        dbContext.Memberships.Add(GroupMember.CreateMember(group.Id, userId, now));
        dbContext.AuditRecords.Add(new GroupAuditRecord(
            group.Id,
            userId,
            "member.joined",
            userId,
            now,
            traceId));
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            dbContext.ChangeTracker.Clear();
            var concurrentMembership = await dbContext.Memberships
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    membership => membership.GroupId == group.Id && membership.UserId == userId,
                    cancellationToken);
            if (concurrentMembership is null)
            {
                throw;
            }

            var concurrentCount = await CountMembersAsync(group.Id, cancellationToken);
            return GroupResult<GroupSummary>.Success(
                ToSummary(group, concurrentMembership.Role, concurrentCount));
        }

        var memberCount = await CountMembersAsync(group.Id, cancellationToken);
        return GroupResult<GroupSummary>.Success(ToSummary(group, GroupRole.Member, memberCount));
    }

    private async Task<GroupFailure?> CheckOwnerAccessAsync(
        Guid groupId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var membership = await dbContext.Memberships
            .AsNoTracking()
            .SingleOrDefaultAsync(
                candidate => candidate.GroupId == groupId && candidate.UserId == userId,
                cancellationToken);
        if (membership is null)
        {
            return GroupFailures.HiddenGroup();
        }

        return membership.Role == GroupRole.Owner ? null : GroupFailures.OwnerRequired();
    }

    private Task<GroupInvitation?> FindByTokenAsync(string token, CancellationToken cancellationToken)
    {
        var hash = HashToken(token);
        return dbContext.Invitations
            .AsNoTracking()
            .SingleOrDefaultAsync(invitation => invitation.TokenHash == hash, cancellationToken);
    }

    private Task<int> CountMembersAsync(Guid groupId, CancellationToken cancellationToken) =>
        dbContext.Memberships.CountAsync(
            membership => membership.GroupId == groupId,
            cancellationToken);

    private static byte[] HashToken(string token) => SHA256.HashData(Encoding.UTF8.GetBytes(token));

    private static GroupSummary ToSummary(Group group, GroupRole role, int memberCount) =>
        new(group.Id, group.Name, role.ToString(), memberCount, group.Version);
}
