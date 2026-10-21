namespace Campfire.Modules.Identity.Presentation.Models;

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
