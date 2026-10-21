namespace Campfire.Modules.Groups.Application.Models;

public sealed record InvitationPreview(Guid GroupId, string GroupName, bool AlreadyMember);
