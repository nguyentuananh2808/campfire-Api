using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
namespace Campfire.Modules.Learning.Presentation;
public sealed class LearningValidationExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validation) return false;
        var errors = validation.Errors.GroupBy(x => x.PropertyName).ToDictionary(x => x.Key, x => x.Select(e => e.ErrorMessage).ToArray());
        await Results.ValidationProblem(errors, statusCode: 400, title: "Dữ liệu học tập không hợp lệ.", extensions: new Dictionary<string, object?> { ["code"] = "validation_error", ["traceId"] = context.TraceIdentifier }).ExecuteAsync(context);
        return true;
    }
}
