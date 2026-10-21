using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Diagnostics;

namespace Campfire.Api.Composition;

public sealed class AntiforgeryExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not AntiforgeryValidationException)
        {
            return false;
        }

        await Results.Problem(
            statusCode: StatusCodes.Status400BadRequest,
            title: "Yêu cầu chống giả mạo không hợp lệ.",
            detail: "Hãy tải lại trang và thử lại.",
            extensions: new Dictionary<string, object?>
            {
                ["code"] = "antiforgery_validation_failed",
                ["traceId"] = context.TraceIdentifier
            }).ExecuteAsync(context);
        return true;
    }
}
