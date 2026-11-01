using System.Security.Claims;
using Campfire.Modules.Chat.Application.GetMessages;
using Campfire.Modules.Chat.Application.EditMessage;
using Campfire.Modules.Chat.Application.SendMessage;
using Campfire.Modules.Chat.Application.SetReaction;
using Campfire.Modules.Chat.Application.SendImage;
using Campfire.Modules.Chat.Application.GetImage;
using Campfire.Modules.Chat.Presentation.Endpoints;
using Campfire.Modules.Chat.Presentation.Models;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;

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
        messages.MapPost("/image", SendImageAsync).AddEndpointFilter<AntiforgeryEndpointFilter>()
            .RequireRateLimiting("chat").WithMetadata(new RequestSizeLimitAttribute(1_200_000)).WithName("SendGroupImage");
        messages.MapGet("/{messageId:guid}/image", GetImageAsync).WithName("GetGroupImage");
        messages.MapPut("/{messageId:guid}/reaction", SetReactionAsync)
            .AddEndpointFilter<AntiforgeryEndpointFilter>()
            .RequireRateLimiting("chat").WithName("SetGroupMessageReaction");
        messages.MapPut("/{messageId:guid}", EditMessageAsync)
            .AddEndpointFilter<AntiforgeryEndpointFilter>()
            .RequireRateLimiting("chat").WithName("EditGroupMessage");
        return endpoints;
    }

    private static async Task<IResult> SendImageAsync(Guid groupId, ClaimsPrincipal principal, ISender sender,
        HttpContext context, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(principal, out var userId)) return ChatEndpointResults.InvalidSession(context);
        if (!context.Request.HasFormContentType) return Results.BadRequest();
        var form = await context.Request.ReadFormAsync(cancellationToken);
        var file = form.Files.GetFile("image");
        if (file is null || file.Length is <= 0 or > 1_048_576) return Results.Problem(statusCode: 400, title: "Ảnh phải nhỏ hơn 1 MiB.");
        if (!Guid.TryParse(form["clientMessageId"], out var clientId)) return Results.BadRequest();
        Guid? replyId = Guid.TryParse(form["replyToMessageId"], out var parsedReply) ? parsedReply : null;
        await using var stream = file.OpenReadStream();
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory, cancellationToken);
        var result = await sender.Send(new SendImageCommand(groupId, userId, clientId, memory.ToArray(), file.ContentType, replyId), cancellationToken);
        if (!result.IsSuccess) return ChatEndpointResults.Failure(result, context);
        return result.Value!.WasCreated ? Results.Created($"/api/v1/groups/{groupId}/messages/{result.Value.Message.Id}", result.Value.Message) : Results.Ok(result.Value.Message);
    }

    private static async Task<IResult> GetImageAsync(Guid groupId, Guid messageId, ClaimsPrincipal principal,
        ISender sender, HttpContext context, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(principal, out var userId)) return ChatEndpointResults.InvalidSession(context);
        var result = await sender.Send(new GetImageQuery(groupId, messageId, userId), cancellationToken);
        return result.IsSuccess
            ? Results.File(result.Value!.Bytes, result.Value.ContentType, enableRangeProcessing: false)
            : ChatEndpointResults.Failure(result, context);
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
