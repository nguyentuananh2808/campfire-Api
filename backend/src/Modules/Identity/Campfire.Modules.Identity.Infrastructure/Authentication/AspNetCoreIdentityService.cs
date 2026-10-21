using Campfire.Modules.Identity.Application.Abstractions;
using Campfire.Modules.Identity.Application.Common;
using Campfire.Modules.Identity.Contracts;
using Campfire.Modules.Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;

namespace Campfire.Modules.Identity.Infrastructure.Authentication;

public sealed class AspNetCoreIdentityService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    TimeProvider timeProvider) : IIdentityService
{
    public async Task<IdentityResult<UserAccount>> RegisterAsync(
        string email,
        string password,
        string displayName,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = email,
            DisplayName = displayName,
            CreatedAtUtc = timeProvider.GetUtcNow()
        };

        var creation = await userManager.CreateAsync(user, password);
        if (!creation.Succeeded)
        {
            return IdentityResult<UserAccount>.Failed(MapCreationFailures(creation.Errors));
        }

        await signInManager.SignInAsync(user, isPersistent: false);
        return IdentityResult<UserAccount>.Success(ToAccount(user));
    }

    public async Task<IdentityResult<UserAccount>> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return InvalidCredentials();
        }

        var signIn = await signInManager.PasswordSignInAsync(
            user,
            password,
            isPersistent: false,
            lockoutOnFailure: true);

        if (signIn.IsLockedOut)
        {
            return IdentityResult<UserAccount>.Failed(new IdentityFailure(
                IdentityFailureKind.LockedOut,
                "account_locked",
                "Tài khoản tạm khóa do đăng nhập sai quá nhiều lần. Vui lòng thử lại sau."));
        }

        return signIn.Succeeded
            ? IdentityResult<UserAccount>.Success(ToAccount(user))
            : InvalidCredentials();
    }

    public async Task<IdentityResult<UserAccount>> GetCurrentUserAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await userManager.FindByIdAsync(userId.ToString());
        return user is null
            ? IdentityResult<UserAccount>.Failed(new IdentityFailure(
                IdentityFailureKind.Unauthorized,
                "session_invalid",
                "Phiên đăng nhập không còn hợp lệ."))
            : IdentityResult<UserAccount>.Success(ToAccount(user));
    }

    public async Task<IdentityResult<bool>> ChangePasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return IdentityResult<bool>.Failed(new IdentityFailure(
                IdentityFailureKind.Unauthorized,
                "session_invalid",
                "Phiên đăng nhập không còn hợp lệ."));
        }

        var changed = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!changed.Succeeded)
        {
            return IdentityResult<bool>.Failed(MapPasswordFailures(changed.Errors));
        }

        await userManager.UpdateSecurityStampAsync(user);
        await signInManager.RefreshSignInAsync(user);
        return IdentityResult<bool>.Success(true);
    }

    public async Task<IdentityResult<bool>> LogoutAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await signInManager.SignOutAsync();
        return IdentityResult<bool>.Success(true);
    }

    private static UserAccount ToAccount(ApplicationUser user) =>
        new(user.Id, user.Email!, user.DisplayName);

    private static IdentityResult<UserAccount> InvalidCredentials() =>
        IdentityResult<UserAccount>.Failed(new IdentityFailure(
            IdentityFailureKind.Unauthorized,
            "invalid_credentials",
            "Email hoặc mật khẩu không đúng."));

    private static IdentityFailure[] MapCreationFailures(IEnumerable<IdentityError> errors) => errors
        .Select(error => error.Code is "DuplicateEmail" or "DuplicateUserName"
            ? new IdentityFailure(
                IdentityFailureKind.Conflict,
                "email_already_registered",
                "Email này đã được đăng ký.",
                "email")
            : new IdentityFailure(
                IdentityFailureKind.Validation,
                "password_policy_failed",
                error.Description,
                "password"))
        .ToArray();

    private static IdentityFailure[] MapPasswordFailures(IEnumerable<IdentityError> errors) => errors
        .Select(error => error.Code == "PasswordMismatch"
            ? new IdentityFailure(
                IdentityFailureKind.Validation,
                "current_password_incorrect",
                "Mật khẩu hiện tại không đúng.",
                "currentPassword")
            : new IdentityFailure(
                IdentityFailureKind.Validation,
                "password_policy_failed",
                error.Description,
                "newPassword"))
        .ToArray();
}
