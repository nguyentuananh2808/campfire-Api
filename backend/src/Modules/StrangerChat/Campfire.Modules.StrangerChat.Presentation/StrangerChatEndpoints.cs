using Campfire.Modules.StrangerChat.Application.UseCases;
using Campfire.Modules.StrangerChat.Presentation.Endpoints;
using Campfire.Modules.StrangerChat.Presentation.Models;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
namespace Campfire.Modules.StrangerChat.Presentation;

public static class StrangerChatEndpoints
{
    private const string Cookie = "Campfire.Guest";
    public static IEndpointRouteBuilder MapStrangerChatEndpoints(this IEndpointRouteBuilder endpoints) { var g = endpoints.MapGroup("/api/v1/stranger").WithTags("Stranger Chat"); g.MapPost("/session", Create).AddEndpointFilter<AntiforgeryEndpointFilter>().RequireRateLimiting("stranger-session"); g.MapGet("/session", Get); g.MapDelete("/session", Delete).AddEndpointFilter<AntiforgeryEndpointFilter>(); g.MapPost("/matchmaking", Queue).AddEndpointFilter<AntiforgeryEndpointFilter>().RequireRateLimiting("stranger-match"); g.MapDelete("/matchmaking", Cancel).AddEndpointFilter<AntiforgeryEndpointFilter>(); g.MapGet("/conversation", Conversation); g.MapGet("/conversations/{id:guid}/messages", Messages); g.MapPost("/conversations/{id:guid}/messages", Send).AddEndpointFilter<AntiforgeryEndpointFilter>().RequireRateLimiting("stranger-message"); g.MapPost("/conversations/{id:guid}/end", End).AddEndpointFilter<AntiforgeryEndpointFilter>(); g.MapPost("/conversations/{id:guid}/reports", Report).AddEndpointFilter<AntiforgeryEndpointFilter>().RequireRateLimiting("stranger-report"); return endpoints; }
    private static string? Token(HttpContext c) => c.Request.Cookies[Cookie];
    private static async Task<IResult> Create(CreateSessionRequest r, ISender s, HttpContext c, CancellationToken ct) { var x = await s.Send(new CreateSessionCommand(r.Nickname, r.Gender, r.GenderPreference, r.ConfirmsAdult), ct); if (!x.IsSuccess) return StrangerEndpointResults.Failure(x, c); c.Response.Cookies.Append(Cookie, x.Value!.RawToken, new CookieOptions { HttpOnly = true, Secure = c.Request.IsHttps, SameSite = SameSiteMode.Lax, MaxAge = TimeSpan.FromHours(24), Path = "/" }); return Results.Created("/api/v1/stranger/session", x.Value.Session); }
    private static async Task<IResult> Get(ISender s, HttpContext c, CancellationToken ct) { var t = Token(c); if (t is null) return StrangerEndpointResults.Missing(c); var x = await s.Send(new GetSessionQuery(t), ct); return x.IsSuccess ? Results.Ok(x.Value) : StrangerEndpointResults.Failure(x, c); }
    private static async Task<IResult> Delete(ISender s, HttpContext c, CancellationToken ct) { var t = Token(c); if (t is null) return StrangerEndpointResults.Missing(c); var x = await s.Send(new DeleteSessionCommand(t), ct); if (!x.IsSuccess) return StrangerEndpointResults.Failure(x, c); c.Response.Cookies.Delete(Cookie); return Results.NoContent(); }
    private static async Task<IResult> Queue(ISender s, HttpContext c, CancellationToken ct) => await Run(c, Token(c), t => s.Send(new QueueCommand(t), ct));
    private static async Task<IResult> Cancel(ISender s, HttpContext c, CancellationToken ct) => await Run(c, Token(c), t => s.Send(new CancelQueueCommand(t), ct));
    private static async Task<IResult> Conversation(ISender s, HttpContext c, CancellationToken ct) => await Run(c, Token(c), t => s.Send(new GetConversationQuery(t), ct));
    private static async Task<IResult> Messages(Guid id, int? limit, ISender s, HttpContext c, CancellationToken ct) => await Run(c, Token(c), t => s.Send(new GetMessagesQuery(t, id, limit ?? 50), ct));
    private static async Task<IResult> Send(Guid id, SendStrangerMessageRequest r, ISender s, HttpContext c, CancellationToken ct) => await Run(c, Token(c), t => s.Send(new SendMessageCommand(t, id, r.ClientMessageId, r.Content), ct));
    private static async Task<IResult> End(Guid id, EndConversationRequest r, ISender s, HttpContext c, CancellationToken ct) => await Run(c, Token(c), t => s.Send(new EndConversationCommand(t, id, r.Block), ct));
    private static async Task<IResult> Report(Guid id, ReportConversationRequest r, ISender s, HttpContext c, CancellationToken ct) => await Run(c, Token(c), t => s.Send(new ReportCommand(t, id, r.Reason, r.Note), ct));
    private static async Task<IResult> Run<T>(HttpContext c, string? token, Func<string, Task<Application.Common.StrangerResult<T>>> action) { if (token is null) return StrangerEndpointResults.Missing(c); var x = await action(token); return x.IsSuccess ? Results.Ok(x.Value) : StrangerEndpointResults.Failure(x, c); }
}
