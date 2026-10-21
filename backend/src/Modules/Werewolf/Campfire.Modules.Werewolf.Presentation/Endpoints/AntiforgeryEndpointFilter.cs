using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
namespace Campfire.Modules.Werewolf.Presentation.Endpoints;

internal sealed class AntiforgeryEndpointFilter(IAntiforgery antiforgery) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    { await antiforgery.ValidateRequestAsync(context.HttpContext); return await next(context); }
}
