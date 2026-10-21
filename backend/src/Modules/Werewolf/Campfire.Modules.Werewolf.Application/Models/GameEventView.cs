namespace Campfire.Modules.Werewolf.Application.Models;

public sealed record GameEventView(Guid Id, string Kind, string Message, DateTimeOffset OccurredAtUtc);
