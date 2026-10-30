using System.Security.Claims;
using Campfire.Modules.Chat.Application.GetMessages;
using Campfire.Modules.Chat.Application.EditMessage;
using Campfire.Modules.Chat.Application.SendMessage;
using Campfire.Modules.Chat.Application.SetReaction;
using Campfire.Modules.Chat.Presentation.Endpoints;
using Campfire.Modules.Chat.Presentation.Models;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Campfire.Modules.Chat.Presentation;

public static class ChatEndpoints
{
    public static IEndpointRouteBuilder MapChatEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var messages = endpoints.MapGroup("/api/v1/groups/{groupId:guid}/messages")
            .WithTags("Chat").RequireAuthorization();
        messages.MapGet("/", GetMessagesAsync).WithName("GetGroupMessages");
        messages.MapPost("/", SendMessageAsync).AddEndpointFilter<AntiforgeryEndpointFilter>()
            .RequireRateLimiting("chat").WithName("SendGroupMessage");
        messages.MapPut("/{messageId:guid}/reaction", SetReactionAsync)
            .AddEndpointFilter<AntiforgeryEndpointFilter>()
            .RequireRateLimiting("chat").WithName("SetGroupMessageReaction");
        messages.MapPut("/{messageId:guid}", EditMessageAsync)
            .AddEndpointFilter<AntiforgeryEndpointFilter>()
            .RequireRateLimiting("chat").WithName("EditGroupMessage");
        return endpoints;
    }

    private static async Task<IResult> GetMessagesAsync(Guid groupId, string? before, string? after, int? limit,
        ClaimsPrincipal principal, ISender sender, HttpContext context, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(principal, out var userId)) return ChatEndpointResults.InvalidSession(context);
        var result = await sender.Send(new GetMessagesQuery(groupId, userId, before, after, limit ?? 50), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ChatEndpointResults.Failure(result, context);
    }

    private static async Task<IResult> SendMessageAsync(Guid groupId, SendMessageRequest request,
        ClaimsPrincipal principal, ISender sender, HttpContext context, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(principal, out var userId)) return ChatEndpointResults.InvalidSession(context);
        var result = await sender.Send(new SendMessageCommand(
            groupId,
            userId,
            request.ClientMessageId,
            request.Kind ?? "Text",
            request.Content,
            request.ReplyToMessageId), cancellationToken);
        if (!result.IsSuccess) return ChatEndpointResults.Failure(result, context);
        return result.Value!.WasCreated
            ? Results.Created($"/api/v1/groups/{groupId}/messages/{result.Value.Message.Id}", result.Value.Message)
            : Results.Ok(result.Value.Message);
    }

    private static async Task<IResult> SetReactionAsync(Guid groupId, Guid messageId, SetMessageReactionRequest request,
        ClaimsPrincipal principal, ISender sender, HttpContext context, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(principal, out var userId)) return ChatEndpointResults.InvalidSession(context);
        var result = await sender.Send(new SetMessageReactionCommand(groupId, messageId, userId, request.Reaction), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ChatEndpointResults.Failure(result, context);
    }

    private static async Task<IResult> EditMessageAsync(Guid groupId, Guid messageId, EditMessageRequest request,
        ClaimsPrincipal principal, ISender sender, HttpContext context, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(principal, out var userId)) return ChatEndpointResults.InvalidSession(context);
        var result = await sender.Send(new EditMessageCommand(groupId, messageId, userId, request.Content, request.ExpectedVersion), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ChatEndpointResults.Failure(result, context);
    }

    private static bool TryGetUserId(ClaimsPrincipal principal, out Guid userId) =>
        Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
}
