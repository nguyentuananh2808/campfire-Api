using System.Security.Claims;
using Campfire.Modules.Events.Application.ChangeEventStatus;
using Campfire.Modules.Events.Application.CreateEvent;
using Campfire.Modules.Events.Application.GetEvent;
using Campfire.Modules.Events.Application.ListEvents;
using Campfire.Modules.Events.Application.SetRsvp;
using Campfire.Modules.Events.Application.UpdateEvent;
using Campfire.Modules.Events.Presentation.Endpoints;
using Campfire.Modules.Events.Presentation.Models;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Campfire.Modules.Events.Presentation;

public static class EventsEndpoints
{
    public static IEndpointRouteBuilder MapEventsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var events = endpoints.MapGroup("/api/v1/groups/{groupId:guid}/events")
            .WithTags("Events").RequireAuthorization();
        events.MapGet("/", ListAsync).WithName("ListGroupEvents");
        events.MapPost("/", CreateAsync).AddEndpointFilter<AntiforgeryEndpointFilter>().WithName("CreateGroupEvent");
        events.MapGet("/{eventId:guid}", GetAsync).WithName("GetGroupEvent");
        events.MapPatch("/{eventId:guid}", UpdateAsync).AddEndpointFilter<AntiforgeryEndpointFilter>().WithName("UpdateGroupEvent");
        events.MapPost("/{eventId:guid}/status", ChangeStatusAsync).AddEndpointFilter<AntiforgeryEndpointFilter>().WithName("ChangeGroupEventStatus");
        events.MapPut("/{eventId:guid}/rsvp", SetRsvpAsync).AddEndpointFilter<AntiforgeryEndpointFilter>().WithName("SetEventRsvp");
        return endpoints;
    }

    private static async Task<IResult> ListAsync(Guid groupId, int? limit, ClaimsPrincipal principal,
        ISender sender, HttpContext context, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(principal, out var userId)) return EventEndpointResults.InvalidSession(context);
        var result = await sender.Send(new ListEventsQuery(groupId, userId, limit ?? 50), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : EventEndpointResults.Failure(result, context);
    }

    private static async Task<IResult> CreateAsync(Guid groupId, CreateEventRequest request,
        ClaimsPrincipal principal, ISender sender, HttpContext context, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(principal, out var userId)) return EventEndpointResults.InvalidSession(context);
        var result = await sender.Send(new CreateEventCommand(groupId, userId, request.Title,
            request.Description, request.StartsAtUtc, request.Location), cancellationToken);
        return result.IsSuccess ? Results.Created($"/api/v1/groups/{groupId}/events/{result.Value!.Id}", result.Value)
            : EventEndpointResults.Failure(result, context);
    }

    private static async Task<IResult> GetAsync(Guid groupId, Guid eventId, ClaimsPrincipal principal,
        ISender sender, HttpContext context, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(principal, out var userId)) return EventEndpointResults.InvalidSession(context);
        var result = await sender.Send(new GetEventQuery(groupId, eventId, userId), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : EventEndpointResults.Failure(result, context);
    }

    private static async Task<IResult> UpdateAsync(Guid groupId, Guid eventId, UpdateEventRequest request,
        ClaimsPrincipal principal, ISender sender, HttpContext context, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(principal, out var userId)) return EventEndpointResults.InvalidSession(context);
        var result = await sender.Send(new UpdateEventCommand(groupId, eventId, userId, request.ExpectedVersion,
            request.Title, request.Description, request.StartsAtUtc, request.Location), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : EventEndpointResults.Failure(result, context);
    }

    private static async Task<IResult> ChangeStatusAsync(Guid groupId, Guid eventId, ChangeEventStatusRequest request,
        ClaimsPrincipal principal, ISender sender, HttpContext context, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(principal, out var userId)) return EventEndpointResults.InvalidSession(context);
        var result = await sender.Send(new ChangeEventStatusCommand(groupId, eventId, userId,
            request.ExpectedVersion, request.Status, context.TraceIdentifier), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : EventEndpointResults.Failure(result, context);
    }

    private static async Task<IResult> SetRsvpAsync(Guid groupId, Guid eventId, SetRsvpRequest request,
        ClaimsPrincipal principal, ISender sender, HttpContext context, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(principal, out var userId)) return EventEndpointResults.InvalidSession(context);
        var result = await sender.Send(new SetRsvpCommand(groupId, eventId, userId, request.Response), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : EventEndpointResults.Failure(result, context);
    }

    private static bool TryGetUserId(ClaimsPrincipal principal, out Guid userId) =>
        Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
}
