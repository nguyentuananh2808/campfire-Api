using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Campfire.IntegrationTests;

public sealed class WerewolfApiTests(CampfireApiFactory factory) : IClassFixture<CampfireApiFactory>
{
    [Fact]
    public async Task Six_players_can_start_and_only_receive_their_own_private_role()
    {
        var clients = Enumerable.Range(1, 6).Select(_ => factory.CreateClient()).ToArray();
        using var outsider = factory.CreateClient();
        try
        {
            for (var index = 0; index < clients.Length; index++) await RegisterAsync(clients[index], $"Người chơi {index + 1}");
            await RegisterAsync(outsider, "Người ngoài");
            var created = await clients[0].PostAsJsonAsync("/api/v1/werewolf/rooms", new
            { name = "Ván thật", visibility = "Private", minimumPlayers = 6, maximumPlayers = 6, deadVoiceEnabled = true });
            Assert.Equal(HttpStatusCode.Created, created.StatusCode);
            using var createdJson = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
            var roomId = createdJson.RootElement.GetProperty("id").GetGuid();
            var code = createdJson.RootElement.GetProperty("code").GetString();
            for (var index = 1; index < clients.Length; index++)
            {
                var joined = await clients[index].PostAsJsonAsync("/api/v1/werewolf/rooms/join", new { code, spectator = false });
                Assert.Equal(HttpStatusCode.OK, joined.StatusCode);
            }
            Assert.Equal(HttpStatusCode.NotFound, (await outsider.GetAsync($"/api/v1/werewolf/rooms/{roomId}")).StatusCode);
            foreach (var client in clients) Assert.Equal(HttpStatusCode.OK, (await client.PutAsJsonAsync($"/api/v1/werewolf/rooms/{roomId}/ready", new { ready = true })).StatusCode);
            using var lobby = JsonDocument.Parse(await (await clients[0].GetAsync($"/api/v1/werewolf/rooms/{roomId}")).Content.ReadAsStringAsync());
            var version = lobby.RootElement.GetProperty("version").GetGuid();
            Assert.Equal(HttpStatusCode.OK, (await clients[0].PostAsJsonAsync($"/api/v1/werewolf/rooms/{roomId}/start", new { expectedVersion = version })).StatusCode);

            var roles = new List<string>();
            var userIds = new Guid[clients.Length];
            var gameId = Guid.Empty;
            for (var index = 0; index < clients.Length; index++)
            {
                using var snapshot = JsonDocument.Parse(await (await clients[index].GetAsync($"/api/v1/werewolf/rooms/{roomId}")).Content.ReadAsStringAsync());
                gameId = snapshot.RootElement.GetProperty("gameId").GetGuid();
                roles.Add(snapshot.RootElement.GetProperty("myRole").GetProperty("id").GetString()!);
                Assert.All(snapshot.RootElement.GetProperty("players").EnumerateArray(), player => Assert.False(player.TryGetProperty("role", out _)));
                userIds[index] = snapshot.RootElement.GetProperty("players").EnumerateArray()
                    .Single(player => player.GetProperty("displayName").GetString() == $"Người chơi {index + 1}")
                    .GetProperty("userId").GetGuid();
            }
            Assert.Equal(2, roles.Count(role => role == "Werewolf"));
            Assert.Contains("Seer", roles); Assert.Contains("Guard", roles); Assert.Contains("Witch", roles);

            var wolfIndex = roles.FindIndex(role => role == "Werewolf");
            var villageIndexes = roles.Select((role, index) => (role, index)).Where(item => item.role != "Werewolf").Select(item => item.index).ToArray();
            var wolfVoice = await clients[wolfIndex].PostAsJsonAsync($"/api/v1/werewolf/rooms/{roomId}/voice-token", new { });
            Assert.Equal(HttpStatusCode.OK, wolfVoice.StatusCode);
            using (var voiceJson = JsonDocument.Parse(await wolfVoice.Content.ReadAsStringAsync()))
                Assert.Equal("wolves", voiceJson.RootElement.GetProperty("channel").GetString());
            Assert.Equal(HttpStatusCode.ServiceUnavailable,
                (await clients[villageIndexes[0]].PostAsJsonAsync($"/api/v1/werewolf/rooms/{roomId}/voice-token", new { })).StatusCode);

            var clientActionId = Guid.NewGuid();
            var firstAction = new { clientActionId, kind = "WerewolfTarget", targetUserId = userIds[villageIndexes[0]] };
            var concurrentRetries = await Task.WhenAll(
                clients[wolfIndex].PostAsJsonAsync($"/api/v1/werewolf/games/{gameId}/actions", firstAction),
                clients[wolfIndex].PostAsJsonAsync($"/api/v1/werewolf/games/{gameId}/actions", firstAction));
            Assert.All(concurrentRetries, response => Assert.Equal(HttpStatusCode.OK, response.StatusCode));
            Assert.Equal(HttpStatusCode.Conflict,
                (await clients[wolfIndex].PostAsJsonAsync($"/api/v1/werewolf/games/{gameId}/actions",
                    new { clientActionId, kind = "WerewolfTarget", targetUserId = userIds[villageIndexes[1]] })).StatusCode);

            Assert.Equal(HttpStatusCode.OK, (await clients[0].DeleteAsync($"/api/v1/werewolf/rooms/{roomId}/players/me")).StatusCode);
            using var transferred = JsonDocument.Parse(await (await clients[1].GetAsync($"/api/v1/werewolf/rooms/{roomId}")).Content.ReadAsStringAsync());
            Assert.Equal(userIds[1], transferred.RootElement.GetProperty("hostUserId").GetGuid());
        }
        finally { foreach (var client in clients) client.Dispose(); }
    }

