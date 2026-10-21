using System.Security.Claims;
using Campfire.Modules.Groups.Application.CreateGroup;
using Campfire.Modules.Groups.Application.CreateInvitation;
using Campfire.Modules.Groups.Application.GetGroupMembers;
using Campfire.Modules.Groups.Application.GetInvitations;
using Campfire.Modules.Groups.Application.GetMyGroups;
using Campfire.Modules.Groups.Application.JoinGroup;
using Campfire.Modules.Groups.Application.LeaveGroup;
using Campfire.Modules.Groups.Application.PreviewInvitation;
using Campfire.Modules.Groups.Application.RemoveMember;
using Campfire.Modules.Groups.Application.RenameGroup;
using Campfire.Modules.Groups.Application.RevokeInvitation;
using Campfire.Modules.Groups.Application.TransferOwnership;
using Campfire.Modules.Groups.Presentation.Endpoints;
using Campfire.Modules.Groups.Presentation.Models;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Campfire.Modules.Groups.Presentation;

public static class GroupsEndpoints
{
    public static IEndpointRouteBuilder MapGroupsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var groups = endpoints.MapGroup("/api/v1/groups")
            .WithTags("Groups")
            .RequireAuthorization();

        groups.MapGet("/", GetMyGroupsAsync).WithName("GetMyGroups");
        groups.MapPost("/", CreateGroupAsync)
            .AddEndpointFilter<AntiforgeryEndpointFilter>()
            .WithName("CreateGroup");
        groups.MapPatch("/{groupId:guid}", RenameGroupAsync)
            .AddEndpointFilter<AntiforgeryEndpointFilter>()
            .WithName("RenameGroup");
        groups.MapGet("/{groupId:guid}/members", GetMembersAsync).WithName("GetGroupMembers");
        groups.MapDelete("/{groupId:guid}/members/me", LeaveGroupAsync)
            .AddEndpointFilter<AntiforgeryEndpointFilter>()
            .WithName("LeaveGroup");
        groups.MapDelete("/{groupId:guid}/members/{memberUserId:guid}", RemoveMemberAsync)
            .AddEndpointFilter<AntiforgeryEndpointFilter>()
            .WithName("RemoveGroupMember");
        groups.MapPost("/{groupId:guid}/owner", TransferOwnershipAsync)
            .AddEndpointFilter<AntiforgeryEndpointFilter>()
            .WithName("TransferGroupOwnership");
        groups.MapGet("/{groupId:guid}/invitations", GetInvitationsAsync)
            .WithName("GetGroupInvitations");
        groups.MapPost("/{groupId:guid}/invitations", CreateInvitationAsync)
            .AddEndpointFilter<AntiforgeryEndpointFilter>()
            .RequireRateLimiting("invitations")
            .WithName("CreateGroupInvitation");
        groups.MapDelete("/{groupId:guid}/invitations/{invitationId:guid}", RevokeInvitationAsync)
            .AddEndpointFilter<AntiforgeryEndpointFilter>()
            .WithName("RevokeGroupInvitation");

        var invitations = endpoints.MapGroup("/api/v1/group-invitations")
            .WithTags("Groups")
            .RequireAuthorization()
            .RequireRateLimiting("invitations");
        invitations.MapPost("/preview", PreviewInvitationAsync)
            .AddEndpointFilter<AntiforgeryEndpointFilter>()
            .WithName("PreviewGroupInvitation");
        invitations.MapPost("/join", JoinGroupAsync)
            .AddEndpointFilter<AntiforgeryEndpointFilter>()
            .WithName("JoinGroup");

