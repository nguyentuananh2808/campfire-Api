using Microsoft.AspNetCore.Identity;

namespace Campfire.Modules.Identity.Infrastructure.Persistence;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public required string DisplayName { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }
}
