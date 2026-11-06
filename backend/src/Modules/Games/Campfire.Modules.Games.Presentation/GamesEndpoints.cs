using System.Security.Claims;
using Campfire.Modules.Games.Application.CreateGame;
using Campfire.Modules.Games.Application.GetGame;
using Campfire.Modules.Games.Application.ListGames;
using Campfire.Modules.Games.Application.PlayMove;
using Campfire.Modules.Games.Presentation.Endpoints;
using Campfire.Modules.Games.Presentation.Models;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Campfire.Modules.Games.Presentation;

public static class GamesEndpoints
{
    public static IEndpointRouteBuilder MapGamesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var games = endpoints.MapGroup("/api/v1/games").WithTags("Games").RequireAuthorization();
        games.MapGet("/", ListAsync).WithName("ListMyGames");
        games.MapPost("/", CreateAsync).AddEndpointFilter<AntiforgeryEndpointFilter>().WithName("CreateGame");
        games.MapGet("/{gameId:guid}", GetAsync).WithName("GetGame");
        games.MapPost("/{gameId:guid}/moves", PlayAsync).AddEndpointFilter<AntiforgeryEndpointFilter>().WithName("PlayGameMove");
        return endpoints;
    }

    private static async Task<IResult> ListAsync(ClaimsPrincipal principal, ISender sender, HttpContext context,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(principal, out var userId)) return GameEndpointResults.InvalidSession(context);
        var result = await sender.Send(new ListGamesQuery(userId), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : GameEndpointResults.Failure(result, context);
    }

    private static async Task<IResult> CreateAsync(CreateGameRequest request, ClaimsPrincipal principal, ISender sender,
        HttpContext context, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(principal, out var userId)) return GameEndpointResults.InvalidSession(context);
        var result = await sender.Send(new CreateGameCommand(request.GroupId, userId, request.OpponentUserId), cancellationToken);
        return result.IsSuccess ? Results.Created($"/api/v1/games/{result.Value!.Id}", result.Value) : GameEndpointResults.Failure(result, context);
    }

    private static async Task<IResult> GetAsync(Guid gameId, ClaimsPrincipal principal, ISender sender,
        HttpContext context, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(principal, out var userId)) return GameEndpointResults.InvalidSession(context);
        var result = await sender.Send(new GetGameQuery(gameId, userId), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : GameEndpointResults.Failure(result, context);
    }

    private static async Task<IResult> PlayAsync(Guid gameId, PlayMoveRequest request, ClaimsPrincipal principal,
        ISender sender, HttpContext context, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(principal, out var userId)) return GameEndpointResults.InvalidSession(context);
        var result = await sender.Send(new PlayMoveCommand(gameId, userId, request.Cell, request.ExpectedVersion), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : GameEndpointResults.Failure(result, context);
    }

    private static bool TryGetUserId(ClaimsPrincipal principal, out Guid userId) =>
        Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
}
