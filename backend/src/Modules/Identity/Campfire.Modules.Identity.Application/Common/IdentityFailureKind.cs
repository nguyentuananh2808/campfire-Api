namespace Campfire.Modules.Identity.Application.Common;

public enum IdentityFailureKind
{
    Validation,
    Conflict,
    Unauthorized,
    LockedOut,
    NotFound
}
