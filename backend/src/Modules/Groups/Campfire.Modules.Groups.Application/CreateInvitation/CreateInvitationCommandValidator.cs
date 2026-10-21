using FluentValidation;

namespace Campfire.Modules.Groups.Application.CreateInvitation;

public sealed class CreateInvitationCommandValidator : AbstractValidator<CreateInvitationCommand>
{
    public CreateInvitationCommandValidator()
    {
        RuleFor(command => command.GroupId).NotEmpty();
        RuleFor(command => command.ActorUserId).NotEmpty();
        RuleFor(command => command.ExpiresInHours).InclusiveBetween(1, 168);
        RuleFor(command => command.TraceId).NotEmpty().MaximumLength(100);
    }
}
