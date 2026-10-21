namespace Campfire.Modules.Groups.Application.Models;

public sealed record InvitationCreated(Guid Id, string Token, DateTimeOffset ExpiresAtUtc);
