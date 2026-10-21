using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Campfire.Api.Composition;

public sealed class UnexpectedExceptionHandler(ILogger<UnexpectedExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled request failure with trace {TraceId}", context.TraceIdentifier);
        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Đã có lỗi xảy ra.",
            Detail = "Vui lòng thử lại hoặc cung cấp mã theo dõi cho quản trị viên."
        };
        problem.Extensions["code"] = "unexpected_error";
        problem.Extensions["traceId"] = context.TraceIdentifier;
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}
