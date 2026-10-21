using FluentValidation;
namespace Campfire.Modules.StrangerChat.Application.UseCases;

public sealed class CreateSessionCommandValidator : AbstractValidator<CreateSessionCommand>
{
    private static readonly string[] Genders = ["Male", "Female", "Other"];
    private static readonly string[] Preferences = ["Any", "Male", "Female", "Other"];

    public CreateSessionCommandValidator()
    {
        RuleFor(x => x.Nickname).NotEmpty().MinimumLength(2).MaximumLength(24);
        RuleFor(x => x.Gender).Must(value => Genders.Contains(value)).WithMessage("Giới tính không hợp lệ.");
        RuleFor(x => x.GenderPreference).Must(value => Preferences.Contains(value)).WithMessage("Lựa chọn người muốn gặp không hợp lệ.");
        RuleFor(x => x.ConfirmsAdult).Equal(true).WithMessage("Bạn phải xác nhận đủ 18 tuổi.");
    }
}
