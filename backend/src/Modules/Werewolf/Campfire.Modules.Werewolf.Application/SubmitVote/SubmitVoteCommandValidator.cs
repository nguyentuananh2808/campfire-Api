using FluentValidation;
namespace Campfire.Modules.Werewolf.Application.SubmitVote;

public sealed class SubmitVoteCommandValidator : AbstractValidator<SubmitVoteCommand>
{ public SubmitVoteCommandValidator() => RuleFor(x => x.ClientActionId).NotEmpty(); }
