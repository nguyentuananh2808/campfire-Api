using Campfire.Modules.Chat.Infrastructure.Realtime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Campfire.Modules.Chat.Infrastructure;

public static class ChatEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapChatHub(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHub<ChatHub>("/hubs/chat").RequireAuthorization().RequireRateLimiting("chat");
        return endpoints;
    }
}
