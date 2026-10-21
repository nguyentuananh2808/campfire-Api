namespace Campfire.Modules.Werewolf.Presentation.Models;

public sealed record SubmitActionRequest(Guid ClientActionId, string Kind, Guid? TargetUserId);
