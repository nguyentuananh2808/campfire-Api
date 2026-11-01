using Campfire.Modules.Chat.Application.Common;
using Campfire.Modules.Chat.Application.Models;
using MediatR;

namespace Campfire.Modules.Chat.Application.GetImage;

public sealed record GetImageQuery(Guid GroupId, Guid MessageId, Guid UserId) : IRequest<ChatResult<ChatImageContent>>;
