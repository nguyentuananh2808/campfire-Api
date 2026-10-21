using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Campfire.IntegrationTests;

public sealed class EventsApiTests(CampfireApiFactory factory) : IClassFixture<CampfireApiFactory>
{
    [Fact]
    public async Task Event_permissions_concurrency_rsvp_lock_and_former_member_history_work_end_to_end()
    {
        using var owner = factory.CreateClient();
        using var member = factory.CreateClient();
        using var outsider = factory.CreateClient();
        await RegisterAsync(owner, "Chủ nhóm");
        await RegisterAsync(member, "Thành viên");
        await RegisterAsync(outsider, "Người ngoài");
        var groupResponse = await owner.PostAsJsonAsync("/api/v1/groups", new { name = "Nhóm lên kèo" });
        var groupId = GetGuid(await groupResponse.Content.ReadAsStringAsync(), "id");
        var invitation = await owner.PostAsJsonAsync($"/api/v1/groups/{groupId}/invitations", new { expiresInHours = 2 });
        var token = GetString(await invitation.Content.ReadAsStringAsync(), "token");
        await PrimeAntiforgeryAsync(member);
        Assert.Equal(HttpStatusCode.OK, (await member.PostAsJsonAsync("/api/v1/group-invitations/join", new { token })).StatusCode);

        var created = await owner.PostAsJsonAsync($"/api/v1/groups/{groupId}/events", new
        {
            title = "Cắm trại",
            description = "Mang theo áo ấm",
            startsAtUtc = DateTimeOffset.UtcNow.AddDays(5),
            location = "Đà Lạt"
        });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var createdJson = await created.Content.ReadAsStringAsync();
        var eventId = GetGuid(createdJson, "id");
        var version = GetGuid(createdJson, "version");
        Assert.Equal(HttpStatusCode.NotFound, (await outsider.GetAsync($"/api/v1/groups/{groupId}/events/{eventId}")).StatusCode);

        var memberEdit = await member.PatchAsJsonAsync($"/api/v1/groups/{groupId}/events/{eventId}", new
        {
            title = "Không được sửa",
            description = "",
            startsAtUtc = DateTimeOffset.UtcNow.AddDays(6),
            location = "Huế",
            expectedVersion = version
        });
        Assert.Equal(HttpStatusCode.Forbidden, memberEdit.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await member.PutAsJsonAsync($"/api/v1/groups/{groupId}/events/{eventId}/rsvp", new { response = "Going" })).StatusCode);

        var firstEdit = owner.PatchAsJsonAsync($"/api/v1/groups/{groupId}/events/{eventId}", new
        {
            title = "Cắm trại A",
            description = "",
            startsAtUtc = DateTimeOffset.UtcNow.AddDays(6),
            location = "Đà Lạt",
            expectedVersion = version
        });
        var secondEdit = owner.PatchAsJsonAsync($"/api/v1/groups/{groupId}/events/{eventId}", new
        {
            title = "Cắm trại B",
            description = "",
            startsAtUtc = DateTimeOffset.UtcNow.AddDays(7),
            location = "Đà Lạt",
            expectedVersion = version
        });
        var editResponses = await Task.WhenAll(firstEdit, secondEdit);
        Assert.Single(editResponses, response => response.StatusCode == HttpStatusCode.OK);
        Assert.Single(editResponses, response => response.StatusCode == HttpStatusCode.Conflict);

        var detail = await owner.GetAsync($"/api/v1/groups/{groupId}/events/{eventId}");
        version = GetGuid(await detail.Content.ReadAsStringAsync(), "version");
        Assert.Equal(HttpStatusCode.OK, (await owner.PostAsJsonAsync($"/api/v1/groups/{groupId}/events/{eventId}/status",
            new { status = "Confirmed", expectedVersion = version })).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await member.PutAsJsonAsync($"/api/v1/groups/{groupId}/events/{eventId}/rsvp",
            new { response = "NotGoing" })).StatusCode);

        Assert.Equal(HttpStatusCode.NoContent, (await member.DeleteAsync($"/api/v1/groups/{groupId}/members/me")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await member.GetAsync($"/api/v1/groups/{groupId}/events/{eventId}")).StatusCode);
        var historicalJson = await (await owner.GetAsync($"/api/v1/groups/{groupId}/events/{eventId}")).Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(historicalJson);
        var historical = document.RootElement.GetProperty("participants").EnumerateArray().Single();
        Assert.False(historical.GetProperty("isCurrentMember").GetBoolean());
        Assert.Equal("Going", historical.GetProperty("response").GetString());
    }

    private static async Task RegisterAsync(HttpClient client, string displayName)
    {
        await PrimeAntiforgeryAsync(client);
        (await client.PostAsJsonAsync("/api/v1/auth/register", new
        { email = $"{Guid.NewGuid():N}@example.com", password = "StrongPassword!1", displayName })).EnsureSuccessStatusCode();
    }

    private static async Task PrimeAntiforgeryAsync(HttpClient client)
    {
        var response = await client.GetAsync("/api/v1/auth/antiforgery"); response.EnsureSuccessStatusCode();
        var cookie = response.Headers.GetValues("Set-Cookie").Single(value => value.StartsWith("XSRF-TOKEN=", StringComparison.Ordinal));
        var token = Uri.UnescapeDataString(cookie.Split(';', 2)[0]["XSRF-TOKEN=".Length..]);
        client.DefaultRequestHeaders.Remove("X-XSRF-TOKEN"); client.DefaultRequestHeaders.Add("X-XSRF-TOKEN", token);
    }

    private static Guid GetGuid(string json, string propertyName)
    {
        using var document = JsonDocument.Parse(json); return document.RootElement.GetProperty(propertyName).GetGuid();
    }

    private static string GetString(string json, string propertyName)
    {
        using var document = JsonDocument.Parse(json); return document.RootElement.GetProperty(propertyName).GetString()!;
    }
}
