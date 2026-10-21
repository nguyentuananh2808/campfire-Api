using Campfire.Modules.StrangerChat.Infrastructure.Realtime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Campfire.Modules.StrangerChat.Infrastructure;

public static class StrangerChatEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapStrangerChatHub(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHub<StrangerChatHub>("/hubs/stranger")
            .RequireRateLimiting("stranger-match");
        return endpoints;
    }
}
