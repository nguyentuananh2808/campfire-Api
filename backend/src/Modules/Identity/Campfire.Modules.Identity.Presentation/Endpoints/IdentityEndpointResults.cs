using Campfire.Modules.Identity.Application.Common;
using Microsoft.AspNetCore.Http;

namespace Campfire.Modules.Identity.Presentation.Endpoints;

internal static class IdentityEndpointResults
{
    public static IResult Failure<T>(IdentityResult<T> result, HttpContext context)
    {
        var primary = result.Failures.First();
        if (primary.Kind == IdentityFailureKind.Validation)
        {
            var errors = result.Failures
                .GroupBy(failure => failure.Field ?? string.Empty)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(failure => failure.Message).Distinct().ToArray());
            return Results.ValidationProblem(
                errors,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Dữ liệu không hợp lệ.",
                extensions: new Dictionary<string, object?>
                {
                    ["code"] = primary.Code,
                    ["traceId"] = context.TraceIdentifier
                });
        }

        var statusCode = primary.Kind switch
        {
            IdentityFailureKind.Conflict => StatusCodes.Status409Conflict,
            IdentityFailureKind.LockedOut => StatusCodes.Status429TooManyRequests,
            IdentityFailureKind.NotFound => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status401Unauthorized
        };
        return Results.Problem(
            statusCode: statusCode,
            title: primary.Message,
            extensions: new Dictionary<string, object?>
            {
                ["code"] = primary.Code,
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
