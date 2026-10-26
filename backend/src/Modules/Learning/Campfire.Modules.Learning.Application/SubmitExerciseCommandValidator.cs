using FluentValidation;
namespace Campfire.Modules.Learning.Application;
public sealed class SubmitExerciseCommandValidator : AbstractValidator<SubmitExerciseCommand>
{
    public SubmitExerciseCommandValidator() { RuleFor(x => x.ActivityId).NotEmpty(); RuleFor(x => x.Answers).NotEmpty().Must(x => x.Count <= 50); RuleForEach(x => x.Answers).ChildRules(a => { a.RuleFor(x => x.QuestionId).NotEmpty().MaximumLength(64); a.RuleFor(x => x.Answer).NotEmpty().MaximumLength(200); }); }
}
