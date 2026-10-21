using FluentValidation;
namespace Campfire.Modules.Werewolf.Application.ModeratePlayer;

public sealed class ModeratePlayerCommandValidator : AbstractValidator<ModeratePlayerCommand>
{ public ModeratePlayerCommandValidator() { RuleFor(x => x.TargetUserId).NotEmpty(); RuleFor(x => x.Action).Must(x => x is "mute" or "unmute" or "kick"); } }
