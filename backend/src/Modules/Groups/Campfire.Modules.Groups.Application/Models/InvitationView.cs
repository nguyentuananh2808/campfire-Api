namespace Campfire.Modules.Groups.Application.Models;

public sealed record InvitationView(
    Guid Id,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset ExpiresAtUtc,
    DateTimeOffset? RevokedAtUtc);
