using Campfire.Modules.Chat.Application.Models;
using MediatR;

namespace Campfire.Modules.Chat.Application.GetInbox;

public sealed record GetChatInboxQuery(Guid UserId) : IRequest<IReadOnlyList<ChatInboxItem>>;
