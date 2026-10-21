using Campfire.Modules.Werewolf.Application.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
namespace Campfire.Modules.Werewolf.Presentation;

public sealed class WerewolfValidationExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not RequestValidationException validation) return false;
        context.Response.StatusCode = 400;
        await Results.ValidationProblem(validation.Errors, title: "Dữ liệu không hợp lệ.", extensions: new Dictionary<string, object?>
        { ["code"] = "validation_failed", ["traceId"] = context.TraceIdentifier }).ExecuteAsync(context);
        return true;
    }
}
