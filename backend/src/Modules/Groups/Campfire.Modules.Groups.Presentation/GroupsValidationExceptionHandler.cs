using Campfire.Modules.Groups.Application.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace Campfire.Modules.Groups.Presentation;

public sealed class GroupsValidationExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not RequestValidationException validationException)
        {
            return false;
        }

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        await Results.ValidationProblem(
            validationException.Errors,
            statusCode: StatusCodes.Status400BadRequest,
            title: "Dữ liệu không hợp lệ.",
            extensions: new Dictionary<string, object?>
            {
                ["code"] = "validation_failed",
                ["traceId"] = httpContext.TraceIdentifier
            }).ExecuteAsync(httpContext);
        return true;
    }
}
