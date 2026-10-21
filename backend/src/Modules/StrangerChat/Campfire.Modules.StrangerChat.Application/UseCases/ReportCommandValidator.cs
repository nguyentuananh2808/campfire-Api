using FluentValidation;
namespace Campfire.Modules.StrangerChat.Application.UseCases;
public sealed class ReportCommandValidator : AbstractValidator<ReportCommand> { private static readonly string[] Allowed = ["harassment", "sexual", "spam", "hate", "other"]; public ReportCommandValidator() { RuleFor(x => x.Reason).Must(x => Allowed.Contains(x)); RuleFor(x => x.Note).MaximumLength(500); } }
