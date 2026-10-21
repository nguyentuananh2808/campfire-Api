using Campfire.Modules.Events.Application.Common;
using Microsoft.AspNetCore.Http;

namespace Campfire.Modules.Events.Presentation.Endpoints;

internal static class EventEndpointResults
{
    public static IResult Failure<T>(EventResult<T> result, HttpContext context) => Results.Problem(
        statusCode: result.Failure!.StatusCode,
        title: result.Failure.Message,
        extensions: new Dictionary<string, object?> { ["code"] = result.Failure.Code, ["traceId"] = context.TraceIdentifier });

    public static IResult InvalidSession(HttpContext context) => Results.Problem(
        statusCode: StatusCodes.Status401Unauthorized, title: "Phiên đăng nhập không còn hợp lệ.",
        extensions: new Dictionary<string, object?> { ["code"] = "session_invalid", ["traceId"] = context.TraceIdentifier });
}
