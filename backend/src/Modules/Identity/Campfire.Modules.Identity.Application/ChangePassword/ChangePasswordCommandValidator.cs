using FluentValidation;

namespace Campfire.Modules.Identity.Application.ChangePassword;

public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.CurrentPassword)
            .NotEmpty().WithMessage("Mật khẩu hiện tại là bắt buộc.")
            .MaximumLength(128).WithMessage("Mật khẩu hiện tại không hợp lệ.");
        RuleFor(command => command.NewPassword)
            .NotEmpty().WithMessage("Mật khẩu mới là bắt buộc.")
            .MinimumLength(12).WithMessage("Mật khẩu mới phải có ít nhất 12 ký tự.")
            .MaximumLength(128).WithMessage("Mật khẩu mới không được dài quá 128 ký tự.")
            .NotEqual(command => command.CurrentPassword).WithMessage("Mật khẩu mới phải khác mật khẩu hiện tại.");
    }
}
