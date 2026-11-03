using FluentValidation;

namespace Campfire.Modules.Learning.Application.SaveReadingDocument;

public sealed class SaveReadingDocumentCommandValidator : AbstractValidator<SaveReadingDocumentCommand>
{
    public SaveReadingDocumentCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Tiêu đề không được để trống.")
            .Must(title => !string.IsNullOrWhiteSpace(title)).WithMessage("Tiêu đề không được chỉ chứa khoảng trắng.")
            .MaximumLength(120).WithMessage("Tiêu đề tối đa 120 ký tự.");
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Văn bản không được để trống.")
            .Must(text => !string.IsNullOrWhiteSpace(text)).WithMessage("Văn bản không được chỉ chứa khoảng trắng.")
            .MaximumLength(10_000).WithMessage("Văn bản tối đa 10.000 ký tự.");
    }
}
