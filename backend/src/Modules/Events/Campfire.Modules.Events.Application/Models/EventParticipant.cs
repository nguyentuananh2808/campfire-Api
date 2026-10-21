namespace Campfire.Modules.Events.Application.Models;

public sealed record EventParticipant(Guid UserId, string DisplayName, string Response,
    DateTimeOffset RespondedAtUtc, bool IsCurrentMember);
