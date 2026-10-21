using Campfire.Modules.Werewolf.Infrastructure.Realtime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Campfire.Modules.Werewolf.Infrastructure;

public static class WerewolfEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapWerewolfHub(this IEndpointRouteBuilder endpoints)
    { endpoints.MapHub<WerewolfHub>("/hubs/werewolf").RequireAuthorization().RequireRateLimiting("werewolf"); return endpoints; }
}
