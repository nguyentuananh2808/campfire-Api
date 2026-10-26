using FluentValidation;
namespace Campfire.Modules.Learning.Application;
public sealed class GetMindMapQueryValidator : AbstractValidator<GetMindMapQuery>
{
    public GetMindMapQueryValidator() { RuleFor(x => x.Word).NotEmpty().MaximumLength(20).Matches("^[\\p{L}\\p{M}]+$"); RuleFor(x => x.HskLevel).InclusiveBetween(1, 6); RuleFor(x => x.MaxChildren).InclusiveBetween(1, 12); }
}
