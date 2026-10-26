using Campfire.Api.Middleware;
using Campfire.Modules.Chat.Infrastructure;
using Campfire.Modules.Chat.Presentation;
using Campfire.Modules.Events.Presentation;
using Campfire.Modules.Groups.Presentation;
using Campfire.Modules.Identity.Presentation;
using Campfire.Modules.Learning.Presentation;
using Campfire.Modules.StrangerChat.Infrastructure;
using Campfire.Modules.StrangerChat.Presentation;
using Campfire.Modules.Werewolf.Infrastructure;
using Campfire.Modules.Werewolf.Presentation;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;

namespace Campfire.Api.Composition;

public static class WebApplicationExtensions
{
    public static WebApplication MapCampfireApi(this WebApplication app)
    {
        ValidateProductionSecurity(app);
        app.UseForwardedHeaders();
        app.UseExceptionHandler();
        app.UseMiddleware<SecurityHeadersMiddleware>();
        app.UseSerilogRequestLogging(options => options.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms");
        if (app.Configuration.GetValue("Security:RequireHttps", true))
        {
            if (!app.Environment.IsDevelopment())
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
        }
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseAntiforgery();
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi("/api/openapi/{documentName}.json");
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/api/openapi/v1.json", "Campfire API v1");
                options.DocumentTitle = "Campfire API";
            });
        }
        app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
        app.MapHealthChecks("/health/ready");
        app.MapGet("/api/v1", () => Results.Ok(new { name = "Campfire API", version = "v1" }))
            .ExcludeFromDescription();
        app.MapIdentityEndpoints();
        app.MapGroupsEndpoints();
        app.MapChatEndpoints();
        app.MapChatHub();
        app.MapEventsEndpoints();
        app.MapWerewolfEndpoints();
        app.MapWerewolfHub();
        app.MapStrangerChatEndpoints();
        app.MapStrangerChatHub();
        app.MapLearningEndpoints();
        return app;
    }

    private static void ValidateProductionSecurity(WebApplication app)
    {
        if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
        {
            return;
        }

        if (!app.Configuration.GetValue("Security:RequireHttps", true))
        {
            throw new InvalidOperationException("Production requires Security:RequireHttps=true.");
        }

        var allowedHosts = app.Configuration["AllowedHosts"];
        if (string.IsNullOrWhiteSpace(allowedHosts) ||
            allowedHosts.Split(';', StringSplitOptions.TrimEntries).Contains("*"))
        {
            throw new InvalidOperationException("Production requires an explicit AllowedHosts value.");
        }

        if (string.IsNullOrWhiteSpace(app.Configuration["DataProtection:KeysPath"]))
        {
            throw new InvalidOperationException("Production requires a persistent DataProtection:KeysPath.");
        }
    }
}
