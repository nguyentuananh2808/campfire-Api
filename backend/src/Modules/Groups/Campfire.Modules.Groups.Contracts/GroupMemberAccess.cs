namespace Campfire.Modules.Groups.Contracts;

public sealed record GroupMemberAccess(Guid UserId, bool IsOwner);