    [Fact]
    public async Task Six_players_can_complete_a_two_round_village_victory()
    {
        var clients = Enumerable.Range(1, 6).Select(_ => factory.CreateClient()).ToArray();
        try
        {
            for (var index = 0; index < clients.Length; index++) await RegisterAsync(clients[index], $"Ván đủ {index + 1}");
            var created = await clients[0].PostAsJsonAsync("/api/v1/werewolf/rooms", new
            { name = "Ván hoàn chỉnh", visibility = "Private", minimumPlayers = 6, maximumPlayers = 6, deadVoiceEnabled = true });
            Assert.Equal(HttpStatusCode.Created, created.StatusCode);
            using var createdJson = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
            var roomId = createdJson.RootElement.GetProperty("id").GetGuid();
            var code = createdJson.RootElement.GetProperty("code").GetString();
            for (var index = 1; index < clients.Length; index++)
                Assert.Equal(HttpStatusCode.OK, (await clients[index].PostAsJsonAsync("/api/v1/werewolf/rooms/join", new { code, spectator = false })).StatusCode);
            foreach (var client in clients)
                Assert.Equal(HttpStatusCode.OK, (await client.PutAsJsonAsync($"/api/v1/werewolf/rooms/{roomId}/ready", new { ready = true })).StatusCode);
            using var lobby = JsonDocument.Parse(await (await clients[0].GetAsync($"/api/v1/werewolf/rooms/{roomId}")).Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK,
                (await clients[0].PostAsJsonAsync($"/api/v1/werewolf/rooms/{roomId}/start", new { expectedVersion = lobby.RootElement.GetProperty("version").GetGuid() })).StatusCode);

            var roles = new string[clients.Length];
            var userIds = new Guid[clients.Length];
            var gameId = Guid.Empty;
            for (var index = 0; index < clients.Length; index++)
            {
                using var snapshot = JsonDocument.Parse(await (await clients[index].GetAsync($"/api/v1/werewolf/rooms/{roomId}")).Content.ReadAsStringAsync());
                gameId = snapshot.RootElement.GetProperty("gameId").GetGuid();
                roles[index] = snapshot.RootElement.GetProperty("myRole").GetProperty("id").GetString()!;
                userIds[index] = snapshot.RootElement.GetProperty("players").EnumerateArray()
                    .Single(player => player.GetProperty("displayName").GetString() == $"Ván đủ {index + 1}")
                    .GetProperty("userId").GetGuid();
            }

            var wolves = roles.Select((role, index) => (role, index)).Where(item => item.role == "Werewolf").Select(item => item.index).ToArray();
            var villager = Array.FindIndex(roles, role => role == "Villager");
            var seer = Array.FindIndex(roles, role => role == "Seer");
            var guard = Array.FindIndex(roles, role => role == "Guard");
            var witch = Array.FindIndex(roles, role => role == "Witch");

            foreach (var wolf in wolves)
                await SubmitActionAsync(clients[wolf], gameId, "WerewolfTarget", userIds[villager]);
            await SubmitActionAsync(clients[seer], gameId, "Investigate", userIds[wolves[0]]);
            await SubmitActionAsync(clients[guard], gameId, "Protect", userIds[seer]);
            await SubmitActionAsync(clients[witch], gameId, "WitchPass", null);

            await factory.AdvanceWerewolfAsync(TimeSpan.FromMinutes(4));
            foreach (var index in Enumerable.Range(0, clients.Length).Where(index => index != villager))
                await SubmitVoteAsync(clients[index], gameId, userIds[wolves[0]]);

            await SubmitActionAsync(clients[wolves[1]], gameId, "WerewolfTarget", userIds[seer]);
            await SubmitActionAsync(clients[seer], gameId, "Investigate", userIds[wolves[1]]);
            await SubmitActionAsync(clients[guard], gameId, "Protect", userIds[witch]);
            await SubmitActionAsync(clients[witch], gameId, "WitchPass", null);

            await factory.AdvanceWerewolfAsync(TimeSpan.FromMinutes(4));
            foreach (var index in new[] { wolves[1], guard, witch })
                await SubmitVoteAsync(clients[index], gameId, userIds[wolves[1]]);

            using var finished = JsonDocument.Parse(await (await clients[guard].GetAsync($"/api/v1/werewolf/rooms/{roomId}")).Content.ReadAsStringAsync());
            Assert.Equal("Finished", finished.RootElement.GetProperty("status").GetString());
            Assert.Equal("Finished", finished.RootElement.GetProperty("phase").GetString());
            Assert.Equal("Village", finished.RootElement.GetProperty("winner").GetString());
            Assert.Equal(2, finished.RootElement.GetProperty("round").GetInt32());
        }
        finally { foreach (var client in clients) client.Dispose(); }
    }

