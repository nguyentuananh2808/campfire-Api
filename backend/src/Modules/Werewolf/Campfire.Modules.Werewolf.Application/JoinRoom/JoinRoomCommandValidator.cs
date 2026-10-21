using FluentValidation;
namespace Campfire.Modules.Werewolf.Application.JoinRoom;

public sealed class JoinRoomCommandValidator : AbstractValidator<JoinRoomCommand>
{ public JoinRoomCommandValidator() => RuleFor(x => x.Code).NotEmpty().Length(8).Matches("^[A-Za-z0-9]+$"); }
