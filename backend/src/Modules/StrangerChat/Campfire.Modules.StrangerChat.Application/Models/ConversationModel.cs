namespace Campfire.Modules.StrangerChat.Application.Models;
public sealed record ConversationModel(Guid Id, string PartnerNickname, DateTimeOffset StartedAtUtc, bool IsActive);
