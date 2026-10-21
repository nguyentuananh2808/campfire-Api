using System.Security.Claims;
using Campfire.Modules.Identity.Application.ChangePassword;
using Campfire.Modules.Identity.Application.GetCurrentUser;
using Campfire.Modules.Identity.Application.Login;
using Campfire.Modules.Identity.Application.Logout;
using Campfire.Modules.Identity.Application.Register;
using Campfire.Modules.Identity.Presentation.Endpoints;
using Campfire.Modules.Identity.Presentation.Models;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Campfire.Modules.Identity.Presentation;

public static class IdentityEndpoints
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/auth").WithTags("Identity");

        group.MapGet("/antiforgery", IssueAntiforgeryToken)
            .WithName("IssueAntiforgeryToken");
        group.MapPost("/register", RegisterAsync)
            .AddEndpointFilter<AntiforgeryEndpointFilter>()
            .RequireRateLimiting("auth")
            .WithName("Register");
        group.MapPost("/login", LoginAsync)
            .AddEndpointFilter<AntiforgeryEndpointFilter>()
            .RequireRateLimiting("auth")
            .WithName("Login");
        group.MapGet("/me", GetCurrentUserAsync)
            .RequireAuthorization()
            .WithName("GetCurrentUser");
        group.MapPost("/change-password", ChangePasswordAsync)
            .RequireAuthorization()
            .AddEndpointFilter<AntiforgeryEndpointFilter>()
            .RequireRateLimiting("auth")
            .WithName("ChangePassword");
        group.MapPost("/logout", LogoutAsync)
            .RequireAuthorization()
            .AddEndpointFilter<AntiforgeryEndpointFilter>()
            .WithName("Logout");

        return endpoints;
    }

    private static IResult IssueAntiforgeryToken(HttpContext context, IAntiforgery antiforgery)
    {
        var tokens = antiforgery.GetAndStoreTokens(context);
        context.Response.Cookies.Append(
            "XSRF-TOKEN",
            tokens.RequestToken!,
            new CookieOptions
            {
                HttpOnly = false,
                IsEssential = true,
                Path = "/",
                SameSite = SameSiteMode.Lax,
                Secure = context.Request.IsHttps
            });
        return Results.NoContent();
    }

    private static async Task<IResult> RegisterAsync(
        RegisterRequest request,
        ISender sender,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new RegisterCommand(request.Email, request.Password, request.DisplayName),
            cancellationToken);
        return result.IsSuccess
            ? Results.Created("/api/v1/auth/me", result.Value)
            : IdentityEndpointResults.Failure(result, context);
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        ISender sender,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new LoginCommand(request.Email, request.Password), cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : IdentityEndpointResults.Failure(result, context);
    }

    private static async Task<IResult> GetCurrentUserAsync(
        ClaimsPrincipal principal,
        ISender sender,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(principal, out var userId))
        {
            return IdentityEndpointResults.InvalidSession(context);
        }

        var result = await sender.Send(new GetCurrentUserQuery(userId), cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : IdentityEndpointResults.Failure(result, context);
    }

    private static async Task<IResult> ChangePasswordAsync(
        ChangePasswordRequest request,
        ClaimsPrincipal principal,
        ISender sender,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(principal, out var userId))
        {
            return IdentityEndpointResults.InvalidSession(context);
        }

        var result = await sender.Send(
            new ChangePasswordCommand(userId, request.CurrentPassword, request.NewPassword),
            cancellationToken);
        return result.IsSuccess
            ? Results.NoContent()
            : IdentityEndpointResults.Failure(result, context);
    }

    private static async Task<IResult> LogoutAsync(
        ISender sender,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new LogoutCommand(), cancellationToken);
        return result.IsSuccess
            ? Results.NoContent()
            : IdentityEndpointResults.Failure(result, context);
    }

    private static bool TryGetUserId(ClaimsPrincipal principal, out Guid userId) =>
        Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
}
