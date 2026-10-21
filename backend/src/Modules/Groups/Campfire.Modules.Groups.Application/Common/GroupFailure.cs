namespace Campfire.Modules.Groups.Application.Common;

public sealed record GroupFailure(GroupFailureKind Kind, string Code, string Message);
