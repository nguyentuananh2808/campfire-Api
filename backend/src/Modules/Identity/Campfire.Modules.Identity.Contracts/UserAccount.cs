namespace Campfire.Modules.Identity.Contracts;

public sealed record UserAccount(Guid Id, string Email, string DisplayName);
