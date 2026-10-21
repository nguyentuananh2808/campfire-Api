namespace Campfire.Modules.Groups.Presentation.Models;

public sealed record TransferOwnershipRequest(Guid NewOwnerUserId, Guid ExpectedVersion);