        return endpoints;
    }

    private static async Task<IResult> GetMyGroupsAsync(
        ClaimsPrincipal principal,
        ISender sender,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(principal, out var userId)) return GroupEndpointResults.InvalidSession(context);
        return Results.Ok(await sender.Send(new GetMyGroupsQuery(userId), cancellationToken));
    }

    private static async Task<IResult> CreateGroupAsync(
        CreateGroupRequest request,
        ClaimsPrincipal principal,
        ISender sender,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(principal, out var userId)) return GroupEndpointResults.InvalidSession(context);
        var result = await sender.Send(
            new CreateGroupCommand(userId, request.Name, context.TraceIdentifier),
            cancellationToken);
        return result.IsSuccess
            ? Results.Created($"/api/v1/groups/{result.Value!.Id}", result.Value)
            : GroupEndpointResults.Failure(result, context);
    }

    private static async Task<IResult> RenameGroupAsync(
        Guid groupId,
        RenameGroupRequest request,
        ClaimsPrincipal principal,
        ISender sender,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(principal, out var userId)) return GroupEndpointResults.InvalidSession(context);
        var result = await sender.Send(
            new RenameGroupCommand(groupId, userId, request.Name, context.TraceIdentifier),
            cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : GroupEndpointResults.Failure(result, context);
    }

    private static async Task<IResult> GetMembersAsync(
        Guid groupId,
        ClaimsPrincipal principal,
        ISender sender,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(principal, out var userId)) return GroupEndpointResults.InvalidSession(context);
        var result = await sender.Send(new GetGroupMembersQuery(groupId, userId), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : GroupEndpointResults.Failure(result, context);
    }

    private static async Task<IResult> RemoveMemberAsync(
        Guid groupId,
        Guid memberUserId,
        ClaimsPrincipal principal,
        ISender sender,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(principal, out var userId)) return GroupEndpointResults.InvalidSession(context);
        var result = await sender.Send(
            new RemoveMemberCommand(groupId, memberUserId, userId, context.TraceIdentifier),
            cancellationToken);
        return result.IsSuccess ? Results.NoContent() : GroupEndpointResults.Failure(result, context);
    }

    private static async Task<IResult> LeaveGroupAsync(
        Guid groupId,
        ClaimsPrincipal principal,
        ISender sender,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(principal, out var userId)) return GroupEndpointResults.InvalidSession(context);
        var result = await sender.Send(
            new LeaveGroupCommand(groupId, userId, context.TraceIdentifier),
            cancellationToken);
        return result.IsSuccess ? Results.NoContent() : GroupEndpointResults.Failure(result, context);
    }

    private static async Task<IResult> TransferOwnershipAsync(
        Guid groupId,
        TransferOwnershipRequest request,
        ClaimsPrincipal principal,
        ISender sender,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(principal, out var userId)) return GroupEndpointResults.InvalidSession(context);
        var result = await sender.Send(
            new TransferOwnershipCommand(
                groupId,
                request.NewOwnerUserId,
                request.ExpectedVersion,
                userId,
                context.TraceIdentifier),
            cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : GroupEndpointResults.Failure(result, context);
    }

    private static async Task<IResult> GetInvitationsAsync(
        Guid groupId,
        ClaimsPrincipal principal,
        ISender sender,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(principal, out var userId)) return GroupEndpointResults.InvalidSession(context);
        var result = await sender.Send(new GetInvitationsQuery(groupId, userId), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : GroupEndpointResults.Failure(result, context);
    }

    private static async Task<IResult> CreateInvitationAsync(
        Guid groupId,
        CreateInvitationRequest request,
        ClaimsPrincipal principal,
        ISender sender,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(principal, out var userId)) return GroupEndpointResults.InvalidSession(context);
        var result = await sender.Send(
            new CreateInvitationCommand(
                groupId,
                userId,
                request.ExpiresInHours,
                context.TraceIdentifier),
            cancellationToken);
        return result.IsSuccess
            ? Results.Created($"/api/v1/groups/{groupId}/invitations/{result.Value!.Id}", result.Value)
            : GroupEndpointResults.Failure(result, context);
    }

    private static async Task<IResult> RevokeInvitationAsync(
        Guid groupId,
        Guid invitationId,
        ClaimsPrincipal principal,
        ISender sender,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(principal, out var userId)) return GroupEndpointResults.InvalidSession(context);
        var result = await sender.Send(
            new RevokeInvitationCommand(
                groupId,
                invitationId,
                userId,
                context.TraceIdentifier),
            cancellationToken);
        return result.IsSuccess ? Results.NoContent() : GroupEndpointResults.Failure(result, context);
    }

    private static async Task<IResult> PreviewInvitationAsync(
        InvitationTokenRequest request,
        ClaimsPrincipal principal,
        ISender sender,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(principal, out var userId)) return GroupEndpointResults.InvalidSession(context);
        var result = await sender.Send(new PreviewInvitationCommand(request.Token, userId), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : GroupEndpointResults.Failure(result, context);
    }

    private static async Task<IResult> JoinGroupAsync(
        InvitationTokenRequest request,
        ClaimsPrincipal principal,
        ISender sender,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(principal, out var userId)) return GroupEndpointResults.InvalidSession(context);
        var result = await sender.Send(
            new JoinGroupCommand(request.Token, userId, context.TraceIdentifier),
            cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : GroupEndpointResults.Failure(result, context);
    }

    private static bool TryGetUserId(ClaimsPrincipal principal, out Guid userId) =>
        Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
}
