using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Campfire.Modules.StrangerChat.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Campfire.IntegrationTests;

public sealed class StrangerChatApiTests(CampfireApiFactory factory) : IClassFixture<CampfireApiFactory>
{
    [Fact]
    public async Task Matchmaking_requires_mutually_compatible_gender_preferences()
    {
        using var womanLookingForMan = factory.CreateClient();
        using var womanLookingForWoman = factory.CreateClient();
        using var manLookingForWoman = factory.CreateClient();

        await CreateSessionAsync(womanLookingForMan, "Bạn nữ A", "Female", "Male");
        await CreateSessionAsync(womanLookingForWoman, "Bạn nữ B", "Female", "Female");
        await CreateSessionAsync(manLookingForWoman, "Bạn nam C", "Male", "Female");

        Assert.Equal("waiting", await QueueAndReadStatusAsync(womanLookingForMan));
        Assert.Equal("waiting", await QueueAndReadStatusAsync(womanLookingForWoman));
        Assert.Equal("matched", await QueueAndReadStatusAsync(manLookingForWoman));

        var firstConversation = await womanLookingForMan.GetAsync("/api/v1/stranger/conversation");
        Assert.Equal(HttpStatusCode.OK, firstConversation.StatusCode);
        Assert.Contains("Bạn nam C", await firstConversation.Content.ReadAsStringAsync(), StringComparison.Ordinal);

        Assert.Equal(HttpStatusCode.NotFound, (await womanLookingForWoman.GetAsync("/api/v1/stranger/conversation")).StatusCode);
    }

    [Fact]
    public async Task Session_rejects_unknown_gender_values()
    {
        using var client = factory.CreateClient();
        await PrimeAntiforgeryAsync(client);

        var response = await client.PostAsJsonAsync("/api/v1/stranger/session", new
        {
            nickname = "Người thử",
            gender = "Unknown",
            genderPreference = "Any",
            confirmsAdult = true
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Guest_mutation_requires_antiforgery_and_session_metadata_is_private()
    {
        using var client = factory.CreateClient();
        var rejected = await client.PostAsJsonAsync("/api/v1/stranger/session", new
        {
            nickname = "Người thử",
            gender = "Other",
            genderPreference = "Any",
            confirmsAdult = true
        });

        Assert.Equal(HttpStatusCode.BadRequest, rejected.StatusCode);
        Assert.Equal(
            "antiforgery_validation_failed",
            (await rejected.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());

        await PrimeAntiforgeryAsync(client);
        var created = await client.PostAsJsonAsync("/api/v1/stranger/session", new
        {
            nickname = "Người thử",
            gender = "Other",
            genderPreference = "Any",
            confirmsAdult = true
        });

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.Equal("no-store", created.Headers.CacheControl?.ToString());
        Assert.Equal("DENY", created.Headers.GetValues("X-Frame-Options").Single());
        Assert.Equal("nosniff", created.Headers.GetValues("X-Content-Type-Options").Single());

        var metadata = await created.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(metadata.TryGetProperty("id", out _));
        Assert.False(metadata.TryGetProperty("rawToken", out _));
    }

    [Fact]
    public async Task Report_is_idempotent_and_ends_the_active_conversation()
    {
        using var first = factory.CreateClient();
        using var second = factory.CreateClient();
        await CreateSessionAsync(first, "Người A", "Other", "Any");
        await CreateSessionAsync(second, "Người B", "Other", "Any");

        Assert.Equal("waiting", await QueueAndReadStatusAsync(first));
        var match = await second.PostAsJsonAsync("/api/v1/stranger/matchmaking", new { });
        match.EnsureSuccessStatusCode();
        var matchBody = await match.Content.ReadFromJsonAsync<JsonElement>();
        var conversationId = matchBody.GetProperty("conversation").GetProperty("id").GetGuid();

        var firstReport = await second.PostAsJsonAsync(
            $"/api/v1/stranger/conversations/{conversationId}/reports",
            new { reason = "harassment", note = "Nội dung kiểm thử" });
        var retry = await second.PostAsJsonAsync(
            $"/api/v1/stranger/conversations/{conversationId}/reports",
            new { reason = "harassment", note = "Nội dung kiểm thử" });

        Assert.Equal(HttpStatusCode.OK, firstReport.StatusCode);
        Assert.Equal(HttpStatusCode.OK, retry.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await first.GetAsync("/api/v1/stranger/conversation")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await second.GetAsync("/api/v1/stranger/conversation")).StatusCode);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StrangerChatDbContext>();
        Assert.Equal(
            1,
            await db.Reports.CountAsync(report =>
                report.ConversationId == conversationId));
    }

    private static async Task CreateSessionAsync(HttpClient client, string nickname, string gender, string preference)
    {
        await PrimeAntiforgeryAsync(client);
        var response = await client.PostAsJsonAsync("/api/v1/stranger/session", new
        {
            nickname,
            gender,
            genderPreference = preference,
            confirmsAdult = true
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var metadata = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(metadata.TryGetProperty("id", out _));
        Assert.False(metadata.TryGetProperty("rawToken", out _));
    }

    private static async Task<string> QueueAndReadStatusAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/v1/stranger/matchmaking", new { });
        response.EnsureSuccessStatusCode();
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("status").GetString()!;
    }

    private static async Task PrimeAntiforgeryAsync(HttpClient client)
    {
        var response = await client.GetAsync("/api/v1/auth/antiforgery");
        response.EnsureSuccessStatusCode();
        var cookie = response.Headers.GetValues("Set-Cookie").Single(value => value.StartsWith("XSRF-TOKEN=", StringComparison.Ordinal));
        var token = Uri.UnescapeDataString(cookie.Split(';', 2)[0]["XSRF-TOKEN=".Length..]);
        client.DefaultRequestHeaders.Remove("X-XSRF-TOKEN");
        client.DefaultRequestHeaders.Add("X-XSRF-TOKEN", token);
    }
}
