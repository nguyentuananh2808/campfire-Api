using FluentValidation;
namespace Campfire.Modules.Learning.Application.CompleteActivity;
public sealed class CompleteActivityCommandValidator : AbstractValidator<CompleteActivityCommand>
{
    public CompleteActivityCommandValidator() { RuleFor(x => x.ActivityId).NotEmpty(); RuleFor(x => x.Score).InclusiveBetween(0, 100); }
}
