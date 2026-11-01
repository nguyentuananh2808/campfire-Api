using FluentValidation;

namespace Campfire.Modules.Learning.Application.PrepareReadingText;

public sealed class PrepareReadingTextCommandValidator : AbstractValidator<PrepareReadingTextCommand>
{
    public PrepareReadingTextCommandValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Văn bản không được để trống.")
            .Must(text => !string.IsNullOrWhiteSpace(text)).WithMessage("Văn bản không được chỉ chứa khoảng trắng.")
            .MaximumLength(10_000).WithMessage("Văn bản tối đa 10.000 ký tự.");
    }
}
