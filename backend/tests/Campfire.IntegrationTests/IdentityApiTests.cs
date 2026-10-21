using System.Net;
using System.Net.Http.Json;
using Campfire.Modules.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Campfire.IntegrationTests;

public sealed class IdentityApiTests(CampfireApiFactory factory) : IClassFixture<CampfireApiFactory>
{
    [Fact]
    public async Task Register_change_password_logout_and_session_revocation_work_end_to_end()
    {
        using var firstSession = factory.CreateClient();
        var email = $"friend-{Guid.NewGuid():N}@example.com";

        await PrimeAntiforgeryAsync(firstSession);
        var registration = await firstSession.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email,
            password = "FirstPassword!1",
            displayName = "Campfire Friend",
            role = "Administrator"
        });
        Assert.Equal(HttpStatusCode.Created, registration.StatusCode);

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
            Assert.Equal(0, await dbContext.UserRoles.CountAsync());
        }

        Assert.Equal(HttpStatusCode.OK, (await firstSession.GetAsync("/api/v1/auth/me")).StatusCode);

        using var secondSession = factory.CreateClient();
        await PrimeAntiforgeryAsync(secondSession);
        var login = await secondSession.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email,
            password = "FirstPassword!1"
        });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        await PrimeAntiforgeryAsync(firstSession);
        var changed = await firstSession.PostAsJsonAsync("/api/v1/auth/change-password", new
        {
            currentPassword = "FirstPassword!1",
            newPassword = "SecondPassword!2"
        });
        Assert.Equal(HttpStatusCode.NoContent, changed.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await firstSession.GetAsync("/api/v1/auth/me")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await secondSession.GetAsync("/api/v1/auth/me")).StatusCode);

        await PrimeAntiforgeryAsync(firstSession);
        Assert.Equal(HttpStatusCode.NoContent, (await firstSession.PostAsJsonAsync("/api/v1/auth/logout", new { })).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await firstSession.GetAsync("/api/v1/auth/me")).StatusCode);
    }

    [Fact]
    public async Task Mutation_without_antiforgery_token_is_rejected()
    {
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "friend@example.com",
            password = "StrongPassword!1"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static async Task PrimeAntiforgeryAsync(HttpClient client)
    {
        var response = await client.GetAsync("/api/v1/auth/antiforgery");
        response.EnsureSuccessStatusCode();
        var cookie = response.Headers.GetValues("Set-Cookie")
            .Single(value => value.StartsWith("XSRF-TOKEN=", StringComparison.Ordinal));
        var token = Uri.UnescapeDataString(cookie.Split(';', 2)[0]["XSRF-TOKEN=".Length..]);
        client.DefaultRequestHeaders.Remove("X-XSRF-TOKEN");
        client.DefaultRequestHeaders.Add("X-XSRF-TOKEN", token);
    }
}
