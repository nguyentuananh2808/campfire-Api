using Campfire.Modules.Groups.Application.Common;
using Microsoft.AspNetCore.Http;

namespace Campfire.Modules.Groups.Presentation.Endpoints;

internal static class GroupEndpointResults
{
    public static IResult Failure<T>(GroupResult<T> result, HttpContext context)
    {
        var failure = result.Failure!;
        var statusCode = failure.Kind switch
        {
            GroupFailureKind.Forbidden => StatusCodes.Status403Forbidden,
            GroupFailureKind.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status404NotFound
        };
        return Results.Problem(
            statusCode: statusCode,
            title: failure.Message,
            extensions: new Dictionary<string, object?>
            {
                ["code"] = failure.Code,
                ["traceId"] = context.TraceIdentifier
            });
    }

    public static IResult InvalidSession(HttpContext context) => Results.Problem(
        statusCode: StatusCodes.Status401Unauthorized,
        title: "Phiên đăng nhập không còn hợp lệ.",
        extensions: new Dictionary<string, object?>
        {
            ["code"] = "session_invalid",
            ["traceId"] = context.TraceIdentifier
        });
}
