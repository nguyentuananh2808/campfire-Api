using Campfire.Modules.Events.Application.Abstractions;
using Campfire.Modules.Events.Application.Common;
using Campfire.Modules.Events.Application.Models;
using Campfire.Modules.Events.Domain.Auditing;
using Campfire.Modules.Events.Domain.Events;
using Campfire.Modules.Events.Infrastructure.Persistence;
using Campfire.Modules.Groups.Contracts;
using Campfire.Modules.Identity.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Campfire.Modules.Events.Infrastructure.Services;

public sealed class EfEventService(EventsDbContext dbContext, IGroupMembershipService membershipService,
    IUserProfileReader profileReader, TimeProvider timeProvider) : IEventService
{
    public async Task<EventResult<EventDetail>> CreateAsync(Guid groupId, Guid userId, string title,
        string description, DateTimeOffset startsAtUtc, string location, CancellationToken cancellationToken)
    {
        if (!(await membershipService.GetAccessAsync(groupId, userId, cancellationToken)).IsMember)
            return EventResult<EventDetail>.Failed(EventFailures.HiddenResource());
        var now = timeProvider.GetUtcNow();
        if (startsAtUtc <= now) return EventResult<EventDetail>.Failed(EventFailures.FutureTimeRequired());
        var item = GroupEvent.Create(groupId, userId, title, description, startsAtUtc, location, now);
        dbContext.Events.Add(item);
        await dbContext.SaveChangesAsync(cancellationToken);
        return await BuildDetailAsync(item, userId, false, cancellationToken);
    }

    public async Task<EventResult<IReadOnlyList<EventSummary>>> ListAsync(Guid groupId, Guid userId,
        int limit, CancellationToken cancellationToken)
    {
        if (!(await membershipService.GetAccessAsync(groupId, userId, cancellationToken)).IsMember)
            return EventResult<IReadOnlyList<EventSummary>>.Failed(EventFailures.HiddenResource());
        var items = await dbContext.Events.AsNoTracking().Where(item => item.GroupId == groupId)
            .OrderByDescending(item => item.StartsAtUtc).ThenByDescending(item => item.Id).Take(limit)
            .Select(item => new EventSummary(item.Id, item.GroupId, item.CreatorUserId, item.Title,
                item.StartsAtUtc, item.Location, item.Status.ToString(), item.Version,
                dbContext.Rsvps.Count(rsvp => rsvp.EventId == item.Id && rsvp.Response == RsvpResponse.Going),
                dbContext.Rsvps.Count(rsvp => rsvp.EventId == item.Id && rsvp.Response == RsvpResponse.NotGoing),
                dbContext.Rsvps.Where(rsvp => rsvp.EventId == item.Id && rsvp.UserId == userId)
                    .Select(rsvp => rsvp.Response.ToString()).SingleOrDefault()))
            .ToListAsync(cancellationToken);
        return EventResult<IReadOnlyList<EventSummary>>.Success(items);
    }

    public async Task<EventResult<EventDetail>> GetAsync(Guid groupId, Guid eventId, Guid userId, CancellationToken cancellationToken)
    {
        var access = await membershipService.GetAccessAsync(groupId, userId, cancellationToken);
        if (!access.IsMember) return EventResult<EventDetail>.Failed(EventFailures.HiddenResource());
        var item = await dbContext.Events.AsNoTracking().SingleOrDefaultAsync(candidate => candidate.Id == eventId && candidate.GroupId == groupId, cancellationToken);
        return item is null ? EventResult<EventDetail>.Failed(EventFailures.HiddenResource())
            : await BuildDetailAsync(item, userId, access.IsOwner, cancellationToken);
    }

    public async Task<EventResult<EventDetail>> UpdateAsync(Guid groupId, Guid eventId, Guid userId,
        Guid expectedVersion, string title, string description, DateTimeOffset startsAtUtc, string location,
        CancellationToken cancellationToken)
    {
        var access = await membershipService.GetAccessAsync(groupId, userId, cancellationToken);
        if (!access.IsMember) return EventResult<EventDetail>.Failed(EventFailures.HiddenResource());
        var item = await dbContext.Events.SingleOrDefaultAsync(candidate => candidate.Id == eventId && candidate.GroupId == groupId, cancellationToken);
        if (item is null) return EventResult<EventDetail>.Failed(EventFailures.HiddenResource());
        if (item.CreatorUserId != userId && !access.IsOwner) return EventResult<EventDetail>.Failed(EventFailures.ManageForbidden());
        if (item.Status == EventStatus.Cancelled) return EventResult<EventDetail>.Failed(EventFailures.EventLocked());
        var now = timeProvider.GetUtcNow();
        if (startsAtUtc <= now) return EventResult<EventDetail>.Failed(EventFailures.FutureTimeRequired());
        if (!item.TryUpdate(expectedVersion, title, description, startsAtUtc, location, now))
            return EventResult<EventDetail>.Failed(EventFailures.Conflict());
        try { await dbContext.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException) { return EventResult<EventDetail>.Failed(EventFailures.Conflict()); }
        return await BuildDetailAsync(item, userId, access.IsOwner, cancellationToken);
    }

    public async Task<EventResult<EventDetail>> ChangeStatusAsync(Guid groupId, Guid eventId, Guid userId,
        Guid expectedVersion, string status, string traceId, CancellationToken cancellationToken)
    {
        var access = await membershipService.GetAccessAsync(groupId, userId, cancellationToken);
        if (!access.IsMember) return EventResult<EventDetail>.Failed(EventFailures.HiddenResource());
        var item = await dbContext.Events.SingleOrDefaultAsync(candidate => candidate.Id == eventId && candidate.GroupId == groupId, cancellationToken);
        if (item is null) return EventResult<EventDetail>.Failed(EventFailures.HiddenResource());
        if (item.CreatorUserId != userId && !access.IsOwner) return EventResult<EventDetail>.Failed(EventFailures.ManageForbidden());
        if (item.Version != expectedVersion) return EventResult<EventDetail>.Failed(EventFailures.Conflict());
        var target = Enum.Parse<EventStatus>(status, true);
        if (!GroupEvent.CanTransition(item.Status, target)) return EventResult<EventDetail>.Failed(EventFailures.InvalidTransition());
        var now = timeProvider.GetUtcNow();
        item.TryChangeStatus(expectedVersion, target, now);
        dbContext.AuditRecords.Add(new EventAuditRecord(item.Id, groupId, userId,
            target == EventStatus.Confirmed ? "event.confirmed" : "event.cancelled", now, traceId));
        try { await dbContext.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException) { return EventResult<EventDetail>.Failed(EventFailures.Conflict()); }
        return await BuildDetailAsync(item, userId, access.IsOwner, cancellationToken);
    }

    public async Task<EventResult<EventDetail>> SetRsvpAsync(Guid groupId, Guid eventId, Guid userId,
        string response, CancellationToken cancellationToken)
    {
        var access = await membershipService.GetAccessAsync(groupId, userId, cancellationToken);
        if (!access.IsMember) return EventResult<EventDetail>.Failed(EventFailures.HiddenResource());
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var item = await dbContext.Events
            .FromSqlInterpolated($@"SELECT * FROM events.""Events"" WHERE ""Id"" = {eventId} AND ""GroupId"" = {groupId} FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken);
        if (item is null)
        {
            await transaction.RollbackAsync(cancellationToken);
            return EventResult<EventDetail>.Failed(EventFailures.HiddenResource());
        }
        if (item.Status != EventStatus.Open)
        {
            await transaction.RollbackAsync(cancellationToken);
            return EventResult<EventDetail>.Failed(EventFailures.RsvpLocked());
        }
        var parsed = Enum.Parse<RsvpResponse>(response, true);
        var now = timeProvider.GetUtcNow();
        await dbContext.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO events.""EventRsvps"" (""EventId"", ""UserId"", ""Response"", ""RespondedAtUtc"")
            VALUES ({eventId}, {userId}, {parsed.ToString()}, {now})
            ON CONFLICT (""EventId"", ""UserId"") DO UPDATE
            SET ""Response"" = EXCLUDED.""Response"", ""RespondedAtUtc"" = EXCLUDED.""RespondedAtUtc""", cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return await BuildDetailAsync(item, userId, access.IsOwner, cancellationToken);
    }

    private async Task<EventResult<EventDetail>> BuildDetailAsync(GroupEvent item, Guid userId, bool isOwner, CancellationToken cancellationToken)
    {
        var rsvps = await dbContext.Rsvps.AsNoTracking().Where(rsvp => rsvp.EventId == item.Id)
            .OrderBy(rsvp => rsvp.RespondedAtUtc).ToListAsync(cancellationToken);
        var members = await membershipService.GetMembersAsync(item.GroupId, cancellationToken);
        var memberIds = members.Select(member => member.UserId).ToHashSet();
        var profileIds = memberIds.Concat(rsvps.Select(rsvp => rsvp.UserId)).Append(item.CreatorUserId).Distinct().ToArray();
        var profiles = await profileReader.GetByIdsAsync(profileIds, cancellationToken);
        string Name(Guid id) => profiles.TryGetValue(id, out var profile) ? profile.DisplayName : "Thành viên";
        var participants = rsvps.Select(rsvp => new EventParticipant(rsvp.UserId, Name(rsvp.UserId),
            rsvp.Response.ToString(), rsvp.RespondedAtUtc, memberIds.Contains(rsvp.UserId))).ToArray();
        var answeredIds = rsvps.Select(rsvp => rsvp.UserId).ToHashSet();
        var unanswered = memberIds.Where(id => !answeredIds.Contains(id)).Select(id => new UnansweredMember(id, Name(id))).ToArray();
        var mine = rsvps.SingleOrDefault(rsvp => rsvp.UserId == userId)?.Response.ToString();
        return EventResult<EventDetail>.Success(new EventDetail(item.Id, item.GroupId, item.CreatorUserId,
            Name(item.CreatorUserId), item.Title, item.Description, item.StartsAtUtc, item.Location,
            item.Status.ToString(), item.Version, item.CreatorUserId == userId || isOwner, mine, participants, unanswered));
    }
}
