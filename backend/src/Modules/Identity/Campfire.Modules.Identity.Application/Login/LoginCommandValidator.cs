using FluentValidation;

namespace Campfire.Modules.Identity.Application.Login;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty().WithMessage("Email là bắt buộc.")
            .EmailAddress().WithMessage("Email không hợp lệ.")
            .MaximumLength(256).WithMessage("Email không được dài quá 256 ký tự.");
        RuleFor(command => command.Password)
            .NotEmpty().WithMessage("Mật khẩu là bắt buộc.")
            .MaximumLength(128).WithMessage("Mật khẩu không được dài quá 128 ký tự.");
    }
}
