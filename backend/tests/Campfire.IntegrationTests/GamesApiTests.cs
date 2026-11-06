using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Campfire.IntegrationTests;

public sealed class GamesApiTests(CampfireApiFactory factory) : IClassFixture<CampfireApiFactory>
{
    [Fact]
    public async Task Two_members_can_play_while_outsiders_and_stale_moves_are_rejected()
    {
        using var owner = factory.CreateClient();
        using var member = factory.CreateClient();
        using var outsider = factory.CreateClient();
        var ownerId = await RegisterAsync(owner, "Chủ bàn");
        var memberId = await RegisterAsync(member, "Bạn chơi");
        await RegisterAsync(outsider, "Người ngoài");

        var groupResponse = await owner.PostAsJsonAsync("/api/v1/groups", new { name = "Nhóm chơi caro" });
        var groupId = GetGuid(await groupResponse.Content.ReadAsStringAsync(), "id");
        var invitation = await owner.PostAsJsonAsync($"/api/v1/groups/{groupId}/invitations", new { expiresInHours = 2 });
        var token = GetString(await invitation.Content.ReadAsStringAsync(), "token");
        await PrimeAntiforgeryAsync(member);
        Assert.Equal(HttpStatusCode.OK, (await member.PostAsJsonAsync("/api/v1/group-invitations/join", new { token })).StatusCode);

        var created = await owner.PostAsJsonAsync("/api/v1/games", new { groupId, opponentUserId = memberId });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var createdJson = await created.Content.ReadAsStringAsync();
        var gameId = GetGuid(createdJson, "id");
        var version = GetGuid(createdJson, "version");
        Assert.Equal(ownerId, GetGuid(createdJson, "currentPlayerUserId"));
        Assert.Equal(HttpStatusCode.NotFound, (await outsider.GetAsync($"/api/v1/games/{gameId}")).StatusCode);

        var firstMove = owner.PostAsJsonAsync($"/api/v1/games/{gameId}/moves", new { cell = 0, expectedVersion = version });
        var duplicateMove = owner.PostAsJsonAsync($"/api/v1/games/{gameId}/moves", new { cell = 1, expectedVersion = version });
        var results = await Task.WhenAll(firstMove, duplicateMove);
        Assert.Single(results, response => response.StatusCode == HttpStatusCode.OK);
        Assert.Single(results, response => response.StatusCode == HttpStatusCode.Conflict);

        var current = await member.GetAsync($"/api/v1/games/{gameId}");
        var currentJson = await current.Content.ReadAsStringAsync();
        Assert.Equal(memberId, GetGuid(currentJson, "currentPlayerUserId"));
        Assert.Equal(HttpStatusCode.OK, current.StatusCode);

        Assert.Equal(HttpStatusCode.NoContent, (await member.DeleteAsync($"/api/v1/groups/{groupId}/members/me")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await member.GetAsync($"/api/v1/games/{gameId}")).StatusCode);
    }

    private static async Task<Guid> RegisterAsync(HttpClient client, string displayName)
    {
        await PrimeAntiforgeryAsync(client);
        var response = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = $"{Guid.NewGuid():N}@example.com",
            password = "StrongPassword!1",
            displayName
        });
        response.EnsureSuccessStatusCode();
        return GetGuid(await response.Content.ReadAsStringAsync(), "id");
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

    private static Guid GetGuid(string json, string propertyName)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.GetProperty(propertyName).GetGuid();
    }

    private static string GetString(string json, string propertyName)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.GetProperty(propertyName).GetString()!;
    }
}
