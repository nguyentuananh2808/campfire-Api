using FluentValidation;

namespace Campfire.Modules.Groups.Application.PreviewInvitation;

public sealed class PreviewInvitationCommandValidator : AbstractValidator<PreviewInvitationCommand>
{
    public PreviewInvitationCommandValidator()
    {
        RuleFor(command => command.Token).NotEmpty().MaximumLength(128);
        RuleFor(command => command.UserId).NotEmpty();
    }
}
