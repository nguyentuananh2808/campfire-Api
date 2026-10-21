using Campfire.Modules.Werewolf.Domain.Rooms;
using FluentValidation;
namespace Campfire.Modules.Werewolf.Application.CreateRoom;

public sealed class CreateRoomCommandValidator : AbstractValidator<CreateRoomCommand>
{ public CreateRoomCommandValidator() { RuleFor(x => x.Name).NotEmpty().MaximumLength(80); RuleFor(x => x.Visibility).Must(x => Enum.TryParse<RoomVisibility>(x, true, out _)); RuleFor(x => x.MinimumPlayers).InclusiveBetween(6, 20); RuleFor(x => x.MaximumPlayers).InclusiveBetween(6, 20).GreaterThanOrEqualTo(x => x.MinimumPlayers); } }
