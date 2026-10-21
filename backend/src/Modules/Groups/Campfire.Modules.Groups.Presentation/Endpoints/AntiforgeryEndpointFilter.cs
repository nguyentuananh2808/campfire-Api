using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;

namespace Campfire.Modules.Groups.Presentation.Endpoints;

public sealed class AntiforgeryEndpointFilter(IAntiforgery antiforgery) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        try
        {
            await antiforgery.ValidateRequestAsync(context.HttpContext);
            return await next(context);
        }
        catch (AntiforgeryValidationException)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Yêu cầu chống giả mạo không hợp lệ.",
                detail: "Hãy tải lại trang và thử lại.",
                extensions: new Dictionary<string, object?>
                {
                    ["code"] = "antiforgery_validation_failed",
                    ["traceId"] = context.HttpContext.TraceIdentifier
                });
        }
    }
}
