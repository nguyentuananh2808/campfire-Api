using Campfire.Modules.Games.Application.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace Campfire.Modules.Games.Presentation;

public sealed class GamesValidationExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not GamesValidationException validationException) return false;
        await Results.ValidationProblem(validationException.Errors, statusCode: StatusCodes.Status400BadRequest,
            title: "Dữ liệu trò chơi chưa hợp lệ.", extensions: new Dictionary<string, object?>
            {
                ["code"] = "games_validation_failed",
                ["traceId"] = context.TraceIdentifier
            }).ExecuteAsync(context);
        return true;
    }
}
