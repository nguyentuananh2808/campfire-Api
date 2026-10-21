using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Campfire.Modules.Groups.Domain.Groups;
using Campfire.Modules.Groups.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Campfire.IntegrationTests;

public sealed class GroupsApiTests(CampfireApiFactory factory) : IClassFixture<CampfireApiFactory>
{
    [Fact]
    public async Task Invitation_preview_join_revoke_and_membership_authorization_work_end_to_end()
    {
        using var owner = factory.CreateClient();
        using var member = factory.CreateClient();
        using var outsider = factory.CreateClient();
        await RegisterAsync(owner, "Owner");
        var memberId = await RegisterAsync(member, "Member");
        await RegisterAsync(outsider, "Outsider");

        var group = await owner.PostAsJsonAsync("/api/v1/groups", new { name = "Lửa trại" });
        Assert.Equal(HttpStatusCode.Created, group.StatusCode);
        var groupId = GetGuid(await group.Content.ReadAsStringAsync(), "id");
        Assert.Equal(
            HttpStatusCode.NotFound,
            (await member.GetAsync($"/api/v1/groups/{groupId}/members")).StatusCode);

        var invitationResponse = await owner.PostAsJsonAsync(
            $"/api/v1/groups/{groupId}/invitations",
            new { expiresInHours = 24 });
        Assert.Equal(HttpStatusCode.Created, invitationResponse.StatusCode);
        var invitationJson = await invitationResponse.Content.ReadAsStringAsync();
        var invitationId = GetGuid(invitationJson, "id");
        var token = GetString(invitationJson, "token");

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<GroupsDbContext>();
            var invitation = await dbContext.Invitations.AsNoTracking().SingleAsync(i => i.Id == invitationId);
            Assert.Equal(32, invitation.TokenHash.Length);
            Assert.Equal(SHA256.HashData(Encoding.UTF8.GetBytes(token)), invitation.TokenHash);
        }

        await PrimeAntiforgeryAsync(member);
        var preview = await member.PostAsJsonAsync(
            "/api/v1/group-invitations/preview",
            new { token });
        Assert.Equal(HttpStatusCode.OK, preview.StatusCode);
        Assert.False(GetBoolean(await preview.Content.ReadAsStringAsync(), "alreadyMember"));

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<GroupsDbContext>();
            Assert.Equal(1, await dbContext.Memberships.CountAsync(m => m.GroupId == groupId));
        }

        Assert.Equal(
            HttpStatusCode.OK,
            (await member.PostAsJsonAsync("/api/v1/group-invitations/join", new { token })).StatusCode);
        Assert.Equal(
            HttpStatusCode.OK,
            (await member.PostAsJsonAsync("/api/v1/group-invitations/join", new { token })).StatusCode);

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<GroupsDbContext>();
            Assert.Equal(
                1,
                await dbContext.Memberships.CountAsync(
                    membership => membership.GroupId == groupId && membership.UserId == memberId));
        }

        Assert.Equal(
            HttpStatusCode.NoContent,
            (await owner.DeleteAsync($"/api/v1/groups/{groupId}/invitations/{invitationId}")).StatusCode);
        await PrimeAntiforgeryAsync(outsider);
        Assert.Equal(
            HttpStatusCode.NotFound,
            (await outsider.PostAsJsonAsync(
                "/api/v1/group-invitations/preview",
                new { token })).StatusCode);
        Assert.Equal(
            HttpStatusCode.Conflict,
            (await owner.DeleteAsync($"/api/v1/groups/{groupId}/members/me")).StatusCode);
    }

    [Fact]
    public async Task Concurrent_owner_transfers_allow_exactly_one_winner()
    {
        using var owner = factory.CreateClient();
        using var firstMember = factory.CreateClient();
        using var secondMember = factory.CreateClient();
        await RegisterAsync(owner, "Owner race");
        var firstMemberId = await RegisterAsync(firstMember, "First member");
        var secondMemberId = await RegisterAsync(secondMember, "Second member");

        var groupResponse = await owner.PostAsJsonAsync("/api/v1/groups", new { name = "Concurrency" });
        var groupJson = await groupResponse.Content.ReadAsStringAsync();
        var groupId = GetGuid(groupJson, "id");
        var version = GetString(groupJson, "version");
        var invitation = await owner.PostAsJsonAsync(
            $"/api/v1/groups/{groupId}/invitations",
            new { expiresInHours = 1 });
        var token = GetString(await invitation.Content.ReadAsStringAsync(), "token");
        await PrimeAntiforgeryAsync(firstMember);
        await PrimeAntiforgeryAsync(secondMember);
        Assert.Equal(
            HttpStatusCode.OK,
            (await firstMember.PostAsJsonAsync("/api/v1/group-invitations/join", new { token })).StatusCode);
        Assert.Equal(
            HttpStatusCode.OK,
            (await secondMember.PostAsJsonAsync("/api/v1/group-invitations/join", new { token })).StatusCode);

        var firstTransfer = owner.PostAsJsonAsync(
            $"/api/v1/groups/{groupId}/owner",
            new { newOwnerUserId = firstMemberId, expectedVersion = version });
        var secondTransfer = owner.PostAsJsonAsync(
            $"/api/v1/groups/{groupId}/owner",
            new { newOwnerUserId = secondMemberId, expectedVersion = version });
        var responses = await Task.WhenAll(firstTransfer, secondTransfer);

        Assert.Single(responses, response => response.StatusCode == HttpStatusCode.OK);
        Assert.Single(responses, response => response.StatusCode == HttpStatusCode.Conflict);
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GroupsDbContext>();
        Assert.Equal(
            1,
            await dbContext.Memberships.CountAsync(
                membership => membership.GroupId == groupId && membership.Role == GroupRole.Owner));
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

    private static Guid GetGuid(string json, string propertyName) =>
        Guid.Parse(GetString(json, propertyName));

    private static string GetString(string json, string propertyName)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.GetProperty(propertyName).GetString()!;
    }

    private static bool GetBoolean(string json, string propertyName)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.GetProperty(propertyName).GetBoolean();
    }
}
