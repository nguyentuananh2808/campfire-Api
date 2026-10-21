namespace Campfire.Modules.Groups.Application.Models;

public sealed record GroupSummary(
    Guid Id,
    string Name,
    string Role,
    int MemberCount,
    Guid Version);
