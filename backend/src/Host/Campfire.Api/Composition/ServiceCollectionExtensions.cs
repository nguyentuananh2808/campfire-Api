using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.RateLimiting;
using Campfire.Modules.Chat.Infrastructure;
using Campfire.Modules.Chat.Presentation;
using Campfire.Modules.Events.Infrastructure;
using Campfire.Modules.Events.Presentation;
using Campfire.Modules.Groups.Infrastructure;
using Campfire.Modules.Groups.Presentation;
using Campfire.Modules.Identity.Infrastructure;
using Campfire.Modules.Identity.Presentation;
using Campfire.Modules.StrangerChat.Infrastructure;
using Campfire.Modules.StrangerChat.Presentation;
using Campfire.Modules.Werewolf.Infrastructure;
using Campfire.Modules.Werewolf.Presentation;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;

namespace Campfire.Api.Composition;

public static class ServiceCollectionExtensions
{
    private static readonly byte[] RateLimitKey = RandomNumberGenerator.GetBytes(32);

    public static IServiceCollection AddCampfireApi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
        {
            context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
            context.ProblemDetails.Extensions.TryAdd("code", "unexpected_error");
        });
        services.AddExceptionHandler<IdentityValidationExceptionHandler>();
        services.AddExceptionHandler<GroupsValidationExceptionHandler>();
        services.AddExceptionHandler<ChatValidationExceptionHandler>();
        services.AddExceptionHandler<EventsValidationExceptionHandler>();
        services.AddExceptionHandler<WerewolfValidationExceptionHandler>();
        services.AddExceptionHandler<StrangerChatValidationExceptionHandler>();
        services.AddExceptionHandler<AntiforgeryExceptionHandler>();
        services.AddExceptionHandler<UnexpectedExceptionHandler>();
        services.AddOpenApi();
        var dataProtection = services.AddDataProtection().SetApplicationName("Campfire");
        var keysPath = configuration["DataProtection:KeysPath"];
        if (!string.IsNullOrWhiteSpace(keysPath))
        {
            dataProtection.PersistKeysToFileSystem(new DirectoryInfo(Path.GetFullPath(keysPath)));
        }
        services.AddHealthChecks();
        var requireHttps = configuration.GetValue("Security:RequireHttps", true);
        services.AddIdentityModule(configuration, requireHttps);
        services.AddGroupsModule(configuration);
        services.AddChatModule(configuration);
        services.AddEventsModule(configuration);
        services.AddWerewolfModule(configuration);
        services.AddStrangerChatModule(configuration);
        services.AddAntiforgery(options =>
        {
            options.Cookie.Name = "Campfire.Antiforgery";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = requireHttps
                ? CookieSecurePolicy.Always
                : CookieSecurePolicy.SameAsRequest;
            options.HeaderName = "X-XSRF-TOKEN";
        });
        var strangerSessionPermitLimit = configuration.GetValue(
            "RateLimits:StrangerSessionPermitLimit",
            5);
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.ContentType = "application/problem+json";
                await Results.Problem(
                    statusCode: StatusCodes.Status429TooManyRequests,
                    title: "Bạn thao tác quá nhanh.",
                    detail: "Vui lòng chờ một lát rồi thử lại.",
                    extensions: new Dictionary<string, object?>
                    {
                        ["code"] = "rate_limit_exceeded",
                        ["traceId"] = context.HttpContext.TraceIdentifier
                    }).ExecuteAsync(context.HttpContext);
            };
            options.AddPolicy("auth", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    NetworkPartition(context),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));
            options.AddPolicy("invitations", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    NetworkPartition(context),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 20,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));
            options.AddPolicy("chat", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                        ?? NetworkPartition(context),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 30,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));
            options.AddPolicy("werewolf", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                        ?? NetworkPartition(context),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 60,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));
            options.AddPolicy("stranger-session", context => RateLimitPartition.GetFixedWindowLimiter(NetworkPartition(context), _ => new FixedWindowRateLimiterOptions { PermitLimit = strangerSessionPermitLimit, Window = TimeSpan.FromMinutes(10), QueueLimit = 0 }));
            options.AddPolicy("stranger-match", context => RateLimitPartition.GetFixedWindowLimiter(GuestPartition(context), _ => new FixedWindowRateLimiterOptions { PermitLimit = 20, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
            options.AddPolicy("stranger-message", context => RateLimitPartition.GetTokenBucketLimiter(GuestPartition(context), _ => new TokenBucketRateLimiterOptions { TokenLimit = 20, TokensPerPeriod = 10, ReplenishmentPeriod = TimeSpan.FromSeconds(10), AutoReplenishment = true, QueueLimit = 0 }));
            options.AddPolicy("stranger-report", context => RateLimitPartition.GetFixedWindowLimiter(GuestPartition(context), _ => new FixedWindowRateLimiterOptions { PermitLimit = 5, Window = TimeSpan.FromHours(1), QueueLimit = 0 }));
        });
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.ForwardLimit = 1;

            foreach (var value in configuration.GetSection("ReverseProxy:KnownProxies").Get<string[]>() ?? [])
            {
                if (!IPAddress.TryParse(value, out var address))
                {
                    throw new InvalidOperationException($"ReverseProxy:KnownProxies contains an invalid IP address: '{value}'.");
                }

                options.KnownProxies.Add(address);
            }
        });
        return services;
    }

    private static string NetworkPartition(HttpContext context)
    {
        var address = context.Connection.RemoteIpAddress?.GetAddressBytes() ?? [];
        return $"network:{Convert.ToHexString(HMACSHA256.HashData(RateLimitKey, address))}";
    }

    private static string GuestPartition(HttpContext context)
    {
        if (context.Request.Cookies.TryGetValue("Campfire.Guest", out var token) &&
            !string.IsNullOrWhiteSpace(token))
        {
            return $"guest:{Convert.ToHexString(HMACSHA256.HashData(RateLimitKey, Encoding.UTF8.GetBytes(token)))}";
        }

        return NetworkPartition(context);
    }
}
