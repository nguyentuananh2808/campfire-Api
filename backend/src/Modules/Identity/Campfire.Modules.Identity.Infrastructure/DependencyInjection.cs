using Campfire.Modules.Identity.Application;
using Campfire.Modules.Identity.Application.Abstractions;
using Campfire.Modules.Identity.Contracts;
using Campfire.Modules.Identity.Infrastructure.Authentication;
using Campfire.Modules.Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Campfire.Modules.Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityModule(
        this IServiceCollection services,
        IConfiguration configuration,
        bool requireHttps)
    {
        var connectionString = configuration.GetConnectionString("Campfire")
            ?? throw new InvalidOperationException("Connection string 'Campfire' is not configured.");

        services.AddIdentityApplication();
        services.AddDbContext<IdentityDbContext>(options => options.UseNpgsql(
            connectionString,
            postgres => postgres.MigrationsHistoryTable(
                "__EFMigrationsHistory",
                IdentityDbContext.Schema)));

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 12;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<IdentityDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
                options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
            })
            .AddIdentityCookies();
        services.AddAuthorization();
        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = "Campfire.Auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = requireHttps
                ? CookieSecurePolicy.Always
                : CookieSecurePolicy.SameAsRequest;
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.SlidingExpiration = true;
            options.Events.OnRedirectToLogin = context => WriteAuthenticationProblemAsync(
                context,
                StatusCodes.Status401Unauthorized,
                "authentication_required",
                "Authentication is required.");
            options.Events.OnRedirectToAccessDenied = context => WriteAuthenticationProblemAsync(
                context,
                StatusCodes.Status403Forbidden,
                "access_denied",
                "Access is denied.");
        });
        services.Configure<SecurityStampValidatorOptions>(options =>
            options.ValidationInterval = TimeSpan.Zero);
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IIdentityService, AspNetCoreIdentityService>();
        services.AddScoped<IUserProfileReader, IdentityUserProfileReader>();
        services.AddHealthChecks().AddDbContextCheck<IdentityDbContext>("identity-database");
        return services;
    }

    private static Task WriteAuthenticationProblemAsync(
        RedirectContext<CookieAuthenticationOptions> context,
        int statusCode,
        string code,
        string title)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = $"https://httpstatuses.com/{statusCode}",
            Instance = context.Request.Path
        };
        problem.Extensions["code"] = code;
        problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

        return context.Response.WriteAsJsonAsync(
            problem,
            cancellationToken: context.HttpContext.RequestAborted);
    }
}
