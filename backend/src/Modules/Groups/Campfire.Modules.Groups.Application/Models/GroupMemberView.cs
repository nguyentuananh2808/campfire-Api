namespace Campfire.Modules.Groups.Application.Models;

public sealed record GroupMemberView(
    Guid UserId,
    string DisplayName,
    string Role,
    DateTimeOffset JoinedAtUtc);
