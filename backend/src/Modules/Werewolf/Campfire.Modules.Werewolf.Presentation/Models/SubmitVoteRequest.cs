namespace Campfire.Modules.Werewolf.Presentation.Models;

public sealed record SubmitVoteRequest(Guid ClientActionId, Guid? TargetUserId);
