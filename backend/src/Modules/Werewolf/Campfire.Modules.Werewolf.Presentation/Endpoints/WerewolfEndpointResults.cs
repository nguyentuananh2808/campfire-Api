using Campfire.Modules.Werewolf.Application.Common;
using Microsoft.AspNetCore.Http;
namespace Campfire.Modules.Werewolf.Presentation.Endpoints;

internal static class WerewolfEndpointResults
{
    public static IResult Failure<T>(WerewolfResult<T> result, HttpContext context) => Results.Problem(statusCode: result.Failure!.StatusCode,
        title: result.Failure.Detail, extensions: new Dictionary<string, object?> { ["code"] = result.Failure.Code, ["traceId"] = context.TraceIdentifier });
    public static IResult InvalidSession(HttpContext context) => Results.Problem(statusCode: 401, title: "Phiên đăng nhập không hợp lệ.",
        extensions: new Dictionary<string, object?> { ["code"] = "session_invalid", ["traceId"] = context.TraceIdentifier });
}
