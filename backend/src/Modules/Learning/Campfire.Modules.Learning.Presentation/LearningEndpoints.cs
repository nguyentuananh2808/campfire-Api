using System.Security.Claims;
using Campfire.Modules.Learning.Application;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Campfire.Modules.Learning.Presentation;

public static class LearningEndpoints
{
    public static IEndpointRouteBuilder MapLearningEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/learning").WithTags("Learning").RequireAuthorization();
        group.MapGet("/today", TodayAsync).WithName("GetLearningToday");
        group.MapGet("/courses/{slug}/path", PathAsync).WithName("GetLearningPath");
        group.MapGet("/days/{dayId:guid}", DayAsync).WithName("GetLearningDay");
        group.MapPost("/activities/{activityId:guid}/complete", CompleteAsync).AddEndpointFilter<AntiforgeryEndpointFilter>().WithName("CompleteLearningActivity");
        group.MapPost("/activities/{activityId:guid}/submit", SubmitAsync).AddEndpointFilter<AntiforgeryEndpointFilter>().WithName("SubmitLearningExercise");
        group.MapGet("/vocabulary/mindmap", MindMapAsync).WithName("GetVocabularyMindMap");
        group.MapGet("/capabilities", CapabilitiesAsync).WithName("GetLearningCapabilities");
        return endpoints;
    }

    private static async Task<IResult> TodayAsync(ClaimsPrincipal principal, ISender sender, HttpContext context, CancellationToken token) => Result(await sender.Send(new GetTodayQuery(UserId(principal)), token), context);
    private static async Task<IResult> PathAsync(string slug, ClaimsPrincipal principal, ISender sender, HttpContext context, CancellationToken token) => Result(await sender.Send(new GetPathQuery(UserId(principal), slug), token), context);
    private static async Task<IResult> DayAsync(Guid dayId, ClaimsPrincipal principal, ISender sender, HttpContext context, CancellationToken token) => Result(await sender.Send(new GetDayQuery(UserId(principal), dayId), token), context);
    private static async Task<IResult> CompleteAsync(Guid activityId, CompleteActivityRequest request, ClaimsPrincipal principal, ISender sender, HttpContext context, CancellationToken token) => Result(await sender.Send(new CompleteActivityCommand(UserId(principal), activityId, request.Score), token), context);
    private static async Task<IResult> SubmitAsync(Guid activityId, SubmitExerciseRequest request, ClaimsPrincipal principal, ISender sender, HttpContext context, CancellationToken token) => Result(await sender.Send(new SubmitExerciseCommand(UserId(principal), activityId, request.Answers.Select(x => new LearningModels.ExerciseAnswer(x.QuestionId, x.Value)).ToList()), token), context);
    private static async Task<IResult> MindMapAsync(string word, int? hskLevel, int? maxChildren, ClaimsPrincipal principal, ISender sender, HttpContext context, CancellationToken token) => Result(await sender.Send(new GetMindMapQuery(UserId(principal), word, hskLevel ?? 1, maxChildren ?? 8), token), context);
    private static async Task<IResult> CapabilitiesAsync(ISender sender, CancellationToken token) => Results.Ok(await sender.Send(new GetCapabilitiesQuery(), token));

    private static Guid UserId(ClaimsPrincipal principal) => Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var value) ? value : throw new UnauthorizedAccessException("Invalid identity session.");
    private static IResult Result<T>(LearningResult<T> result, HttpContext context)
    {
        if (result.IsSuccess) return Results.Ok(result.Value);
        var status = result.ErrorCode switch { "not_found" => 404, "forbidden" => 403, "conflict" => 409, _ => 400 };
        return Results.Problem(statusCode: status, title: result.Error, extensions: new Dictionary<string, object?> { ["code"] = result.ErrorCode, ["traceId"] = context.TraceIdentifier });
    }
}
