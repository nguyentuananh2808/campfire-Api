using Campfire.Modules.Identity.Application.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace Campfire.Modules.Identity.Presentation;

public sealed class IdentityValidationExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not RequestValidationException validationException)
        {
            return false;
        }

        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await Results.ValidationProblem(
                validationException.Errors,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Dữ liệu không hợp lệ.",
                extensions: new Dictionary<string, object?>
                {
                    ["code"] = "validation_error",
                    ["traceId"] = context.TraceIdentifier
                })
            .ExecuteAsync(context);
        return true;
    }
}
