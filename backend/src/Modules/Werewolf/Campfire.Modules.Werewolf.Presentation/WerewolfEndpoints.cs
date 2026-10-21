using System.Security.Claims;
using Campfire.Modules.Werewolf.Application.CreateRoom;
using Campfire.Modules.Werewolf.Application.GetRoom;
using Campfire.Modules.Werewolf.Application.GetVoiceAccess;
using Campfire.Modules.Werewolf.Application.JoinRoom;
using Campfire.Modules.Werewolf.Application.LeaveRoom;
using Campfire.Modules.Werewolf.Application.ListRooms;
using Campfire.Modules.Werewolf.Application.ModeratePlayer;
using Campfire.Modules.Werewolf.Application.SetReady;
using Campfire.Modules.Werewolf.Application.StartGame;
using Campfire.Modules.Werewolf.Application.SubmitAction;
using Campfire.Modules.Werewolf.Application.SubmitVote;
using Campfire.Modules.Werewolf.Application.UpdateRoom;
using Campfire.Modules.Werewolf.Presentation.Endpoints;
using Campfire.Modules.Werewolf.Presentation.Models;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Campfire.Modules.Werewolf.Presentation;

public static class WerewolfEndpoints
{
    public static IEndpointRouteBuilder MapWerewolfEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var api = endpoints.MapGroup("/api/v1/werewolf").WithTags("Werewolf").RequireAuthorization();
        api.MapGet("/rooms", ListAsync);
        api.MapPost("/rooms", CreateAsync).Mutating();
        api.MapPost("/rooms/join", JoinAsync).Mutating();
        api.MapGet("/rooms/{roomId:guid}", GetAsync);
        api.MapPatch("/rooms/{roomId:guid}", UpdateAsync).Mutating();
        api.MapPut("/rooms/{roomId:guid}/ready", SetReadyAsync).Mutating();
        api.MapPost("/rooms/{roomId:guid}/start", StartAsync).Mutating();
        api.MapDelete("/rooms/{roomId:guid}/players/me", LeaveAsync).Mutating();
        api.MapPost("/rooms/{roomId:guid}/players/{targetUserId:guid}/moderation", ModerateAsync).Mutating();
        api.MapPost("/rooms/{roomId:guid}/voice-token", VoiceAsync).Mutating();
        api.MapPost("/games/{gameId:guid}/actions", ActionAsync).Mutating();
        api.MapPost("/games/{gameId:guid}/votes", VoteAsync).Mutating();
        return endpoints;
    }

    private static RouteHandlerBuilder Mutating(this RouteHandlerBuilder builder) => builder.AddEndpointFilter<AntiforgeryEndpointFilter>().RequireRateLimiting("werewolf");
    private static bool UserId(ClaimsPrincipal principal, out Guid userId) => Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
    private static async Task<IResult> ListAsync(ClaimsPrincipal p, ISender s, HttpContext c, CancellationToken ct) =>
        !UserId(p, out var id) ? WerewolfEndpointResults.InvalidSession(c) : Result(await s.Send(new ListRoomsQuery(id), ct), c);
    private static async Task<IResult> CreateAsync(CreateRoomRequest r, ClaimsPrincipal p, ISender s, HttpContext c, CancellationToken ct) =>
        !UserId(p, out var id) ? WerewolfEndpointResults.InvalidSession(c) : Result(await s.Send(new CreateRoomCommand(id, r.Name, r.Visibility, r.MinimumPlayers, r.MaximumPlayers, r.DeadVoiceEnabled), ct), c, true);
    private static async Task<IResult> JoinAsync(JoinRoomRequest r, ClaimsPrincipal p, ISender s, HttpContext c, CancellationToken ct) =>
        !UserId(p, out var id) ? WerewolfEndpointResults.InvalidSession(c) : Result(await s.Send(new JoinRoomCommand(id, r.Code, r.Spectator), ct), c);
    private static async Task<IResult> GetAsync(Guid roomId, ClaimsPrincipal p, ISender s, HttpContext c, CancellationToken ct) =>
        !UserId(p, out var id) ? WerewolfEndpointResults.InvalidSession(c) : Result(await s.Send(new GetRoomQuery(roomId, id), ct), c);
    private static async Task<IResult> UpdateAsync(Guid roomId, UpdateRoomRequest r, ClaimsPrincipal p, ISender s, HttpContext c, CancellationToken ct) =>
        !UserId(p, out var id) ? WerewolfEndpointResults.InvalidSession(c) : Result(await s.Send(new UpdateRoomCommand(roomId, id, r.ExpectedVersion, r.Name, r.Visibility, r.MinimumPlayers, r.MaximumPlayers, r.DeadVoiceEnabled), ct), c);
    private static async Task<IResult> SetReadyAsync(Guid roomId, SetReadyRequest r, ClaimsPrincipal p, ISender s, HttpContext c, CancellationToken ct) =>
        !UserId(p, out var id) ? WerewolfEndpointResults.InvalidSession(c) : Result(await s.Send(new SetReadyCommand(roomId, id, r.Ready), ct), c);
    private static async Task<IResult> StartAsync(Guid roomId, StartGameRequest r, ClaimsPrincipal p, ISender s, HttpContext c, CancellationToken ct) =>
        !UserId(p, out var id) ? WerewolfEndpointResults.InvalidSession(c) : Result(await s.Send(new StartGameCommand(roomId, id, r.ExpectedVersion), ct), c);
    private static async Task<IResult> LeaveAsync(Guid roomId, ClaimsPrincipal p, ISender s, HttpContext c, CancellationToken ct) =>
        !UserId(p, out var id) ? WerewolfEndpointResults.InvalidSession(c) : Result(await s.Send(new LeaveRoomCommand(roomId, id), ct), c);
    private static async Task<IResult> ModerateAsync(Guid roomId, Guid targetUserId, ModeratePlayerRequest r, ClaimsPrincipal p, ISender s, HttpContext c, CancellationToken ct) =>
        !UserId(p, out var id) ? WerewolfEndpointResults.InvalidSession(c) : Result(await s.Send(new ModeratePlayerCommand(roomId, id, targetUserId, r.Action), ct), c);
    private static async Task<IResult> VoiceAsync(Guid roomId, ClaimsPrincipal p, ISender s, HttpContext c, CancellationToken ct) =>
        !UserId(p, out var id) ? WerewolfEndpointResults.InvalidSession(c) : Result(await s.Send(new GetVoiceAccessCommand(roomId, id), ct), c);
    private static async Task<IResult> ActionAsync(Guid gameId, SubmitActionRequest r, ClaimsPrincipal p, ISender s, HttpContext c, CancellationToken ct) =>
        !UserId(p, out var id) ? WerewolfEndpointResults.InvalidSession(c) : Result(await s.Send(new SubmitActionCommand(gameId, id, r.ClientActionId, r.Kind, r.TargetUserId), ct), c);
    private static async Task<IResult> VoteAsync(Guid gameId, SubmitVoteRequest r, ClaimsPrincipal p, ISender s, HttpContext c, CancellationToken ct) =>
        !UserId(p, out var id) ? WerewolfEndpointResults.InvalidSession(c) : Result(await s.Send(new SubmitVoteCommand(gameId, id, r.ClientActionId, r.TargetUserId), ct), c);
    private static IResult Result<T>(Campfire.Modules.Werewolf.Application.Common.WerewolfResult<T> result, HttpContext context, bool created = false) =>
        result.IsSuccess ? created ? Results.Created("/api/v1/werewolf/rooms", result.Value) : Results.Ok(result.Value) : WerewolfEndpointResults.Failure(result, context);
}
