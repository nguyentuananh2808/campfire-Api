namespace Campfire.Modules.Identity.Application.Common;

public sealed record IdentityFailure(
    IdentityFailureKind Kind,
    string Code,
    string Message,
    string? Field = null);
