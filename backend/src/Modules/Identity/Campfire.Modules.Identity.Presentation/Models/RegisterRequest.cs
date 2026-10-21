namespace Campfire.Modules.Identity.Presentation.Models;

public sealed record RegisterRequest(string Email, string Password, string DisplayName);