    private static async Task SubmitActionAsync(HttpClient client, Guid gameId, string kind, Guid? targetUserId)
    {
        var response = await client.PostAsJsonAsync($"/api/v1/werewolf/games/{gameId}/actions",
            new { clientActionId = Guid.NewGuid(), kind, targetUserId });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static async Task SubmitVoteAsync(HttpClient client, Guid gameId, Guid? targetUserId)
    {
        var response = await client.PostAsJsonAsync($"/api/v1/werewolf/games/{gameId}/votes",
            new { clientActionId = Guid.NewGuid(), targetUserId });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static async Task RegisterAsync(HttpClient client, string displayName)
    {
        var response = await client.GetAsync("/api/v1/auth/antiforgery"); response.EnsureSuccessStatusCode();
        var cookie = response.Headers.GetValues("Set-Cookie").Single(value => value.StartsWith("XSRF-TOKEN=", StringComparison.Ordinal));
        var token = Uri.UnescapeDataString(cookie.Split(';', 2)[0]["XSRF-TOKEN=".Length..]);
        client.DefaultRequestHeaders.Add("X-XSRF-TOKEN", token);
        (await client.PostAsJsonAsync("/api/v1/auth/register", new { email = $"{Guid.NewGuid():N}@example.com", password = "StrongPassword!1", displayName })).EnsureSuccessStatusCode();
    }
}
