using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace Campfire.Modules.Chat.Presentation;

public sealed class ChatValidationExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validation) return false;
        var errors = validation.Errors.GroupBy(error => error.PropertyName)
            .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray());
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await Results.ValidationProblem(errors, title: "Dữ liệu không hợp lệ.",
            extensions: new Dictionary<string, object?> { ["code"] = "validation_failed", ["traceId"] = context.TraceIdentifier })
            .ExecuteAsync(context);
        return true;
    }
}
