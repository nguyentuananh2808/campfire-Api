using FluentValidation;

namespace Campfire.Modules.Identity.Application.Register;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty().WithMessage("Email là bắt buộc.")
            .EmailAddress().WithMessage("Email không hợp lệ.")
            .MaximumLength(256).WithMessage("Email không được dài quá 256 ký tự.");
        RuleFor(command => command.Password)
            .NotEmpty().WithMessage("Mật khẩu là bắt buộc.")
            .MinimumLength(12).WithMessage("Mật khẩu phải có ít nhất 12 ký tự.")
            .MaximumLength(128).WithMessage("Mật khẩu không được dài quá 128 ký tự.");
        RuleFor(command => command.DisplayName)
            .NotEmpty().WithMessage("Tên hiển thị là bắt buộc.")
            .MinimumLength(2).WithMessage("Tên hiển thị phải có ít nhất 2 ký tự.")
            .MaximumLength(50).WithMessage("Tên hiển thị không được dài quá 50 ký tự.");
    }
}
