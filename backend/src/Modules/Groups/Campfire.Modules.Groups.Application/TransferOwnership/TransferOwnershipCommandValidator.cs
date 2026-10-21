using FluentValidation;

namespace Campfire.Modules.Groups.Application.TransferOwnership;

public sealed class TransferOwnershipCommandValidator : AbstractValidator<TransferOwnershipCommand>
{
    public TransferOwnershipCommandValidator()
    {
        RuleFor(command => command.GroupId).NotEmpty();
        RuleFor(command => command.NewOwnerUserId).NotEmpty();
        RuleFor(command => command.ExpectedVersion).NotEmpty();
        RuleFor(command => command.ActorUserId).NotEmpty();
        RuleFor(command => command.TraceId).NotEmpty().MaximumLength(100);
        RuleFor(command => command.NewOwnerUserId)
            .NotEqual(command => command.ActorUserId)
            .WithMessage("Chủ nhóm mới phải là một thành viên khác.");
    }
}
