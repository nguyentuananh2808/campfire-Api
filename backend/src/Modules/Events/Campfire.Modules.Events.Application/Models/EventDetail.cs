namespace Campfire.Modules.Events.Application.Models;

public sealed record EventDetail(Guid Id, Guid GroupId, Guid CreatorUserId, string CreatorDisplayName,
    string Title, string Description, DateTimeOffset StartsAtUtc, string Location, string Status,
    Guid Version, bool CanManage, string? MyResponse, IReadOnlyList<EventParticipant> Participants,
    IReadOnlyList<UnansweredMember> UnansweredMembers);
